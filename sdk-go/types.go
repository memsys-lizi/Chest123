package pan123

import (
	"context"
	"encoding/json"
	"io"
	"net/http"
	"time"
)

const (
	defaultBaseURL  = "https://open-api.123pan.com"
	defaultPlatform = "open_platform"
	defaultTimeout  = 30 * time.Second

	fileCheckingCode = 20103
)

// Options configures a Client.
type Options struct {
	ClientID       string
	ClientSecret   string
	AccessToken    string
	TokenExpiresAt time.Time
	BaseURL        string
	Platform       string
	Timeout        time.Duration
	HTTPClient     *http.Client
}

// RequestOptions configures one low-level SDK request.
type RequestOptions struct {
	Query   any
	Body    any
	Form    *MultipartForm
	Headers map[string]string
	BaseURL string
	NoAuth  bool
}

// MultipartForm describes a multipart/form-data request.
type MultipartForm struct {
	Fields map[string]any
	Files  []MultipartFile
}

// MultipartFile describes one uploaded multipart file field.
type MultipartFile struct {
	FieldName string
	FileName  string
	Reader    io.Reader
}

type pan123Response struct {
	Code    int             `json:"code"`
	Message string          `json:"message"`
	Data    json.RawMessage `json:"data"`
	TraceID string          `json:"x-traceID"`
}

type AccessTokenData struct {
	AccessToken string    `json:"accessToken"`
	ExpiredAt   time.Time `json:"expiredAt"`
}

type OAuthTokenRequest struct {
	ClientID     string `json:"client_id" url:"client_id"`
	ClientSecret string `json:"client_secret" url:"client_secret"`
	GrantType    string `json:"grant_type" url:"grant_type"`
	Code         string `json:"code,omitempty" url:"code,omitempty"`
	RefreshToken string `json:"refresh_token,omitempty" url:"refresh_token,omitempty"`
	RedirectURI  string `json:"redirect_uri,omitempty" url:"redirect_uri,omitempty"`
}

type OAuthTokenData struct {
	TokenType    string `json:"token_type"`
	AccessToken  string `json:"access_token"`
	RefreshToken string `json:"refresh_token"`
	ExpiresIn    int    `json:"expires_in"`
	Scope        string `json:"scope"`
}

type FileListRequest struct {
	ParentFileID int64  `json:"parentFileId" url:"parentFileId"`
	Limit        int    `json:"limit" url:"limit"`
	SearchData   string `json:"searchData,omitempty" url:"searchData,omitempty"`
	SearchMode   *int   `json:"searchMode,omitempty" url:"searchMode,omitempty"`
	LastFileID   *int64 `json:"lastFileId,omitempty" url:"lastFileId,omitempty"`
}

type FileListData struct {
	LastFileID int64      `json:"lastFileId"`
	FileList   []FileInfo `json:"fileList"`
}

type FileInfo struct {
	FileID       *int64 `json:"fileID,omitempty"`
	FileId       *int64 `json:"fileId,omitempty"`
	Filename     string `json:"filename"`
	Type         int    `json:"type"`
	Size         int64  `json:"size"`
	Etag         string `json:"etag,omitempty"`
	Status       *int   `json:"status,omitempty"`
	ParentFileID *int64 `json:"parentFileID,omitempty"`
	ParentFileId *int64 `json:"parentFileId,omitempty"`
	Trashed      *int   `json:"trashed,omitempty"`
}

type DownloadInfoRequest struct {
	FileId                int64         `json:"fileId" url:"fileId"`
	CheckingRetryAttempts int           `json:"-"`
	CheckingRetryDelay    time.Duration `json:"-"`
}

type DownloadInfoData struct {
	DownloadURL string `json:"downloadUrl"`
}

type UploadCreateRequest struct {
	ParentFileID int64  `json:"parentFileID"`
	Filename     string `json:"filename"`
	Etag         string `json:"etag"`
	Size         int64  `json:"size"`
	Duplicate    *int   `json:"duplicate,omitempty"`
	ContainDir   *bool  `json:"containDir,omitempty"`
}

type UploadCreateData struct {
	FileID      *int64   `json:"fileID,omitempty"`
	Reuse       bool     `json:"reuse"`
	PreuploadID string   `json:"preuploadID,omitempty"`
	SliceSize   int      `json:"sliceSize,omitempty"`
	Servers     []string `json:"servers,omitempty"`
}

type UploadCompleteData struct {
	Completed bool  `json:"completed"`
	FileID    int64 `json:"fileID"`
}

type UploadFileOptions struct {
	FilePath                  string
	ParentFileID              int64
	Filename                  string
	Duplicate                 *int
	ContainDir                *bool
	SingleUploadMaxBytes      int64
	SingleUploadRetryAttempts int
	SingleUploadRetryDelay    time.Duration
	CompletePollingAttempts   int
	CompletePollingDelay      time.Duration
	TransientRetryAttempts    int
	TransientRetryDelay       time.Duration
	OnProgress                func(UploadProgressEvent)
}

type UploadFileResult struct {
	FileID    int64
	Completed bool
	Reuse     bool
}

type UploadProgressEvent struct {
	Stage           string
	LoadedBytes     int64
	TotalBytes      int64
	Percent         float64
	SliceNo         int
	TotalSlices     int
	CompletedSlices int
	Attempt         int
}

type DirectLinkURLRequest struct {
	FileID                int64         `json:"fileID" url:"fileID"`
	CheckingRetryAttempts int           `json:"-"`
	CheckingRetryDelay    time.Duration `json:"-"`
}

type DirectLinkURLData struct {
	URL string `json:"url"`
}

type rawMap = map[string]any

type module struct {
	client *Client
}

func (m module) do(ctx context.Context, method, path string, opts RequestOptions, out any) error {
	return m.client.Do(ctx, method, path, opts, out)
}
