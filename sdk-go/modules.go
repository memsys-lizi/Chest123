package pan123

import "context"

type ShareModule struct {
	module
}

func (m *ShareModule) Create(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/share/create", params)
}

func (m *ShareModule) List(ctx context.Context, params any) (rawMap, error) {
	return queryMap(ctx, m.module, "GET", "/api/v1/share/list", params)
}

func (m *ShareModule) Update(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "PUT", "/api/v1/share/list/info", params)
}

func (m *ShareModule) CreatePaid(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/share/content-payment/create", params)
}

func (m *ShareModule) ListPaid(ctx context.Context, params any) (rawMap, error) {
	return queryMap(ctx, m.module, "GET", "/api/v1/share/payment/list", params)
}

func (m *ShareModule) UpdatePaid(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "PUT", "/api/v1/share/list/payment/info", params)
}

type OfflineModule struct {
	module
}

func (m *OfflineModule) CreateDownloadTask(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/offline/download", params)
}

func (m *OfflineModule) GetDownloadProcess(ctx context.Context, params any) (rawMap, error) {
	return queryMap(ctx, m.module, "GET", "/api/v1/offline/download/process", params)
}

type UserModule struct {
	module
}

func (m *UserModule) GetInfo(ctx context.Context) (rawMap, error) {
	var out rawMap
	err := m.do(ctx, "GET", "/api/v1/user/info", RequestOptions{}, &out)
	return out, err
}

type OssModule struct {
	module
}

func (m *OssModule) Mkdir(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/upload/v1/oss/file/mkdir", params)
}

func (m *OssModule) Create(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/upload/v1/oss/file/create", params)
}

func (m *OssModule) GetUploadURL(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/upload/v1/oss/file/get_upload_url", params)
}

func (m *OssModule) Complete(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/upload/v1/oss/file/upload_complete", params)
}

func (m *OssModule) AsyncResult(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/upload/v1/oss/file/upload_async_result", params)
}

func (m *OssModule) CreateCopyTask(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/oss/source/copy", params)
}

func (m *OssModule) GetCopyProcess(ctx context.Context, params any) (rawMap, error) {
	return queryMap(ctx, m.module, "GET", "/api/v1/oss/source/copy/process", params)
}

func (m *OssModule) GetCopyFailList(ctx context.Context, params any) (rawMap, error) {
	return queryMap(ctx, m.module, "GET", "/api/v1/oss/source/copy/fail", params)
}

func (m *OssModule) Move(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/oss/file/move", params)
}

func (m *OssModule) Delete(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/oss/file/delete", params)
}

func (m *OssModule) Detail(ctx context.Context, params any) (rawMap, error) {
	return queryMap(ctx, m.module, "GET", "/api/v1/oss/file/detail", params)
}

func (m *OssModule) List(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/oss/file/list", params)
}

func (m *OssModule) CreateOfflineMigration(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/oss/offline/download", params)
}

func (m *OssModule) GetOfflineMigration(ctx context.Context, params any) (rawMap, error) {
	return queryMap(ctx, m.module, "GET", "/api/v1/oss/offline/download/process", params)
}

type TranscodeModule struct {
	module
}

func (m *TranscodeModule) ListCloudDiskVideos(ctx context.Context, params FileListRequest) (*FileListData, error) {
	var out FileListData
	err := m.do(ctx, "GET", "/api/v2/file/list", RequestOptions{Query: params}, &out)
	return &out, err
}

func (m *TranscodeModule) UploadFromCloudDisk(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/upload/from_cloud_disk", params)
}

func (m *TranscodeModule) ListFiles(ctx context.Context, params FileListRequest) (*FileListData, error) {
	var out FileListData
	err := m.do(ctx, "GET", "/api/v2/file/list", RequestOptions{Query: params}, &out)
	return &out, err
}

func (m *TranscodeModule) FolderInfo(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/folder/info", params)
}

func (m *TranscodeModule) VideoResolutions(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/video/resolutions", params)
}

func (m *TranscodeModule) List(ctx context.Context, params any) (rawMap, error) {
	return queryMap(ctx, m.module, "GET", "/api/v1/video/transcode/list", params)
}

func (m *TranscodeModule) Transcode(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/video", params)
}

func (m *TranscodeModule) Record(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/video/record", params)
}

func (m *TranscodeModule) Result(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/video/result", params)
}

func (m *TranscodeModule) Delete(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/delete", params)
}

func (m *TranscodeModule) DownloadOriginal(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/file/download", params)
}

func (m *TranscodeModule) DownloadM3u8OrTs(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/m3u8_ts/download", params)
}

func (m *TranscodeModule) DownloadAll(ctx context.Context, params any) (rawMap, error) {
	return bodyMap(ctx, m.module, "POST", "/api/v1/transcode/file/download/all", params)
}

func bodyMap(ctx context.Context, m module, method, path string, params any) (rawMap, error) {
	var out rawMap
	err := m.do(ctx, method, path, RequestOptions{Body: params}, &out)
	return out, err
}

func queryMap(ctx context.Context, m module, method, path string, params any) (rawMap, error) {
	var out rawMap
	err := m.do(ctx, method, path, RequestOptions{Query: params}, &out)
	return out, err
}
