package pan123

import (
	"bytes"
	"context"
	"fmt"
	"io"
	"math"
	"os"
	"path/filepath"
	"time"
)

const singleUploadMaxBytes = int64(1024 * 1024 * 1024)

type UploadModule struct {
	module
}

func (m *UploadModule) Create(ctx context.Context, params UploadCreateRequest) (*UploadCreateData, error) {
	var out UploadCreateData
	err := m.do(ctx, "POST", "/upload/v2/file/create", RequestOptions{Body: params}, &out)
	return &out, err
}

func (m *UploadModule) Slice(ctx context.Context, uploadURL, preuploadID string, sliceNo int, sliceMD5 string, slice io.Reader, filename string) error {
	return m.do(ctx, "POST", "/upload/v2/file/slice", RequestOptions{
		BaseURL: uploadURL,
		Form: &MultipartForm{
			Fields: map[string]any{
				"preuploadID": preuploadID,
				"sliceNo":     sliceNo,
				"sliceMD5":    sliceMD5,
			},
			Files: []MultipartFile{{
				FieldName: "slice",
				FileName:  filename,
				Reader:    slice,
			}},
		},
	}, nil)
}

func (m *UploadModule) Complete(ctx context.Context, preuploadID string, attempts int, delay time.Duration) (*UploadCompleteData, error) {
	return retryWhileFileChecking(ctx, attempts, delay, func() (*UploadCompleteData, error) {
		return m.completeOnce(ctx, preuploadID)
	})
}

func (m *UploadModule) WaitComplete(ctx context.Context, preuploadID string, attempts int, delay time.Duration) (*UploadCompleteData, error) {
	return m.waitForUploadComplete(ctx, preuploadID, attempts, delay, 0, 0, 0, nil)
}

func (m *UploadModule) Domain(ctx context.Context) ([]string, error) {
	var out []string
	err := m.do(ctx, "GET", "/upload/v2/file/domain", RequestOptions{}, &out)
	return out, err
}

func (m *UploadModule) Single(ctx context.Context, uploadURL string, filePath string, params UploadCreateRequest) (*UploadCompleteData, error) {
	if uploadURL == "" {
		domains, err := m.Domain(ctx)
		if err != nil {
			return nil, err
		}
		if len(domains) == 0 {
			return nil, &APIError{Message: "No upload domain returned by /upload/v2/file/domain."}
		}
		uploadURL = domains[0]
	}
	result, err := m.singleUpload(ctx, uploadURL, filePath, params)
	if err != nil {
		return nil, err
	}
	if !isCompletedUpload(result) {
		return nil, &APIError{Message: "Single upload did not return a completed upload with a valid fileID."}
	}
	return result, nil
}

func (m *UploadModule) Sha1Reuse(ctx context.Context, params any) (*UploadCreateData, error) {
	var out UploadCreateData
	err := m.do(ctx, "POST", "/upload/v2/file/sha1_reuse", RequestOptions{Body: params}, &out)
	return &out, err
}

func (m *UploadModule) UploadFile(ctx context.Context, options UploadFileOptions) (UploadFileResult, error) {
	info, err := os.Stat(options.FilePath)
	if err != nil {
		return UploadFileResult{}, err
	}
	filename := options.Filename
	if filename == "" {
		filename = filepath.Base(options.FilePath)
	}
	emitProgress(options.OnProgress, UploadProgressEvent{Stage: "hashing", LoadedBytes: 0, TotalBytes: info.Size(), Percent: 0})
	etag, err := md5File(options.FilePath)
	if err != nil {
		return UploadFileResult{}, err
	}
	size := info.Size()
	emitProgress(options.OnProgress, UploadProgressEvent{Stage: "hashing", LoadedBytes: size, TotalBytes: size, Percent: 100})

	createRequest := UploadCreateRequest{
		ParentFileID: options.ParentFileID,
		Filename:     filename,
		Etag:         etag,
		Size:         size,
		Duplicate:    options.Duplicate,
		ContainDir:   options.ContainDir,
	}
	maxSingle := options.SingleUploadMaxBytes
	if maxSingle <= 0 {
		maxSingle = singleUploadMaxBytes
	}

	if size <= maxSingle {
		domains, err := m.Domain(ctx)
		if err != nil {
			return UploadFileResult{}, err
		}
		if len(domains) == 0 {
			return UploadFileResult{}, &APIError{Message: "No upload domain returned by /upload/v2/file/domain."}
		}
		completed, err := retryTransientUpload(ctx, options.TransientRetryAttempts, options.TransientRetryDelay, func() (*UploadCompleteData, error) {
			return m.singleUpload(ctx, domains[0], options.FilePath, createRequest)
		})
		if err != nil {
			return UploadFileResult{}, err
		}
		if !isCompletedUpload(completed) {
			return UploadFileResult{}, &APIError{Message: "Single upload did not return a completed upload with a valid fileID."}
		}
		emitProgress(options.OnProgress, UploadProgressEvent{Stage: "single", LoadedBytes: size, TotalBytes: size, Percent: 100})
		return UploadFileResult{FileID: completed.FileID, Completed: true}, nil
	}

	emitProgress(options.OnProgress, UploadProgressEvent{Stage: "create", LoadedBytes: 0, TotalBytes: size, Percent: 0})
	created, err := retryTransientUpload(ctx, options.TransientRetryAttempts, options.TransientRetryDelay, func() (*UploadCreateData, error) {
		return m.Create(ctx, createRequest)
	})
	if err != nil {
		return UploadFileResult{}, err
	}
	if created.Reuse {
		if created.FileID == nil || *created.FileID <= 0 {
			return UploadFileResult{}, &APIError{Message: "Upload create reported reuse but did not return a valid fileID."}
		}
		emitProgress(options.OnProgress, UploadProgressEvent{Stage: "reuse", LoadedBytes: size, TotalBytes: size, Percent: 100})
		return UploadFileResult{FileID: *created.FileID, Completed: true, Reuse: true}, nil
	}
	if created.PreuploadID == "" || created.SliceSize <= 0 || len(created.Servers) == 0 {
		return UploadFileResult{}, &APIError{Message: "Upload create did not return preuploadID, sliceSize, or servers."}
	}

	file, err := os.Open(options.FilePath)
	if err != nil {
		return UploadFileResult{}, err
	}
	defer file.Close()

	buffer := make([]byte, created.SliceSize)
	totalSlices := int(math.Ceil(float64(size) / float64(created.SliceSize)))
	for sliceNo := 1; ; sliceNo++ {
		n, readErr := io.ReadFull(file, buffer)
		if readErr == io.EOF {
			break
		}
		if readErr != nil && readErr != io.ErrUnexpectedEOF {
			return UploadFileResult{}, readErr
		}
		sliceBytes := append([]byte(nil), buffer[:n]...)
		err = retryTransientUploadNoValue(ctx, options.TransientRetryAttempts, options.TransientRetryDelay, func() error {
			return m.Slice(ctx, created.Servers[0], created.PreuploadID, sliceNo, md5Bytes(sliceBytes), bytes.NewReader(sliceBytes), fmt.Sprintf("%s.part%d", filename, sliceNo))
		})
		if err != nil {
			return UploadFileResult{}, err
		}
		loaded := int64(sliceNo * created.SliceSize)
		if loaded > size {
			loaded = size
		}
		percent := 100.0
		if size > 0 {
			percent = float64(loaded) * 100 / float64(size)
		}
		emitProgress(options.OnProgress, UploadProgressEvent{
			Stage:           "slice",
			LoadedBytes:     loaded,
			TotalBytes:      size,
			Percent:         percent,
			SliceNo:         sliceNo,
			TotalSlices:     totalSlices,
			CompletedSlices: sliceNo,
		})
		if readErr == io.ErrUnexpectedEOF {
			break
		}
	}

	completed, err := m.waitForUploadComplete(ctx, created.PreuploadID, options.CompletePollingAttempts, options.CompletePollingDelay, options.TransientRetryAttempts, options.TransientRetryDelay, size, options.OnProgress)
	if err != nil {
		return UploadFileResult{}, err
	}
	return UploadFileResult{FileID: completed.FileID, Completed: true}, nil
}

func (m *UploadModule) singleUpload(ctx context.Context, uploadURL string, filePath string, params UploadCreateRequest) (*UploadCompleteData, error) {
	file, err := os.Open(filePath)
	if err != nil {
		return nil, err
	}
	defer file.Close()

	var out UploadCompleteData
	err = m.do(ctx, "POST", "/upload/v2/file/single/create", RequestOptions{
		BaseURL: uploadURL,
		Form: &MultipartForm{
			Fields: map[string]any{
				"parentFileID": params.ParentFileID,
				"filename":     params.Filename,
				"etag":         params.Etag,
				"size":         params.Size,
				"duplicate":    params.Duplicate,
				"containDir":   params.ContainDir,
			},
			Files: []MultipartFile{{
				FieldName: "file",
				FileName:  params.Filename,
				Reader:    file,
			}},
		},
	}, &out)
	if err != nil {
		return nil, err
	}
	return &out, nil
}

func (m *UploadModule) completeOnce(ctx context.Context, preuploadID string) (*UploadCompleteData, error) {
	var out UploadCompleteData
	err := m.do(ctx, "POST", "/upload/v2/file/upload_complete", RequestOptions{Body: map[string]any{"preuploadID": preuploadID}}, &out)
	if err != nil {
		return nil, err
	}
	return &out, nil
}

func (m *UploadModule) waitForUploadComplete(ctx context.Context, preuploadID string, attempts int, delay time.Duration, transientAttempts int, transientDelay time.Duration, totalBytes int64, onProgress func(UploadProgressEvent)) (*UploadCompleteData, error) {
	attempts = positive(attempts, 60)
	delay = defaultDuration(delay, time.Second)
	var last *UploadCompleteData
	for attempt := 1; attempt <= attempts; attempt++ {
		completed, err := retryTransientUpload(ctx, transientAttempts, transientDelay, func() (*UploadCompleteData, error) {
			return m.completeOnce(ctx, preuploadID)
		})
		if err != nil {
			if !isFileCheckingError(err) || attempt == attempts {
				return nil, err
			}
		} else {
			last = completed
			if isCompletedUpload(completed) {
				emitProgress(onProgress, UploadProgressEvent{Stage: "complete", LoadedBytes: totalBytes, TotalBytes: totalBytes, Percent: 100, Attempt: attempt})
				return completed, nil
			}
		}
		emitProgress(onProgress, UploadProgressEvent{Stage: "complete", LoadedBytes: totalBytes, TotalBytes: totalBytes, Percent: 100, Attempt: attempt})
		if attempt < attempts {
			if err := sleepContext(ctx, delay); err != nil {
				return nil, err
			}
		}
	}
	return nil, &APIError{Message: fmt.Sprintf("Upload completion did not return completed=true with a valid fileID after %d polling attempts.", attempts), ResponseBody: fmt.Sprintf("%+v", last)}
}

func retryTransientUploadNoValue(ctx context.Context, attempts int, delay time.Duration, task func() error) error {
	_, err := retryTransientUpload(ctx, attempts, delay, func() (struct{}, error) {
		return struct{}{}, task()
	})
	return err
}

func isCompletedUpload(data *UploadCompleteData) bool {
	return data != nil && data.Completed && data.FileID > 0
}

func emitProgress(callback func(UploadProgressEvent), event UploadProgressEvent) {
	if callback != nil {
		callback(event)
	}
}
