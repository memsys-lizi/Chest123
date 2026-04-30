package pan123

import "context"

type DirectLinkModule struct {
	module
}

func (m *DirectLinkModule) Enable(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/direct-link/enable", params)
}

func (m *DirectLinkModule) Disable(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/direct-link/disable", params)
}

func (m *DirectLinkModule) URL(ctx context.Context, params DirectLinkURLRequest) (*DirectLinkURLData, error) {
	return retryWhileFileChecking(ctx, params.CheckingRetryAttempts, params.CheckingRetryDelay, func() (*DirectLinkURLData, error) {
		var out DirectLinkURLData
		err := m.do(ctx, "GET", "/api/v1/direct-link/url", RequestOptions{Query: params}, &out)
		if err != nil {
			return nil, err
		}
		return &out, nil
	})
}

func (m *DirectLinkModule) RefreshCache(ctx context.Context) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/direct-link/cache/refresh", nil)
}

func (m *DirectLinkModule) GetTrafficLogs(ctx context.Context, params any) (rawMap, error) {
	return m.queryMap(ctx, "GET", "/api/v1/direct-link/log", params)
}

func (m *DirectLinkModule) GetOfflineLogs(ctx context.Context, params any) (rawMap, error) {
	return m.queryMap(ctx, "GET", "/api/v1/direct-link/offline/logs", params)
}

func (m *DirectLinkModule) SetIPBlacklistEnabled(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/developer/config/forbide-ip/switch", params)
}

func (m *DirectLinkModule) UpdateIPBlacklist(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/developer/config/forbide-ip/update", params)
}

func (m *DirectLinkModule) ListIPBlacklist(ctx context.Context, params any) (rawMap, error) {
	return m.queryMap(ctx, "GET", "/api/v1/developer/config/forbide-ip/list", params)
}

func (m *DirectLinkModule) bodyMap(ctx context.Context, method, path string, params any) (rawMap, error) {
	var out rawMap
	err := m.do(ctx, method, path, RequestOptions{Body: params}, &out)
	return out, err
}

func (m *DirectLinkModule) queryMap(ctx context.Context, method, path string, params any) (rawMap, error) {
	var out rawMap
	err := m.do(ctx, method, path, RequestOptions{Query: params}, &out)
	return out, err
}
