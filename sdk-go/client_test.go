package pan123

import (
	"context"
	"encoding/json"
	"errors"
	"io"
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"strings"
	"testing"
	"time"
)

func TestClientFetchesCachesTokenAndAddsHeaders(t *testing.T) {
	var tokenCalls int
	var userCalls int
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/api/v1/access_token":
			tokenCalls++
			assertHeader(t, r, "Platform", "open_platform")
			writeOK(w, map[string]any{"accessToken": "token-1", "expiredAt": "2099-01-01T00:00:00+08:00"})
		case "/api/v1/user/info":
			userCalls++
			assertHeader(t, r, "Platform", "open_platform")
			assertHeader(t, r, "Authorization", "Bearer token-1")
			writeOK(w, map[string]any{"uid": 123})
		default:
			t.Fatalf("unexpected path: %s", r.URL.Path)
		}
	}))
	defer server.Close()

	client := NewClient(Options{BaseURL: server.URL, ClientID: "client", ClientSecret: "secret"})
	for i := 0; i < 2; i++ {
		if _, err := client.User.GetInfo(context.Background()); err != nil {
			t.Fatal(err)
		}
	}
	if tokenCalls != 1 {
		t.Fatalf("token calls = %d, want 1", tokenCalls)
	}
	if userCalls != 2 {
		t.Fatalf("user calls = %d, want 2", userCalls)
	}
}

func TestClientRefreshesExpiredToken(t *testing.T) {
	var sawFresh bool
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/api/v1/access_token":
			writeOK(w, map[string]any{"accessToken": "fresh-1", "expiredAt": "2099-01-01T00:00:00+08:00"})
		case "/api/v1/user/info":
			if r.Header.Get("Authorization") == "Bearer fresh-1" {
				sawFresh = true
			}
			writeOK(w, map[string]any{"uid": 123})
		default:
			t.Fatalf("unexpected path: %s", r.URL.Path)
		}
	}))
	defer server.Close()

	client := NewClient(Options{
		BaseURL:        server.URL,
		ClientID:       "client",
		ClientSecret:   "secret",
		AccessToken:    "old-token",
		TokenExpiresAt: time.Now().Add(-time.Hour),
	})
	if _, err := client.User.GetInfo(context.Background()); err != nil {
		t.Fatal(err)
	}
	if !sawFresh {
		t.Fatal("request did not use refreshed token")
	}
}

func TestClientRequiresCredentialsForAutomaticToken(t *testing.T) {
	client := NewClient(Options{})
	_, err := client.User.GetInfo(context.Background())
	if err == nil {
		t.Fatal("expected error")
	}
	var apiErr *APIError
	if !errors.As(err, &apiErr) || !strings.Contains(apiErr.Message, "ClientID") {
		t.Fatalf("unexpected error: %#v", err)
	}
}

func TestAPIErrorIncludesCodeTraceAndBody(t *testing.T) {
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		writeJSON(w, map[string]any{
			"code":      5066,
			"message":   "file not found",
			"data":      nil,
			"x-traceID": "trace-1",
		})
	}))
	defer server.Close()

	client := NewClient(Options{BaseURL: server.URL, AccessToken: "token"})
	_, err := client.Files.Detail(context.Background(), map[string]any{"fileID": 1})
	var apiErr *APIError
	if !errors.As(err, &apiErr) {
		t.Fatalf("expected APIError, got %T", err)
	}
	if apiErr.Code != 5066 || apiErr.TraceID != "trace-1" || !strings.Contains(apiErr.ResponseBody, "file not found") {
		t.Fatalf("unexpected APIError: %#v", apiErr)
	}
}

func TestQueryJSONAndMultipartSerialization(t *testing.T) {
	var sawList bool
	var sawRename bool
	var sawSlice bool
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/api/v2/file/list":
			sawList = true
			if r.URL.Query().Get("parentFileId") != "0" || r.URL.Query().Get("limit") != "100" {
				t.Fatalf("unexpected query: %s", r.URL.RawQuery)
			}
			writeOK(w, map[string]any{"lastFileId": 0, "fileList": []any{}})
		case "/api/v1/file/name":
			sawRename = true
			var body map[string]any
			mustDecode(t, r.Body, &body)
			if body["fileID"].(float64) != 123 || body["filename"] != "new.txt" {
				t.Fatalf("unexpected body: %#v", body)
			}
			writeOK(w, map[string]any{"ok": true})
		case "/upload/v2/file/slice":
			sawSlice = true
			if err := r.ParseMultipartForm(1 << 20); err != nil {
				t.Fatal(err)
			}
			if r.FormValue("preuploadID") != "pre-1" || r.FormValue("sliceNo") != "1" || r.FormValue("sliceMD5") != "md5" {
				t.Fatalf("unexpected multipart values: %#v", r.MultipartForm.Value)
			}
			file, _, err := r.FormFile("slice")
			if err != nil {
				t.Fatal(err)
			}
			defer file.Close()
			data, _ := io.ReadAll(file)
			if string(data) != "hello" {
				t.Fatalf("unexpected file data: %q", data)
			}
			writeOK(w, nil)
		default:
			t.Fatalf("unexpected path: %s", r.URL.Path)
		}
	}))
	defer server.Close()

	client := NewClient(Options{BaseURL: server.URL, AccessToken: "token"})
	if _, err := client.Files.List(context.Background(), FileListRequest{ParentFileID: 0, Limit: 100}); err != nil {
		t.Fatal(err)
	}
	if _, err := client.Files.Rename(context.Background(), map[string]any{"fileID": 123, "filename": "new.txt"}); err != nil {
		t.Fatal(err)
	}
	if err := client.Upload.Slice(context.Background(), server.URL, "pre-1", 1, "md5", strings.NewReader("hello"), "part1"); err != nil {
		t.Fatal(err)
	}
	if !sawList || !sawRename || !sawSlice {
		t.Fatalf("missing request: list=%v rename=%v slice=%v", sawList, sawRename, sawSlice)
	}
}

func TestFileCheckingRetries(t *testing.T) {
	var directCalls, downloadCalls, completeCalls int
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/api/v1/direct-link/url":
			directCalls++
			if directCalls == 1 {
				writeAPIError(w, fileCheckingCode, "文件正在校验中,请间隔1秒后再试")
				return
			}
			writeOK(w, map[string]any{"url": "https://example.com/file.txt"})
		case "/api/v1/file/download_info":
			downloadCalls++
			if downloadCalls == 1 {
				writeAPIError(w, fileCheckingCode, "文件正在校验中,请间隔1秒后再试")
				return
			}
			writeOK(w, map[string]any{"downloadUrl": "https://example.com/download.txt"})
		case "/upload/v2/file/upload_complete":
			completeCalls++
			if completeCalls == 1 {
				writeAPIError(w, fileCheckingCode, "文件正在校验中,请间隔1秒后再试")
				return
			}
			writeOK(w, map[string]any{"completed": true, "fileID": 1003})
		default:
			t.Fatalf("unexpected path: %s", r.URL.Path)
		}
	}))
	defer server.Close()

	client := NewClient(Options{BaseURL: server.URL, AccessToken: "token"})
	if _, err := client.DirectLink.URL(context.Background(), DirectLinkURLRequest{FileID: 1, CheckingRetryAttempts: 2, CheckingRetryDelay: time.Nanosecond}); err != nil {
		t.Fatal(err)
	}
	if _, err := client.Files.DownloadInfo(context.Background(), DownloadInfoRequest{FileId: 2, CheckingRetryAttempts: 2, CheckingRetryDelay: time.Nanosecond}); err != nil {
		t.Fatal(err)
	}
	if _, err := client.Upload.Complete(context.Background(), "pre-1", 2, time.Nanosecond); err != nil {
		t.Fatal(err)
	}
}

func TestUploadFileSmallUsesSingleUpload(t *testing.T) {
	filePath := tempFile(t, "hello")
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/upload/v2/file/domain":
			writeOK(w, []string{"http://" + r.Host})
		case "/upload/v2/file/single/create":
			if err := r.ParseMultipartForm(1 << 20); err != nil {
				t.Fatal(err)
			}
			if r.FormValue("parentFileID") != "0" || r.FormValue("size") != "5" || r.FormValue("duplicate") != "1" {
				t.Fatalf("unexpected form values: %#v", r.MultipartForm.Value)
			}
			writeOK(w, map[string]any{"completed": true, "fileID": 1001})
		default:
			t.Fatalf("unexpected path: %s", r.URL.Path)
		}
	}))
	defer server.Close()

	dup := 1
	client := NewClient(Options{BaseURL: server.URL, AccessToken: "token"})
	result, err := client.Upload.UploadFile(context.Background(), UploadFileOptions{
		FilePath:     filePath,
		ParentFileID: 0,
		Duplicate:    &dup,
	})
	if err != nil {
		t.Fatal(err)
	}
	if !result.Completed || result.FileID != 1001 {
		t.Fatalf("unexpected result: %#v", result)
	}
}

func TestUploadFileRejectsIncompleteSingleUpload(t *testing.T) {
	filePath := tempFile(t, "hello")
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/upload/v2/file/domain":
			writeOK(w, []string{"http://" + r.Host})
		case "/upload/v2/file/single/create":
			writeOK(w, map[string]any{"completed": false, "fileID": 0})
		default:
			t.Fatalf("unexpected path: %s", r.URL.Path)
		}
	}))
	defer server.Close()

	client := NewClient(Options{BaseURL: server.URL, AccessToken: "token"})
	_, err := client.Upload.UploadFile(context.Background(), UploadFileOptions{FilePath: filePath, ParentFileID: 0})
	var apiErr *APIError
	if !errors.As(err, &apiErr) || !strings.Contains(apiErr.Message, "Single upload") {
		t.Fatalf("unexpected error: %#v", err)
	}
}

func TestUploadFileRetriesTransientQueueError(t *testing.T) {
	filePath := tempFile(t, "hello")
	var singleCalls int
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/upload/v2/file/domain":
			writeOK(w, []string{"http://" + r.Host})
		case "/upload/v2/file/single/create":
			singleCalls++
			if singleCalls == 1 {
				writeAPIError(w, 1, "该任务已成功进入秒传队列,任务队列削峰中,未直接获取到文件ID,请慢一点")
				return
			}
			writeOK(w, map[string]any{"completed": true, "fileID": 4004})
		default:
			t.Fatalf("unexpected path: %s", r.URL.Path)
		}
	}))
	defer server.Close()

	client := NewClient(Options{BaseURL: server.URL, AccessToken: "token"})
	result, err := client.Upload.UploadFile(context.Background(), UploadFileOptions{
		FilePath:               filePath,
		ParentFileID:           0,
		TransientRetryAttempts: 2,
		TransientRetryDelay:    time.Nanosecond,
	})
	if err != nil {
		t.Fatal(err)
	}
	if result.FileID != 4004 || singleCalls != 2 {
		t.Fatalf("result=%#v singleCalls=%d", result, singleCalls)
	}
}

func TestUploadFileLargePollsCompletion(t *testing.T) {
	filePath := tempFile(t, "abcdef")
	var sliceCalls int
	var completeCalls int
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/upload/v2/file/create":
			writeOK(w, map[string]any{"reuse": false, "preuploadID": "pre-1", "sliceSize": 2, "servers": []string{"http://" + r.Host}})
		case "/upload/v2/file/slice":
			sliceCalls++
			writeOK(w, nil)
		case "/upload/v2/file/upload_complete":
			completeCalls++
			if completeCalls < 3 {
				writeOK(w, map[string]any{"completed": false, "fileID": 0})
				return
			}
			writeOK(w, map[string]any{"completed": true, "fileID": 3003})
		default:
			t.Fatalf("unexpected path: %s", r.URL.Path)
		}
	}))
	defer server.Close()

	var stages []string
	client := NewClient(Options{BaseURL: server.URL, AccessToken: "token"})
	result, err := client.Upload.UploadFile(context.Background(), UploadFileOptions{
		FilePath:                filePath,
		ParentFileID:            0,
		SingleUploadMaxBytes:    1,
		CompletePollingAttempts: 3,
		CompletePollingDelay:    time.Nanosecond,
		TransientRetryAttempts:  1,
		TransientRetryDelay:     time.Nanosecond,
		OnProgress: func(e UploadProgressEvent) {
			stages = append(stages, e.Stage)
		},
	})
	if err != nil {
		t.Fatal(err)
	}
	if result.FileID != 3003 || sliceCalls != 3 || completeCalls != 3 {
		t.Fatalf("result=%#v slices=%d completes=%d", result, sliceCalls, completeCalls)
	}
	if !containsStage(stages, "hashing") || !containsStage(stages, "slice") || !containsStage(stages, "complete") {
		t.Fatalf("missing progress stages: %#v", stages)
	}
}

func TestUploadFileLargeFailsWhenCompletionNeverReturnsFileID(t *testing.T) {
	filePath := tempFile(t, "abcdef")
	server := httptest.NewServer(http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		switch r.URL.Path {
		case "/upload/v2/file/create":
			writeOK(w, map[string]any{"reuse": false, "preuploadID": "pre-1", "sliceSize": 2, "servers": []string{"http://" + r.Host}})
		case "/upload/v2/file/slice":
			writeOK(w, nil)
		case "/upload/v2/file/upload_complete":
			writeOK(w, map[string]any{"completed": false, "fileID": 0})
		default:
			t.Fatalf("unexpected path: %s", r.URL.Path)
		}
	}))
	defer server.Close()

	client := NewClient(Options{BaseURL: server.URL, AccessToken: "token"})
	_, err := client.Upload.UploadFile(context.Background(), UploadFileOptions{
		FilePath:                filePath,
		ParentFileID:            0,
		SingleUploadMaxBytes:    1,
		CompletePollingAttempts: 2,
		CompletePollingDelay:    time.Nanosecond,
		TransientRetryAttempts:  1,
	})
	var apiErr *APIError
	if !errors.As(err, &apiErr) || !strings.Contains(apiErr.Message, "after 2 polling attempts") {
		t.Fatalf("unexpected error: %#v", err)
	}
}

func writeOK(w http.ResponseWriter, data any) {
	writeJSON(w, map[string]any{"code": 0, "message": "ok", "data": data, "x-traceID": "trace-ok"})
}

func writeAPIError(w http.ResponseWriter, code int, message string) {
	writeJSON(w, map[string]any{"code": code, "message": message, "data": nil, "x-traceID": "trace-error"})
}

func writeJSON(w http.ResponseWriter, value any) {
	w.Header().Set("Content-Type", "application/json")
	_ = json.NewEncoder(w).Encode(value)
}

func assertHeader(t *testing.T, r *http.Request, key, want string) {
	t.Helper()
	if got := r.Header.Get(key); got != want {
		t.Fatalf("%s = %q, want %q", key, got, want)
	}
}

func mustDecode(t *testing.T, reader io.Reader, out any) {
	t.Helper()
	if err := json.NewDecoder(reader).Decode(out); err != nil {
		t.Fatal(err)
	}
}

func tempFile(t *testing.T, content string) string {
	t.Helper()
	dir := t.TempDir()
	path := filepath.Join(dir, "test.txt")
	if err := os.WriteFile(path, []byte(content), 0o644); err != nil {
		t.Fatal(err)
	}
	return path
}

func containsStage(stages []string, want string) bool {
	for _, stage := range stages {
		if stage == want {
			return true
		}
	}
	return false
}
