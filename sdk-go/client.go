package pan123

import (
	"context"
	"net/http"
	"sync"
	"time"
)

// Client is the main entry point for the 123Pan SDK.
type Client struct {
	Auth       *AuthModule
	Files      *FilesModule
	Upload     *UploadModule
	UploadV1   *UploadV1Module
	Share      *ShareModule
	Offline    *OfflineModule
	User       *UserModule
	DirectLink *DirectLinkModule
	Oss        *OssModule
	Transcode  *TranscodeModule

	baseURL      string
	platform     string
	clientID     string
	clientSecret string
	httpClient   *http.Client

	mu                sync.Mutex
	accessToken       string
	tokenExpiresAt    time.Time
	refreshInProgress bool
}

// NewClient creates a 123Pan SDK client.
func NewClient(options Options) *Client {
	baseURL := options.BaseURL
	if baseURL == "" {
		baseURL = defaultBaseURL
	}
	platform := options.Platform
	if platform == "" {
		platform = defaultPlatform
	}
	timeout := options.Timeout
	if timeout <= 0 {
		timeout = defaultTimeout
	}
	httpClient := options.HTTPClient
	if httpClient == nil {
		httpClient = &http.Client{Timeout: timeout}
	} else if httpClient.Timeout == 0 {
		httpClient.Timeout = timeout
	}

	c := &Client{
		baseURL:        baseURL,
		platform:       platform,
		clientID:       options.ClientID,
		clientSecret:   options.ClientSecret,
		httpClient:     httpClient,
		accessToken:    options.AccessToken,
		tokenExpiresAt: options.TokenExpiresAt,
	}
	c.Auth = &AuthModule{module{client: c}}
	c.Files = &FilesModule{module{client: c}}
	c.Upload = &UploadModule{module{client: c}}
	c.UploadV1 = &UploadV1Module{module{client: c}}
	c.Share = &ShareModule{module{client: c}}
	c.Offline = &OfflineModule{module{client: c}}
	c.User = &UserModule{module{client: c}}
	c.DirectLink = &DirectLinkModule{module{client: c}}
	c.Oss = &OssModule{module{client: c}}
	c.Transcode = &TranscodeModule{module{client: c}}
	return c
}

// Do sends a low-level SDK request and decodes the official data field into out.
func (c *Client) Do(ctx context.Context, method, path string, opts RequestOptions, out any) error {
	return c.send(ctx, method, path, opts, out)
}

func (c *Client) hasUsableTokenLocked(now time.Time) bool {
	if c.accessToken == "" {
		return false
	}
	return c.tokenExpiresAt.IsZero() || now.Before(c.tokenExpiresAt.Add(-time.Minute))
}
