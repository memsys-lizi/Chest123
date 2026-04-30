package pan123

import (
	"context"
	"time"
)

type AuthModule struct {
	module
}

func (m *AuthModule) GetAccessToken(ctx context.Context) (*AccessTokenData, error) {
	return m.client.getAccessToken(ctx)
}

func (m *AuthModule) EnsureAccessToken(ctx context.Context) (string, error) {
	return m.client.ensureAccessToken(ctx)
}

func (m *AuthModule) SetAccessToken(token string, expiresAt time.Time) {
	m.client.mu.Lock()
	defer m.client.mu.Unlock()
	m.client.accessToken = token
	m.client.tokenExpiresAt = expiresAt
}

func (m *AuthModule) GetOAuthToken(ctx context.Context, request OAuthTokenRequest) (*OAuthTokenData, error) {
	var data OAuthTokenData
	err := m.do(ctx, "POST", "/api/v1/oauth2/access_token", RequestOptions{
		Query:  request,
		NoAuth: true,
	}, &data)
	if err != nil {
		return nil, err
	}
	return &data, nil
}

func (c *Client) ensureAccessToken(ctx context.Context) (string, error) {
	c.mu.Lock()
	defer c.mu.Unlock()
	if c.hasUsableTokenLocked(time.Now()) {
		return c.accessToken, nil
	}
	data, err := c.getAccessTokenLocked(ctx)
	if err != nil {
		return "", err
	}
	return data.AccessToken, nil
}

func (c *Client) getAccessToken(ctx context.Context) (*AccessTokenData, error) {
	c.mu.Lock()
	defer c.mu.Unlock()
	return c.getAccessTokenLocked(ctx)
}

func (c *Client) getAccessTokenLocked(ctx context.Context) (*AccessTokenData, error) {
	if c.clientID == "" || c.clientSecret == "" {
		return nil, &APIError{Message: "ClientID and ClientSecret are required to fetch access_token."}
	}
	var data AccessTokenData
	err := c.send(ctx, "POST", "/api/v1/access_token", RequestOptions{
		NoAuth: true,
		Body: map[string]any{
			"clientID":     c.clientID,
			"clientSecret": c.clientSecret,
		},
	}, &data)
	if err != nil {
		return nil, err
	}
	if data.AccessToken == "" {
		return nil, &APIError{Message: "The access_token response did not contain accessToken."}
	}
	c.accessToken = data.AccessToken
	c.tokenExpiresAt = data.ExpiredAt
	return &data, nil
}
