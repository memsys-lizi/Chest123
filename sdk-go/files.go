package pan123

import "context"

type FilesModule struct {
	module
}

func (m *FilesModule) Mkdir(ctx context.Context, params map[string]any) (rawMap, error) {
	var out rawMap
	err := m.do(ctx, "POST", "/upload/v1/file/mkdir", RequestOptions{Body: params}, &out)
	return out, err
}

func (m *FilesModule) Rename(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "PUT", "/api/v1/file/name", params)
}

func (m *FilesModule) BatchRename(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/file/rename", params)
}

func (m *FilesModule) Trash(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/file/trash", params)
}

func (m *FilesModule) Copy(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/file/copy", params)
}

func (m *FilesModule) AsyncCopy(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/file/async/copy", params)
}

func (m *FilesModule) AsyncCopyProcess(ctx context.Context, params any) (rawMap, error) {
	return m.queryMap(ctx, "GET", "/api/v1/file/async/copy/process", params)
}

func (m *FilesModule) Recover(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/file/recover", params)
}

func (m *FilesModule) RecoverByPath(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/file/recover/by_path", params)
}

func (m *FilesModule) Detail(ctx context.Context, params any) (rawMap, error) {
	return m.queryMap(ctx, "GET", "/api/v1/file/detail", params)
}

func (m *FilesModule) Infos(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/file/infos", params)
}

func (m *FilesModule) List(ctx context.Context, params FileListRequest) (*FileListData, error) {
	var out FileListData
	err := m.do(ctx, "GET", "/api/v2/file/list", RequestOptions{Query: params}, &out)
	return &out, err
}

func (m *FilesModule) ListLegacy(ctx context.Context, params any) (rawMap, error) {
	return m.queryMap(ctx, "GET", "/api/v1/file/list", params)
}

func (m *FilesModule) Move(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "POST", "/api/v1/file/move", params)
}

func (m *FilesModule) DownloadInfo(ctx context.Context, params DownloadInfoRequest) (*DownloadInfoData, error) {
	return retryWhileFileChecking(ctx, params.CheckingRetryAttempts, params.CheckingRetryDelay, func() (*DownloadInfoData, error) {
		var out DownloadInfoData
		err := m.do(ctx, "GET", "/api/v1/file/download_info", RequestOptions{Query: params}, &out)
		if err != nil {
			return nil, err
		}
		return &out, nil
	})
}

func (m *FilesModule) bodyMap(ctx context.Context, method, path string, params any) (rawMap, error) {
	var out rawMap
	err := m.do(ctx, method, path, RequestOptions{Body: params}, &out)
	return out, err
}

func (m *FilesModule) queryMap(ctx context.Context, method, path string, params any) (rawMap, error) {
	var out rawMap
	err := m.do(ctx, method, path, RequestOptions{Query: params}, &out)
	return out, err
}
