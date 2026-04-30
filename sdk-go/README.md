# pan123

123 云盘开放平台 Go SDK。本文档按 SDK 方法逐个说明参数、返回值和示例，不按业务流程混写。

## 安装与初始化

```bash
go get github.com/memsys-lizi/Chest123/sdk-go
```

```go
package main

import (
	"context"
	"fmt"
	"os"

	pan123 "github.com/memsys-lizi/Chest123/sdk-go"
)

func main() {
	client := pan123.NewClient(pan123.Options{
		ClientID:     os.Getenv("PAN123_CLIENT_ID"),
		ClientSecret: os.Getenv("PAN123_CLIENT_SECRET"),
	})

	files, err := client.Files.List(context.Background(), pan123.FileListRequest{
		ParentFileID: 0,
		Limit:        100,
	})
	if err != nil {
		panic(err)
	}
	fmt.Println(files.FileList)
}
```

Token 行为：

- SDK 会自动获取、缓存并维护 `access_token`。
- 调用需要鉴权的业务接口时，SDK 会先检查当前 token 是否可用。
- 如果没有 token，或 token 已过期/即将过期，SDK 会自动调用 `POST /api/v1/access_token` 获取新 token。
- 正常业务代码可以直接调用 `client.Files.List(...)`、`client.Upload.UploadFile(...)` 等方法，不需要手动先获取 token。
- `client.Auth.EnsureAccessToken(ctx)` 是可选方法，适合提前预热 token 或调试 token 状态。

## 客户端配置

| 字段 | 类型 | 必填 | 默认值 | 说明 |
| --- | --- | --- | --- | --- |
| `ClientID` | `string` | 否 | 无 | 123 云盘开放平台 `clientID`，自动获取 token 时必填 |
| `ClientSecret` | `string` | 否 | 无 | 123 云盘开放平台 `clientSecret`，自动获取 token 时必填 |
| `AccessToken` | `string` | 否 | 无 | 已有 access token |
| `TokenExpiresAt` | `time.Time` | 否 | 零值 | access token 过期时间 |
| `BaseURL` | `string` | 否 | `https://open-api.123pan.com` | API 基础地址 |
| `Platform` | `string` | 否 | `open_platform` | `Platform` 请求头 |
| `Timeout` | `time.Duration` | 否 | 30 秒 | HTTP 请求超时时间 |
| `HTTPClient` | `*http.Client` | 否 | 自动创建 | 自定义 HTTP 客户端 |

SDK 默认解析官方响应中的 `data` 字段。HTTP 错误或官方响应 `code != 0` 时返回 `*pan123.APIError`，其中包含 `Code`、`Message`、`TraceID`、`StatusCode` 和 `ResponseBody`。

## 传参约定

- 常用接口提供结构体，例如 `FileListRequest`、`DownloadInfoRequest`、`DirectLinkURLRequest`、`UploadCreateRequest`、`UploadFileOptions`。
- 官方字段复杂或容易变化的接口使用 `map[string]any` 或 `any` 薄封装。
- 匿名 map 的 key 就是发给官方的 JSON/query 字段名，例如 `map[string]any{"fileID": 123456}`。
- SDK 保留官方字段差异，不自动合并 `fileID/fileId`、`parentFileID/parentFileId`。
- 所有网络方法都接收 `context.Context`，调用方可以用 context 控制取消和超时。

## Auth

### `client.Auth.GetAccessToken(ctx)`

用途：使用客户端配置中的 `ClientID` 和 `ClientSecret` 获取开发者 `access_token`。

HTTP：`POST /api/v1/access_token`

参数：无。客户端初始化时必须配置 `ClientID` 和 `ClientSecret`。

返回：`*pan123.AccessTokenData`

| 字段 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `AccessToken` | `accessToken` | `string` | 访问凭证 |
| `ExpiredAt` | `expiredAt` | `time.Time` | 过期时间 |

示例：

```go
token, err := client.Auth.GetAccessToken(ctx)
fmt.Println(token.AccessToken, token.ExpiredAt)
```

注意：SDK 会自动缓存 token，普通业务代码通常不需要手动调用。

### `client.Auth.EnsureAccessToken(ctx)`

用途：确保当前客户端有可用 access token；没有或即将过期时自动获取。

HTTP：本地 helper，必要时调用 `POST /api/v1/access_token`。

参数：无。

返回：`string`，可用 access token。

示例：

```go
token, err := client.Auth.EnsureAccessToken(ctx)
```

注意：所有需要鉴权的 SDK 方法内部都会自动执行这个逻辑。除非你想提前获取 token，否则不需要在每次业务调用前手动调用。

### `client.Auth.SetAccessToken(token, expiresAt)`

用途：手动设置已有 access token。

HTTP：本地 helper。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `token` | `string` | 是 | access token |
| `expiresAt` | `time.Time` | 否 | token 过期时间；传零值表示不按时间过期 |

返回：无。

示例：

```go
client.Auth.SetAccessToken(existingToken, time.Now().Add(time.Hour))
```

### `client.Auth.GetOAuthToken(ctx, request)`

用途：OAuth 授权码或 refresh token 换取授权 token。

HTTP：`POST /api/v1/oauth2/access_token`

参数：`pan123.OAuthTokenRequest`

| 字段 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `ClientID` | `client_id` | `string` | 是 | OAuth 应用 appId |
| `ClientSecret` | `client_secret` | `string` | 是 | OAuth 应用 secretId |
| `GrantType` | `grant_type` | `string` | 是 | `authorization_code` 或 `refresh_token` |
| `Code` | `code` | `string` | 否 | 授权码，`authorization_code` 时使用 |
| `RefreshToken` | `refresh_token` | `string` | 否 | 刷新 token，`refresh_token` 时使用 |
| `RedirectURI` | `redirect_uri` | `string` | 否 | 授权回调地址 |

返回：`*pan123.OAuthTokenData`

| 字段 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `TokenType` | `token_type` | `string` | 通常为 `Bearer` |
| `AccessToken` | `access_token` | `string` | 授权 access token |
| `RefreshToken` | `refresh_token` | `string` | 新 refresh token |
| `ExpiresIn` | `expires_in` | `int` | access token 有效期，单位秒 |
| `Scope` | `scope` | `string` | 权限范围 |

示例：

```go
oauth, err := client.Auth.GetOAuthToken(ctx, pan123.OAuthTokenRequest{
	ClientID:     "app-id",
	ClientSecret: "secret-id",
	GrantType:    "authorization_code",
	Code:         "oauth-code",
	RedirectURI:  "https://example.com/callback",
})
```

## Files

### `client.Files.Mkdir(ctx, params)`

用途：创建目录。

HTTP：`POST /upload/v1/file/mkdir`

参数：`map[string]any`

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `name` | `string` | 是 | 目录名，不能重名 |
| `parentID` | `number` | 是 | 父目录 ID，根目录传 `0` |

返回：`map[string]any`，成功数据含 `dirID`。

示例：

```go
dir, err := client.Files.Mkdir(ctx, map[string]any{
	"name":     "SDK-Test",
	"parentID": 0,
})
```

### `client.Files.Rename(ctx, params)`

用途：单个文件或目录重命名。

HTTP：`PUT /api/v1/file/name`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `fileName` | `string` | 是 | 新文件名 |

返回：`map[string]any`，成功通常为空。

示例：

```go
_, err := client.Files.Rename(ctx, map[string]any{
	"fileId":   123456,
	"fileName": "new-name.txt",
})
```

### `client.Files.BatchRename(ctx, params)`

用途：批量重命名文件。

HTTP：`POST /api/v1/file/rename`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `renameList` | `array` | 是 | 重命名数组，按官方格式传文件 ID 和新文件名，最多 30 个 |

返回：`map[string]any`，成功数据含 `successList`、`failList`。

示例：

```go
result, err := client.Files.BatchRename(ctx, map[string]any{
	"renameList": []map[string]any{
		{"fileID": 123456, "filename": "new-name.txt"},
	},
})
```

### `client.Files.Trash(ctx, params)`

用途：删除文件至回收站。

HTTP：`POST /api/v1/file/trash`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |

返回：`map[string]any`，成功通常为空。

示例：

```go
_, err := client.Files.Trash(ctx, map[string]any{"fileIDs": []int64{123456}})
```

### `client.Files.Copy(ctx, params)`

用途：复制单个文件。

HTTP：`POST /api/v1/file/copy`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 源文件 ID |
| `targetDirId` | `number` | 是 | 目标目录 ID |

返回：`map[string]any`，成功数据含 `sourceFileId`、`targetFileId`。

示例：

```go
copied, err := client.Files.Copy(ctx, map[string]any{
	"fileId":      123456,
	"targetDirId": 0,
})
```

### `client.Files.AsyncCopy(ctx, params)`

用途：批量复制文件。

HTTP：`POST /api/v1/file/async/copy`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIds` | `number[]` | 是 | 文件 ID 数组 |
| `targetDirId` | `number` | 是 | 目标目录 ID |

返回：`map[string]any`，成功数据含 `taskId`。

示例：

```go
task, err := client.Files.AsyncCopy(ctx, map[string]any{
	"fileIds":     []int64{123456},
	"targetDirId": 0,
})
```

### `client.Files.AsyncCopyProcess(ctx, params)`

用途：查询批量复制任务进度。

HTTP：`GET /api/v1/file/async/copy/process`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `taskId` | `number` | 是 | 批量复制任务 ID |

返回：`map[string]any`，成功数据含 `taskId`、`status`。

示例：

```go
progress, err := client.Files.AsyncCopyProcess(ctx, map[string]any{"taskId": 2020})
```

### `client.Files.Recover(ctx, params)`

用途：从回收站恢复文件到删除前位置。

HTTP：`POST /api/v1/file/recover`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |

返回：`map[string]any`，成功数据含 `abnormalFileIDs`。

示例：

```go
_, err := client.Files.Recover(ctx, map[string]any{"fileIDs": []int64{123456}})
```

### `client.Files.RecoverByPath(ctx, params)`

用途：从回收站恢复文件到指定目录。

HTTP：`POST /api/v1/file/recover/by_path`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |
| `parentFileID` | `number` | 是 | 指定恢复目录 ID |

返回：`map[string]any`，成功通常为空。

示例：

```go
_, err := client.Files.RecoverByPath(ctx, map[string]any{
	"fileIDs":      []int64{123456},
	"parentFileID": 0,
})
```

### `client.Files.Detail(ctx, params)`

用途：获取单个文件详情。

HTTP：`GET /api/v1/file/detail`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileID` | `number` | 是 | 文件 ID |

返回：`map[string]any`，文件详情。

| 常见字段 | 类型 | 说明 |
| --- | --- | --- |
| `fileID` | `number` | 文件 ID |
| `filename` | `string` | 文件名 |
| `type` | `number` | `0` 文件，`1` 文件夹 |
| `size` | `number` | 文件大小 |
| `etag` | `string` | MD5 |
| `status` | `number` | 审核状态 |
| `parentFileID` | `number` | 父目录 ID |
| `trashed` | `number` | `0` 否，`1` 是 |

示例：

```go
detail, err := client.Files.Detail(ctx, map[string]any{"fileID": 123456})
```

### `client.Files.Infos(ctx, params)`

用途：获取多个文件详情。

HTTP：`POST /api/v1/file/infos`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIds` | `number[]` | 是 | 文件 ID 数组 |

返回：`map[string]any`，成功数据含 `fileList`。

示例：

```go
infos, err := client.Files.Infos(ctx, map[string]any{
	"fileIds": []int64{123456, 123457},
})
```

### `client.Files.List(ctx, params)`

用途：获取文件列表，推荐接口。

HTTP：`GET /api/v2/file/list`

参数：`pan123.FileListRequest`

| 字段 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `ParentFileID` | `parentFileId` | `int64` | 是 | 文件夹 ID，根目录传 `0` |
| `Limit` | `limit` | `int` | 是 | 每页数量，最大 100 |
| `SearchData` | `searchData` | `string` | 否 | 搜索关键字，传入后会全局搜索 |
| `SearchMode` | `searchMode` | `*int` | 否 | `0` 模糊搜索，`1` 精准搜索 |
| `LastFileID` | `lastFileId` | `*int64` | 否 | 翻页游标 |

返回：`*pan123.FileListData`

| 字段 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `LastFileID` | `lastFileId` | `int64` | `-1` 表示最后一页 |
| `FileList` | `fileList` | `[]pan123.FileInfo` | 文件列表 |

示例：

```go
list, err := client.Files.List(ctx, pan123.FileListRequest{
	ParentFileID: 0,
	Limit:        100,
})
```

注意：此接口可能返回回收站文件，请按 `Trashed` 字段过滤。

### `client.Files.ListLegacy(ctx, params)`

用途：获取文件列表，旧接口。

HTTP：`GET /api/v1/file/list`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `parentFileId` | `number` | 是 | 文件夹 ID，根目录传 `0` |
| `page` | `number` | 是 | 页码 |
| `limit` | `number` | 是 | 每页数量，最大 100 |
| `orderBy` | `string` | 是 | 排序字段，如 `file_id`、`size`、`file_name` |
| `orderDirection` | `string` | 是 | `asc` 或 `desc` |
| `trashed` | `boolean` | 否 | 是否查看回收站 |
| `searchData` | `string` | 否 | 搜索关键字 |

返回：`map[string]any`，成功数据含 `total`、`fileList`。

示例：

```go
list, err := client.Files.ListLegacy(ctx, map[string]any{
	"parentFileId":   0,
	"page":           1,
	"limit":          100,
	"orderBy":        "file_id",
	"orderDirection": "desc",
})
```

### `client.Files.Move(ctx, params)`

用途：移动文件。

HTTP：`POST /api/v1/file/move`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |
| `toParentFileID` | `number` | 是 | 目标目录 ID，根目录传 `0` |

返回：`map[string]any`，成功通常为空。

示例：

```go
_, err := client.Files.Move(ctx, map[string]any{
	"fileIDs":        []int64{123456},
	"toParentFileID": 0,
})
```

### `client.Files.DownloadInfo(ctx, params)`

用途：获取文件下载链接。

HTTP：`GET /api/v1/file/download_info`

参数：`pan123.DownloadInfoRequest`

| 字段 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `FileId` | `fileId` | `int64` | 是 | 文件 ID |
| `CheckingRetryAttempts` | 无 | `int` | 否 | `20103 文件正在校验中` 重试次数，默认 60 |
| `CheckingRetryDelay` | 无 | `time.Duration` | 否 | `20103` 重试间隔，默认 1 秒 |

返回：`*pan123.DownloadInfoData`

| 字段 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `DownloadURL` | `downloadUrl` | `string` | 下载地址 |

示例：

```go
info, err := client.Files.DownloadInfo(ctx, pan123.DownloadInfoRequest{
	FileId:                123456,
	CheckingRetryAttempts: 60,
	CheckingRetryDelay:    time.Second,
})
```

## Upload V2

### `client.Upload.UploadFile(ctx, options)`

用途：上传本地文件。SDK 自动计算文件 MD5，自动选择 V2 单步上传或分片上传。

HTTP：高层 helper，内部会调用 V2 上传接口。

参数：`pan123.UploadFileOptions`

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `FilePath` | `string` | 是 | 本地文件路径 |
| `ParentFileID` | `int64` | 是 | 云盘父目录 ID，根目录传 `0` |
| `Filename` | `string` | 否 | 云盘文件名，不传则取本地文件名 |
| `Duplicate` | `*int` | 否 | `1` 保留两者，`2` 覆盖原文件 |
| `ContainDir` | `*bool` | 否 | 文件名是否包含路径 |
| `SingleUploadMaxBytes` | `int64` | 否 | 单步上传阈值，默认 1GB |
| `SingleUploadRetryAttempts` | `int` | 否 | 保留字段，首版高层单步上传使用临时错误重试配置 |
| `SingleUploadRetryDelay` | `time.Duration` | 否 | 保留字段 |
| `CompletePollingAttempts` | `int` | 否 | 分片上传完成轮询次数，默认 60 |
| `CompletePollingDelay` | `time.Duration` | 否 | 分片上传完成轮询间隔，默认 1 秒 |
| `TransientRetryAttempts` | `int` | 否 | 上传削峰、限流等临时错误重试次数，默认 5 |
| `TransientRetryDelay` | `time.Duration` | 否 | 临时错误重试间隔，默认 1 秒 |
| `OnProgress` | `func(UploadProgressEvent)` | 否 | 上传进度回调 |

返回：`pan123.UploadFileResult`

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `FileID` | `int64` | 上传后的文件 ID |
| `Completed` | `bool` | 是否上传完成 |
| `Reuse` | `bool` | 是否秒传 |

注意：`UploadFile` 只有拿到 `completed=true` 且 `fileID>0` 才会返回成功。若 123 云盘返回 `completed=false`、`fileID=0`、文件检测中超时或业务错误，SDK 会返回 `*pan123.APIError`，避免调用方误以为上传已完成。

示例：

```go
dup := 1
result, err := client.Upload.UploadFile(ctx, pan123.UploadFileOptions{
	FilePath:     "./Test.txt",
	ParentFileID: 0,
	Duplicate:    &dup,
	OnProgress: func(event pan123.UploadProgressEvent) {
		fmt.Println(event.Stage, event.Percent)
	},
})
```

### `client.Upload.Create(ctx, params)`

用途：V2 创建文件，获取预上传信息或秒传结果。

HTTP：`POST /upload/v2/file/create`

参数：`pan123.UploadCreateRequest`

| 字段 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `ParentFileID` | `parentFileID` | `int64` | 是 | 父目录 ID，根目录传 `0` |
| `Filename` | `filename` | `string` | 是 | 文件名 |
| `Etag` | `etag` | `string` | 是 | 文件 MD5 |
| `Size` | `size` | `int64` | 是 | 文件大小，单位 byte |
| `Duplicate` | `duplicate` | `*int` | 否 | `1` 保留两者，`2` 覆盖原文件 |
| `ContainDir` | `containDir` | `*bool` | 否 | 文件名是否包含路径 |

返回：`*pan123.UploadCreateData`

| 字段 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `Reuse` | `reuse` | `bool` | 是否秒传 |
| `FileID` | `fileID` | `*int64` | 秒传成功时的文件 ID |
| `PreuploadID` | `preuploadID` | `string` | 预上传 ID |
| `SliceSize` | `sliceSize` | `int` | 分片大小 |
| `Servers` | `servers` | `[]string` | 上传域名 |

示例：

```go
created, err := client.Upload.Create(ctx, pan123.UploadCreateRequest{
	ParentFileID: 0,
	Filename:     "large.bin",
	Etag:         "file-md5",
	Size:         1024,
})
```

### `client.Upload.Slice(ctx, uploadURL, preuploadID, sliceNo, sliceMD5, slice, filename)`

用途：V2 上传一个分片。

HTTP：`POST /upload/v2/file/slice`，基础域名使用 `uploadURL`。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `uploadURL` | `string` | 是 | 上传域名，来自 `Create` 返回的 `Servers` |
| `preuploadID` | `string` | 是 | 预上传 ID |
| `sliceNo` | `int` | 是 | 分片序号，从 1 开始 |
| `sliceMD5` | `string` | 是 | 当前分片 MD5 |
| `slice` | `io.Reader` | 是 | 分片内容 |
| `filename` | `string` | 是 | multipart 文件名 |

返回：`error`，成功为 `nil`。

示例：

```go
err := client.Upload.Slice(ctx, uploadURL, "preupload-id", 1, "slice-md5", bytes.NewReader(part), "large.bin.part1")
```

### `client.Upload.Complete(ctx, preuploadID, attempts, delay)`

用途：V2 通知上传完成，并查询合并结果。此方法会在官方返回 `20103 文件正在校验中` 时重试。

HTTP：`POST /upload/v2/file/upload_complete`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `preuploadID` | `string` | 是 | 预上传 ID |
| `attempts` | `int` | 否 | `20103` 重试次数，传 `0` 使用默认 60 |
| `delay` | `time.Duration` | 否 | 重试间隔，传 `0` 使用默认 1 秒 |

返回：`*pan123.UploadCompleteData`

| 字段 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `Completed` | `completed` | `bool` | 是否完成 |
| `FileID` | `fileID` | `int64` | 完成后的文件 ID |

示例：

```go
completed, err := client.Upload.Complete(ctx, "preupload-id", 60, time.Second)
```

### `client.Upload.WaitComplete(ctx, preuploadID, attempts, delay)`

用途：轮询 V2 上传完成结果，直到 `completed=true` 且 `fileID>0`。

HTTP：`POST /upload/v2/file/upload_complete`

参数：同 `Complete`。

返回：`*pan123.UploadCompleteData`。若耗尽轮询次数仍未得到有效 `fileID`，返回 `*pan123.APIError`。

示例：

```go
completed, err := client.Upload.WaitComplete(ctx, "preupload-id", 60, time.Second)
```

### `client.Upload.Domain(ctx)`

用途：获取 V2 单步上传域名。

HTTP：`GET /upload/v2/file/domain`

参数：无。

返回：`[]string`，上传域名列表。

示例：

```go
domains, err := client.Upload.Domain(ctx)
```

### `client.Upload.Single(ctx, uploadURL, filePath, params)`

用途：V2 单步上传小文件。

HTTP：`POST /upload/v2/file/single/create`，基础域名使用 `uploadURL`；如果 `uploadURL` 为空，SDK 会自动调用 `Domain` 获取上传域名。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `uploadURL` | `string` | 否 | 上传域名，不传则自动获取 |
| `filePath` | `string` | 是 | 本地文件路径 |
| `params` | `pan123.UploadCreateRequest` | 是 | `ParentFileID`、`Filename`、`Etag`、`Size` 等 |

返回：`*pan123.UploadCompleteData`。

示例：

```go
result, err := client.Upload.Single(ctx, "", "./Test.txt", pan123.UploadCreateRequest{
	ParentFileID: 0,
	Filename:     "Test.txt",
	Etag:         "file-md5",
	Size:         123,
})
```

注意：官方限制单步上传最大 1GB。

### `client.Upload.Sha1Reuse(ctx, params)`

用途：按 SHA1 尝试文件秒传。

HTTP：`POST /upload/v2/file/sha1_reuse`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `parentFileID` | `number` | 是 | 父目录 ID |
| `filename` | `string` | 是 | 文件名 |
| `sha1` | `string` | 是 | 文件 SHA1 |
| `size` | `number` | 是 | 文件大小 |
| `duplicate` | `number` | 否 | 同名策略 |

返回：`*pan123.UploadCreateData`，含 `Reuse`、`FileID`。

示例：

```go
reuse, err := client.Upload.Sha1Reuse(ctx, map[string]any{
	"parentFileID": 0,
	"filename":     "file.bin",
	"sha1":         "sha1",
	"size":         1024,
})
```

## Upload V1

### `client.UploadV1.Create(ctx, params)`

用途：V1 创建文件。

HTTP：`POST /upload/v1/file/create`

参数：`any`，常用 `map[string]any`。包含 `parentFileID`、`filename`、`etag`、`size`，可选 `duplicate`、`containDir`。

返回：`map[string]any`，含 `reuse`、`fileID`、`preuploadID`、`sliceSize`。

示例：

```go
created, err := client.UploadV1.Create(ctx, map[string]any{
	"parentFileID": 0,
	"filename":     "file.bin",
	"etag":         "md5",
	"size":         1024,
})
```

### `client.UploadV1.GetUploadURL(ctx, params)`

用途：获取 V1 分片上传地址。

HTTP：`POST /upload/v1/file/get_upload_url`

参数：`any`，常用 `map[string]any`。包含 `preuploadID`、`sliceNo`。

返回：`map[string]any`，含 `presignedURL`。

示例：

```go
uploadURL, err := client.UploadV1.GetUploadURL(ctx, map[string]any{
	"preuploadID": "pre-id",
	"sliceNo":     1,
})
```

### `client.UploadV1.ListUploadParts(ctx, params)`

用途：列举 V1 已上传分片。

HTTP：`POST /upload/v1/file/list_upload_parts`

参数：`any`，常用 `map[string]any`。包含 `preuploadID`。

返回：`map[string]any`，含 `parts`。

示例：

```go
parts, err := client.UploadV1.ListUploadParts(ctx, map[string]any{"preuploadID": "pre-id"})
```

### `client.UploadV1.Complete(ctx, params)`

用途：V1 通知上传完成。

HTTP：`POST /upload/v1/file/upload_complete`

参数：`any`，常用 `map[string]any`。包含 `preuploadID`。

返回：`map[string]any`，含 `async`、`completed`、`fileID`。

示例：

```go
completed, err := client.UploadV1.Complete(ctx, map[string]any{"preuploadID": "pre-id"})
```

### `client.UploadV1.AsyncResult(ctx, params)`

用途：V1 异步轮询上传结果。

HTTP：`POST /upload/v1/file/upload_async_result`

参数：`any`，常用 `map[string]any`。包含 `preuploadID`。

返回：`map[string]any`，含 `completed`、`fileID`。

示例：

```go
result, err := client.UploadV1.AsyncResult(ctx, map[string]any{"preuploadID": "pre-id"})
```

## Share

### `client.Share.Create(ctx, params)`

用途：创建普通分享链接。

HTTP：`POST /api/v1/share/create`

参数：`any`，常用 `map[string]any`。包含 `shareName`、`shareExpire`、`fileIDList`，可选 `sharePwd`、`trafficSwitch`、`trafficLimitSwitch`、`trafficLimit`。

返回：`map[string]any`，含 `shareID`、`shareKey`。

示例：

```go
share, err := client.Share.Create(ctx, map[string]any{
	"shareName":   "share",
	"shareExpire": 7,
	"fileIDList":  "123456",
})
```

### `client.Share.List(ctx, params)`

用途：获取普通分享列表。

HTTP：`GET /api/v1/share/list`

参数：`any`，常用 `map[string]any`。包含 `limit`，可选 `lastShareId`。

返回：`map[string]any`，含 `shareList`、`lastShareId`。

示例：

```go
list, err := client.Share.List(ctx, map[string]any{"limit": 100})
```

### `client.Share.Update(ctx, params)`

用途：修改普通分享链接配置。

HTTP：`PUT /api/v1/share/list/info`

参数：`any`，常用 `map[string]any`。包含 `shareIdList`，可选 `trafficSwitch`、`trafficLimitSwitch`、`trafficLimit`。

返回：`map[string]any`，成功通常为空。

示例：

```go
_, err := client.Share.Update(ctx, map[string]any{
	"shareIdList":        []int64{123456},
	"trafficLimitSwitch": 1,
})
```

### `client.Share.CreatePaid(ctx, params)`

用途：创建付费分享链接。

HTTP：`POST /api/v1/share/content-payment/create`

参数：`any`，常用 `map[string]any`。包含 `shareName`、`fileIDList`、`payAmount`，可选 `isReward`、`resourceDesc`、`trafficSwitch`、`trafficLimitSwitch`、`trafficLimit`。

返回：`map[string]any`，含 `shareID`、`shareKey`。

示例：

```go
paid, err := client.Share.CreatePaid(ctx, map[string]any{
	"shareName":  "paid",
	"fileIDList": "123456",
	"payAmount":  10,
})
```

### `client.Share.ListPaid(ctx, params)`

用途：获取付费分享列表。

HTTP：`GET /api/v1/share/payment/list`

参数：同 `client.Share.List(ctx, params)`。

返回：`map[string]any`，含 `shareList`、`lastShareId`。

示例：

```go
paidList, err := client.Share.ListPaid(ctx, map[string]any{"limit": 100})
```

### `client.Share.UpdatePaid(ctx, params)`

用途：修改付费分享链接配置。

HTTP：`PUT /api/v1/share/list/payment/info`

参数：同 `client.Share.Update(ctx, params)`。

返回：`map[string]any`，成功通常为空。

示例：

```go
_, err := client.Share.UpdatePaid(ctx, map[string]any{
	"shareIdList":        []int64{123456},
	"trafficLimitSwitch": 1,
})
```

## Offline

### `client.Offline.CreateDownloadTask(ctx, params)`

用途：创建离线下载任务。

HTTP：`POST /api/v1/offline/download`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `url` | `string` | 是 | http/https 下载资源地址 |
| `fileName` | `string` | 否 | 自定义文件名 |
| `dirID` | `number` | 否 | 指定下载目录；官方说明不支持根目录 |
| `callBackUrl` | `string` | 否 | 下载成功或失败后的回调地址 |

返回：`map[string]any`，含 `taskID`。

示例：

```go
task, err := client.Offline.CreateDownloadTask(ctx, map[string]any{
	"url": "https://example.com/file.zip",
})
```

### `client.Offline.GetDownloadProcess(ctx, params)`

用途：查询离线下载进度。

HTTP：`GET /api/v1/offline/download/process`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `taskID` | `number` | 是 | 离线下载任务 ID |

返回：`map[string]any`，含 `process`、`status`。

示例：

```go
progress, err := client.Offline.GetDownloadProcess(ctx, map[string]any{"taskID": 394756})
```

## User

### `client.User.GetInfo(ctx)`

用途：获取用户信息。

HTTP：`GET /api/v1/user/info`

参数：无。

返回：`map[string]any`，用户信息。

| 常见字段 | 类型 | 说明 |
| --- | --- | --- |
| `uid` | `number` | 用户 ID |
| `nickname` | `string` | 昵称 |
| `spaceUsed` | `number` | 已用空间 |
| `spacePermanent` | `number` | 永久空间 |
| `vip` | `boolean` | 是否会员 |
| `directTraffic` | `number` | 剩余直链流量 |
| `developerInfo` | `object` | 开发者权益信息 |

示例：

```go
user, err := client.User.GetInfo(ctx)
```

## DirectLink

### `client.DirectLink.Enable(ctx, params)`

用途：启用直链空间。

HTTP：`POST /api/v1/direct-link/enable`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileID` | `number` | 是 | 要启用直链空间的文件夹 ID |

返回：`map[string]any`，含 `filename`。

示例：

```go
_, err := client.DirectLink.Enable(ctx, map[string]any{"fileID": 123456})
```

### `client.DirectLink.Disable(ctx, params)`

用途：禁用直链空间。

HTTP：`POST /api/v1/direct-link/disable`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileID` | `number` | 是 | 要禁用直链空间的文件夹 ID |

返回：`map[string]any`，含 `filename`。

示例：

```go
_, err := client.DirectLink.Disable(ctx, map[string]any{"fileID": 123456})
```

### `client.DirectLink.URL(ctx, params)`

用途：获取文件直链 URL。

HTTP：`GET /api/v1/direct-link/url`

参数：`pan123.DirectLinkURLRequest`

| 字段 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `FileID` | `fileID` | `int64` | 是 | 文件 ID |
| `CheckingRetryAttempts` | 无 | `int` | 否 | `20103 文件正在校验中` 重试次数，默认 60 |
| `CheckingRetryDelay` | 无 | `time.Duration` | 否 | `20103` 重试间隔，默认 1 秒 |

返回：`*pan123.DirectLinkURLData`

| 字段 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `URL` | `url` | `string` | 直链地址 |

示例：

```go
link, err := client.DirectLink.URL(ctx, pan123.DirectLinkURLRequest{
	FileID: 123456,
})
```

### `client.DirectLink.RefreshCache(ctx)`

用途：刷新直链缓存。

HTTP：`POST /api/v1/direct-link/cache/refresh`

参数：无。

返回：`map[string]any`，成功通常为空对象。

示例：

```go
_, err := client.DirectLink.RefreshCache(ctx)
```

### `client.DirectLink.GetTrafficLogs(ctx, params)`

用途：获取直链流量日志。

HTTP：`GET /api/v1/direct-link/log`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `pageNum` | `number` | 是 | 页码 |
| `pageSize` | `number` | 是 | 每页数量 |
| `startTime` | `string` | 是 | 开始时间，如 `2025-01-01 00:00:00` |
| `endTime` | `string` | 是 | 结束时间，如 `2025-01-01 23:59:59` |

返回：`map[string]any`，含 `total`、`list`。

示例：

```go
logs, err := client.DirectLink.GetTrafficLogs(ctx, map[string]any{
	"pageNum":   1,
	"pageSize":  100,
	"startTime": "2025-01-01 00:00:00",
	"endTime":   "2025-01-01 23:59:59",
})
```

### `client.DirectLink.GetOfflineLogs(ctx, params)`

用途：获取直链离线日志。

HTTP：`GET /api/v1/direct-link/offline/logs`

参数：`any`，常用 `map[string]any`。包含 `startHour`、`endHour`、`pageNum`、`pageSize`。

返回：`map[string]any`，含 `total`、`list`。

示例：

```go
logs, err := client.DirectLink.GetOfflineLogs(ctx, map[string]any{
	"startHour": "2025010115",
	"endHour":   "2025010116",
	"pageNum":   1,
	"pageSize":  100,
})
```

### `client.DirectLink.SetIPBlacklistEnabled(ctx, params)`

用途：启用或禁用 IP 黑名单。

HTTP：`POST /api/v1/developer/config/forbide-ip/switch`

参数：`any`，常用 `map[string]any`。包含 `Status`，`1` 启用，`2` 禁用。

返回：`map[string]any`，含 `Done`。

示例：

```go
_, err := client.DirectLink.SetIPBlacklistEnabled(ctx, map[string]any{"Status": 1})
```

### `client.DirectLink.UpdateIPBlacklist(ctx, params)`

用途：更新 IP 黑名单列表。

HTTP：`POST /api/v1/developer/config/forbide-ip/update`

参数：`any`，常用 `map[string]any`。包含 `IpList`，IPv4 地址列表，最多 2000 个。

返回：`map[string]any`，成功通常为空。

示例：

```go
_, err := client.DirectLink.UpdateIPBlacklist(ctx, map[string]any{
	"IpList": []string{"192.168.1.1"},
})
```

### `client.DirectLink.ListIPBlacklist(ctx, params)`

用途：获取 IP 黑名单列表。

HTTP：`GET /api/v1/developer/config/forbide-ip/list`

参数：官方未列出请求参数，传 `map[string]any{}`。

返回：`map[string]any`，含 `ipList`、`status`。

示例：

```go
list, err := client.DirectLink.ListIPBlacklist(ctx, map[string]any{})
```

## Oss 图床

### `client.Oss.Mkdir(ctx, params)`

用途：创建图床目录。

HTTP：`POST /upload/v1/oss/file/mkdir`

参数：`any`，常用 `map[string]any`。包含 `name` 字符串数组、`parentID`、`type: 1`。

返回：`map[string]any`，含 `list`。

示例：

```go
_, err := client.Oss.Mkdir(ctx, map[string]any{
	"name":     []string{"images"},
	"parentID": "",
	"type":     1,
})
```

### `client.Oss.Create(ctx, params)`

用途：创建图床文件上传任务。

HTTP：`POST /upload/v1/oss/file/create`

参数：`any`，常用 `map[string]any`。包含 `parentFileID`、`filename`、`etag`、`size`、`type: 1`。

返回：`map[string]any`，含 `reuse`、`fileID`、`preuploadID`、`sliceSize`。

示例：

```go
created, err := client.Oss.Create(ctx, map[string]any{
	"parentFileID": "",
	"filename":     "image.jpg",
	"etag":         "md5",
	"size":         1024,
	"type":         1,
})
```

### `client.Oss.GetUploadURL(ctx, params)`

用途：获取图床上传分片地址。

HTTP：`POST /upload/v1/oss/file/get_upload_url`

参数：`any`，常用 `map[string]any`。包含 `preuploadID`、`sliceNo`。

返回：`map[string]any`，含 `presignedURL`。

示例：

```go
uploadURL, err := client.Oss.GetUploadURL(ctx, map[string]any{
	"preuploadID": "pre-id",
	"sliceNo":     1,
})
```

### `client.Oss.Complete(ctx, params)`

用途：图床上传完成。

HTTP：`POST /upload/v1/oss/file/upload_complete`

参数：`any`，常用 `map[string]any`。包含 `preuploadID`。

返回：`map[string]any`，含 `async`、`completed`、`fileID`。

示例：

```go
completed, err := client.Oss.Complete(ctx, map[string]any{"preuploadID": "pre-id"})
```

### `client.Oss.AsyncResult(ctx, params)`

用途：轮询图床上传异步结果。

HTTP：`POST /upload/v1/oss/file/upload_async_result`

参数：`any`，常用 `map[string]any`。包含 `preuploadID`。

返回：`map[string]any`，含 `completed`、`fileID`。

示例：

```go
result, err := client.Oss.AsyncResult(ctx, map[string]any{"preuploadID": "pre-id"})
```

### `client.Oss.CreateCopyTask(ctx, params)`

用途：从云盘复制图片到图床。

HTTP：`POST /api/v1/oss/source/copy`

参数：`any`，常用 `map[string]any`。包含 `fileIDs`、`toParentFileID`、`sourceType: "1"`、`type: 1`。

返回：`map[string]any`，含 `taskID`。

示例：

```go
task, err := client.Oss.CreateCopyTask(ctx, map[string]any{
	"fileIDs":        []string{"123456"},
	"toParentFileID": "",
	"sourceType":     "1",
	"type":           1,
})
```

### `client.Oss.GetCopyProcess(ctx, params)`

用途：查询图床复制任务状态。

HTTP：`GET /api/v1/oss/source/copy/process`

参数：`any`，常用 `map[string]any`。包含 `taskID`。

返回：`map[string]any`，含 `status`、`failMsg`。

示例：

```go
process, err := client.Oss.GetCopyProcess(ctx, map[string]any{"taskID": "task-id"})
```

### `client.Oss.GetCopyFailList(ctx, params)`

用途：获取图床复制失败列表。

HTTP：`GET /api/v1/oss/source/copy/fail`

参数：`any`，常用 `map[string]any`。包含 `taskID`、`limit`、`page`。

返回：`map[string]any`，含 `total`、`list`。

示例：

```go
failList, err := client.Oss.GetCopyFailList(ctx, map[string]any{
	"taskID": "task-id",
	"limit":  100,
	"page":   1,
})
```

### `client.Oss.Move(ctx, params)`

用途：移动图床图片。

HTTP：`POST /api/v1/oss/file/move`

参数：`any`，常用 `map[string]any`。包含 `fileIDs`、`toParentFileID`。

返回：`map[string]any`，成功通常为空。

示例：

```go
_, err := client.Oss.Move(ctx, map[string]any{
	"fileIDs":        []string{"file-id"},
	"toParentFileID": "target-dir-id",
})
```

### `client.Oss.Delete(ctx, params)`

用途：删除图床图片。

HTTP：`POST /api/v1/oss/file/delete`

参数：`any`，常用 `map[string]any`。包含 `fileIDs`，最多 100 个。

返回：`map[string]any`，成功通常为空。

示例：

```go
_, err := client.Oss.Delete(ctx, map[string]any{"fileIDs": []string{"file-id"}})
```

### `client.Oss.Detail(ctx, params)`

用途：获取图床图片详情。

HTTP：`GET /api/v1/oss/file/detail`

参数：`any`，常用 `map[string]any`。包含 `fileID` 字符串。

返回：`map[string]any`，常见字段含 `fileId`、`filename`、`downloadURL`、`userSelfURL`、`totalTraffic`。

示例：

```go
detail, err := client.Oss.Detail(ctx, map[string]any{"fileID": "file-id"})
```

### `client.Oss.List(ctx, params)`

用途：获取图床图片列表。

HTTP：`POST /api/v1/oss/file/list`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `limit` | `number` | 是 | 每页数量，最大 100 |
| `type` | `number` | 是 | 固定为 `1` |
| `parentFileId` | `string` | 否 | 父目录 ID，根目录可为空 |
| `startTime` | `number` | 否 | 开始时间戳 |
| `endTime` | `number` | 否 | 结束时间戳 |
| `lastFileId` | `string` | 否 | 翻页游标 |

返回：`map[string]any`，含 `lastFileId`、`fileList`。

示例：

```go
list, err := client.Oss.List(ctx, map[string]any{"limit": 100, "type": 1})
```

### `client.Oss.CreateOfflineMigration(ctx, params)`

用途：创建图床离线迁移任务。

HTTP：`POST /api/v1/oss/offline/download`

参数：`any`，常用 `map[string]any`。包含 `url`、`type: 1`，可选 `fileName`、`businessDirID`、`callBackUrl`。

返回：`map[string]any`，含 `taskID`。

示例：

```go
task, err := client.Oss.CreateOfflineMigration(ctx, map[string]any{
	"url":  "https://example.com/image.jpg",
	"type": 1,
})
```

### `client.Oss.GetOfflineMigration(ctx, params)`

用途：查询图床离线迁移进度。

HTTP：`GET /api/v1/oss/offline/download/process`

参数：`any`，常用 `map[string]any`。包含 `taskID`。

返回：`map[string]any`，含 `process`、`status`。

示例：

```go
progress, err := client.Oss.GetOfflineMigration(ctx, map[string]any{"taskID": 403316})
```

## Transcode

### `client.Transcode.ListCloudDiskVideos(ctx, params)`

用途：获取云盘视频文件列表。

HTTP：`GET /api/v2/file/list`

参数：`pan123.FileListRequest`，即 `ParentFileID`、`Limit`、可选 `SearchData`、`SearchMode`、`LastFileID`。

返回：`*pan123.FileListData`。

示例：

```go
videos, err := client.Transcode.ListCloudDiskVideos(ctx, pan123.FileListRequest{
	ParentFileID: 0,
	Limit:        100,
})
```

注意：官方视频筛选字段 `category: 2` 当前可用 `Client.Do` 兜底传入。

### `client.Transcode.UploadFromCloudDisk(ctx, params)`

用途：从云盘空间导入视频到转码空间。

HTTP：`POST /api/v1/transcode/upload/from_cloud_disk`

参数：`any`，常用 `map[string]any`。包含 `fileId` 数组，一次最多 100 个。

返回：`map[string]any`。

示例：

```go
result, err := client.Transcode.UploadFromCloudDisk(ctx, map[string]any{
	"fileId": []map[string]any{{"fileId": 123456}},
})
```

### `client.Transcode.ListFiles(ctx, params)`

用途：获取转码空间文件列表。

HTTP：`GET /api/v2/file/list`

参数：`pan123.FileListRequest`。

返回：`*pan123.FileListData`。

示例：

```go
files, err := client.Transcode.ListFiles(ctx, pan123.FileListRequest{
	ParentFileID: 0,
	Limit:        100,
})
```

注意：官方转码空间字段 `businessType: 2` 当前可用 `Client.Do` 兜底传入。

### `client.Transcode.FolderInfo(ctx, params)`

用途：获取转码空间文件夹信息。

HTTP：`POST /api/v1/transcode/folder/info`

参数：官方未列出独立请求参数，传 `map[string]any{}`。

返回：`map[string]any`，含 `fileID`。

示例：

```go
info, err := client.Transcode.FolderInfo(ctx, map[string]any{})
```

### `client.Transcode.VideoResolutions(ctx, params)`

用途：获取视频文件可转码分辨率。

HTTP：`POST /api/v1/transcode/video/resolutions`

参数：`any`，常用 `map[string]any`。包含 `fileId`。

返回：`map[string]any`，含 `IsGetResolution`、`Resolutions`、`NowOrFinishedResolutions`、`CodecNames`、`VideoTime`。

示例：

```go
resolutions, err := client.Transcode.VideoResolutions(ctx, map[string]any{"fileId": 123456})
```

注意：官方建议轮询查询，约 10 秒一次。

### `client.Transcode.List(ctx, params)`

用途：获取视频转码列表，第三方挂载应用授权使用。

HTTP：`GET /api/v1/video/transcode/list`

参数：`any`，常用 `map[string]any`。包含 `fileId`。

返回：`map[string]any`，含 `status`、`list`。

示例：

```go
list, err := client.Transcode.List(ctx, map[string]any{"fileId": 123456})
```

### `client.Transcode.Transcode(ctx, params)`

用途：发起视频转码。

HTTP：`POST /api/v1/transcode/video`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `codecName` | `string` | 是 | 编码方式 |
| `videoTime` | `number` | 是 | 视频时长，单位秒 |
| `resolutions` | `string` | 是 | 分辨率，多个用逗号分隔，如 `2160P,1080P,720P` |

返回：`map[string]any`。

示例：

```go
_, err := client.Transcode.Transcode(ctx, map[string]any{
	"fileId":      123456,
	"codecName":   "H.264",
	"videoTime":   60,
	"resolutions": "1080P,720P",
})
```

### `client.Transcode.Record(ctx, params)`

用途：查询某个视频的转码记录。

HTTP：`POST /api/v1/transcode/video/record`

参数：`any`，常用 `map[string]any`。包含 `fileId`。

返回：`map[string]any`，含 `UserTranscodeVideoRecordList`。

示例：

```go
record, err := client.Transcode.Record(ctx, map[string]any{"fileId": 123456})
```

### `client.Transcode.Result(ctx, params)`

用途：查询某个视频的转码结果。

HTTP：`POST /api/v1/transcode/video/result`

参数：`any`，常用 `map[string]any`。包含 `fileId`。

返回：`map[string]any`，含 `UserTranscodeVideoList`。

示例：

```go
result, err := client.Transcode.Result(ctx, map[string]any{"fileId": 123456})
```

### `client.Transcode.Delete(ctx, params)`

用途：删除转码视频。

HTTP：`POST /api/v1/transcode/delete`

参数：`any`，常用 `map[string]any`。包含 `fileId`、`businessType: 2`、`trashed`。`trashed=1` 删除原文件，`2` 删除原文件和转码文件。

返回：`map[string]any`。

示例：

```go
_, err := client.Transcode.Delete(ctx, map[string]any{
	"fileId":       123456,
	"businessType": 2,
	"trashed":      2,
})
```

### `client.Transcode.DownloadOriginal(ctx, params)`

用途：获取转码空间原文件下载地址。

HTTP：`POST /api/v1/transcode/file/download`

参数：`any`，常用 `map[string]any`。包含 `fileId`。

返回：`map[string]any`，含 `downloadUrl`、`isFull`。

示例：

```go
download, err := client.Transcode.DownloadOriginal(ctx, map[string]any{"fileId": 123456})
```

### `client.Transcode.DownloadM3u8OrTs(ctx, params)`

用途：下载单个转码文件，m3u8 或 ts。

HTTP：`POST /api/v1/transcode/m3u8_ts/download`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `resolution` | `string` | 是 | 分辨率 |
| `type` | `number` | 是 | `1` 下载 m3u8，`2` 下载 ts |
| `tsName` | `string` | 否 | 下载 ts 时必填 |

返回：`map[string]any`，含 `downloadUrl`、`isFull`。

示例：

```go
download, err := client.Transcode.DownloadM3u8OrTs(ctx, map[string]any{
	"fileId":     123456,
	"resolution": "1080P",
	"type":       1,
})
```

### `client.Transcode.DownloadAll(ctx, params)`

用途：下载某个视频全部转码文件。

HTTP：`POST /api/v1/transcode/file/download/all`

参数：`any`，常用 `map[string]any`。

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `zipName` | `string` | 是 | 下载 zip 文件名 |

返回：`map[string]any`，含 `isDownloading`、`isFull`、`downloadUrl`。

示例：

```go
download, err := client.Transcode.DownloadAll(ctx, map[string]any{
	"fileId":  123456,
	"zipName": "video-transcode.zip",
})
```

## Low-Level Request

### `client.Do(ctx, method, path, options, out)`

用途：SDK 未封装新接口时的底层请求。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `ctx` | `context.Context` | 是 | 请求上下文 |
| `method` | `string` | 是 | HTTP 方法，如 `GET`、`POST`、`PUT` |
| `path` | `string` | 是 | API 路径 |
| `options.Query` | `any` | 否 | URL query |
| `options.Body` | `any` | 否 | JSON body |
| `options.Form` | `*pan123.MultipartForm` | 否 | multipart/form-data |
| `options.Headers` | `map[string]string` | 否 | 额外请求头 |
| `options.BaseURL` | `string` | 否 | 覆盖基础域名 |
| `options.NoAuth` | `bool` | 否 | 是否不自动带 Bearer token；默认 `false` 表示自动鉴权 |
| `out` | `any` | 否 | 解码目标；传指针接收官方响应中的 `data` |

返回：`error`。

示例：

```go
var detail map[string]any
err := client.Do(ctx, "GET", "/api/v1/file/detail", pan123.RequestOptions{
	Query: map[string]any{"fileID": 123456},
}, &detail)
```

multipart 示例：

```go
err := client.Do(ctx, "POST", "/upload/v2/file/slice", pan123.RequestOptions{
	BaseURL: uploadURL,
	Form: &pan123.MultipartForm{
		Fields: map[string]any{
			"preuploadID": "pre-id",
			"sliceNo":     1,
			"sliceMD5":    "slice-md5",
		},
		Files: []pan123.MultipartFile{{
			FieldName: "slice",
			FileName:  "part.bin",
			Reader:    bytes.NewReader(part),
		}},
	},
}, nil)
```

## 错误处理

```go
detail, err := client.Files.Detail(ctx, map[string]any{"fileID": 404})
if err != nil {
	var apiErr *pan123.APIError
	if errors.As(err, &apiErr) {
		fmt.Println(apiErr.Code)
		fmt.Println(apiErr.Message)
		fmt.Println(apiErr.TraceID)
		fmt.Println(apiErr.StatusCode)
		fmt.Println(apiErr.ResponseBody)
	}
}
_ = detail
```

常见业务错误：

| code | 说明 |
| --- | --- |
| `401` | access_token 无效 |
| `429` | 请求太频繁 |
| `5066` | 文件不存在 |
| `5113` | 流量超限 |
| `20103` | 文件正在校验中，部分方法会自动重试 |

## 本地开发

```bash
cd sdk-go
go test ./...
```

Live 测试：

```bash
PAN123_CLIENT_ID=your_client_id PAN123_CLIENT_SECRET=your_client_secret go test ./... -run Live
```

Live 测试会上传仓库根目录 `Test.txt`，不会删除或回收测试文件，也不会启用或禁用直链空间。
