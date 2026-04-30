package pan123

import (
	"bytes"
	"context"
	"io"
	"net/http"
	"os"
	"path/filepath"
	"strconv"
	"testing"
	"time"
)

func TestLiveUploadDownloadAndCompareBytes(t *testing.T) {
	clientID := os.Getenv("PAN123_CLIENT_ID")
	clientSecret := os.Getenv("PAN123_CLIENT_SECRET")
	if clientID == "" || clientSecret == "" {
		t.Skip("PAN123_CLIENT_ID and PAN123_CLIENT_SECRET are required for live tests")
	}

	root := findRepositoryRoot(t)
	sourceFile := filepath.Join(root, "Test.txt")
	sourceBytes, err := os.ReadFile(sourceFile)
	if err != nil {
		t.Fatal(err)
	}
	parentFileID := int64(0)
	if text := os.Getenv("PAN123_PARENT_FILE_ID"); text != "" {
		if parsed, err := strconv.ParseInt(text, 10, 64); err == nil {
			parentFileID = parsed
		}
	}

	ctx := context.Background()
	client := NewClient(Options{
		ClientID:     clientID,
		ClientSecret: clientSecret,
		Timeout:      120 * time.Second,
	})

	token, err := client.Auth.EnsureAccessToken(ctx)
	if err != nil {
		t.Fatal(err)
	}
	if token == "" {
		t.Fatal("empty token")
	}
	if _, err := client.User.GetInfo(ctx); err != nil {
		t.Fatal(err)
	}
	if _, err := client.Files.List(ctx, FileListRequest{ParentFileID: parentFileID, Limit: 100}); err != nil {
		t.Fatal(err)
	}

	dup := 1
	upload, err := client.Upload.UploadFile(ctx, UploadFileOptions{
		FilePath:     sourceFile,
		ParentFileID: parentFileID,
		Filename:     "Test-sdk-live-go-" + time.Now().UTC().Format("20060102150405000") + ".txt",
		Duplicate:    &dup,
	})
	if err != nil {
		t.Fatal(err)
	}
	if !upload.Completed || upload.FileID <= 0 {
		t.Fatalf("unexpected upload result: %#v", upload)
	}
	if _, err := client.Files.Detail(ctx, map[string]any{"fileID": upload.FileID}); err != nil {
		t.Fatal(err)
	}
	download, err := client.Files.DownloadInfo(ctx, DownloadInfoRequest{FileId: upload.FileID})
	if err != nil {
		t.Fatal(err)
	}
	resp, err := http.Get(download.DownloadURL)
	if err != nil {
		t.Fatal(err)
	}
	defer resp.Body.Close()
	downloaded, err := io.ReadAll(resp.Body)
	if err != nil {
		t.Fatal(err)
	}
	if !bytes.Equal(sourceBytes, downloaded) {
		t.Fatal("downloaded bytes do not match source file")
	}
}

func findRepositoryRoot(t *testing.T) string {
	t.Helper()
	current, err := os.Getwd()
	if err != nil {
		t.Fatal(err)
	}
	for {
		if _, err := os.Stat(filepath.Join(current, "Test.txt")); err == nil {
			return current
		}
		parent := filepath.Dir(current)
		if parent == current {
			t.Fatal("could not locate repository root containing Test.txt")
		}
		current = parent
	}
}
