package pan123

import "context"

type UploadV1Module struct {
	module
}

func (m *UploadV1Module) Create(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "/upload/v1/file/create", params)
}

func (m *UploadV1Module) GetUploadURL(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "/upload/v1/file/get_upload_url", params)
}

func (m *UploadV1Module) ListUploadParts(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "/upload/v1/file/list_upload_parts", params)
}

func (m *UploadV1Module) Complete(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "/upload/v1/file/upload_complete", params)
}

func (m *UploadV1Module) AsyncResult(ctx context.Context, params any) (rawMap, error) {
	return m.bodyMap(ctx, "/upload/v1/file/upload_async_result", params)
}

func (m *UploadV1Module) bodyMap(ctx context.Context, path string, params any) (rawMap, error) {
	var out rawMap
	err := m.do(ctx, "POST", path, RequestOptions{Body: params}, &out)
	return out, err
}
