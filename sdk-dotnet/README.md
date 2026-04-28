# Chest123.PanSdk

123 云盘开放平台 C#/.NET SDK。本文档按 SDK 方法逐个说明参数、返回值和示例，不按业务流程混写。

## 安装与初始化

```bash
dotnet add package Chest123.PanSdk
```

```csharp
using Chest123.PanSdk;

var client = new Pan123Client(new Pan123ClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("PAN123_CLIENT_ID"),
    ClientSecret = Environment.GetEnvironmentVariable("PAN123_CLIENT_SECRET")
});
```

Token 行为：

- SDK 会自动获取、缓存并维护 `access_token`。
- 调用需要鉴权的业务接口时，SDK 会先检查当前 token 是否可用。
- 如果没有 token，或 token 已过期/即将过期，SDK 会自动调用 `POST /api/v1/access_token` 获取新 token。
- 正常业务代码可以直接调用 `client.Files.ListAsync(...)`、`client.Upload.UploadFileAsync(...)` 等方法，不需要手动先获取 token。
- `client.Auth.EnsureAccessTokenAsync()` 是可选方法，适合你想提前预热 token 或调试 token 状态时使用。

## 客户端配置

| 属性 | 类型 | 必填 | 默认值 | 说明 |
| --- | --- | --- | --- | --- |
| `ClientId` | `string?` | 否 | 无 | 123 云盘开放平台 `clientID`，自动获取 token 时必填 |
| `ClientSecret` | `string?` | 否 | 无 | 123 云盘开放平台 `clientSecret`，自动获取 token 时必填 |
| `AccessToken` | `string?` | 否 | 无 | 已有 access token |
| `TokenExpiresAt` | `DateTimeOffset?` | 否 | 无 | access token 过期时间 |
| `BaseUrl` | `string` | 否 | `https://open-api.123pan.com` | API 基础地址 |
| `Platform` | `string` | 否 | `open_platform` | `Platform` 请求头 |
| `Timeout` | `TimeSpan` | 否 | 30 秒 | HTTP 请求超时时间 |
| `HttpClient` | `HttpClient?` | 否 | 无 | 自定义 HTTP 客户端 |

SDK 默认返回官方响应中的 `data` 字段。官方响应 `code != 0` 或 HTTP 请求失败时抛出 `Pan123ApiException`。

## 传参约定

- 常用接口提供 DTO，例如 `FileListRequest`、`UploadFileRequest`。
- 官方字段复杂或容易变化的接口使用 `object request`，建议传匿名对象。
- 匿名对象字段名就是发给官方的 JSON/query 字段名，例如 `new { fileID = 123456 }`。
- SDK 保留官方字段差异，不自动合并 `fileID/fileId`、`parentFileID/parentFileId`。

## Auth

### `client.Auth.GetAccessTokenAsync(cancellationToken?)`

用途：使用客户端配置中的 `ClientId` 和 `ClientSecret` 获取开发者 `access_token`。

HTTP：`POST /api/v1/access_token`

参数：无。

返回：`AccessTokenData`

| 属性 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `AccessToken` | `accessToken` | `string` | 访问凭证 |
| `ExpiredAt` | `expiredAt` | `DateTimeOffset` | 过期时间 |

示例：

```csharp
var token = await client.Auth.GetAccessTokenAsync();
```

### `client.Auth.EnsureAccessTokenAsync(cancellationToken?)`

用途：确保当前客户端有可用 access token；没有或即将过期时自动获取。

HTTP：本地 helper，必要时调用 `POST /api/v1/access_token`。

参数：无。

返回：`Task<string>`，可用 access token。

示例：

```csharp
var token = await client.Auth.EnsureAccessTokenAsync();
```

注意：所有需要鉴权的 SDK 方法内部都会自动执行这个逻辑。除非你想提前获取 token，否则不需要在每次业务调用前手动调用。

### `client.Auth.SetAccessToken(token, expiresAt?)`

用途：手动设置已有 access token。

HTTP：本地 helper。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `token` | `string` | 是 | access token |
| `expiresAt` | `DateTimeOffset?` | 否 | token 过期时间 |

返回：`void`

示例：

```csharp
client.Auth.SetAccessToken(existingToken, DateTimeOffset.UtcNow.AddHours(1));
```

### `client.Auth.GetOAuthTokenAsync(request, cancellationToken?)`

用途：OAuth 授权码或 refresh token 换取授权 token。

HTTP：`POST /api/v1/oauth2/access_token`

参数：`OAuthTokenRequest`

| 属性 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `ClientId` | `client_id` | `string` | 是 | OAuth 应用 appId |
| `ClientSecret` | `client_secret` | `string` | 是 | OAuth 应用 secretId |
| `GrantType` | `grant_type` | `string` | 是 | `authorization_code` 或 `refresh_token` |
| `Code` | `code` | `string?` | 否 | 授权码 |
| `RefreshToken` | `refresh_token` | `string?` | 否 | 刷新 token |
| `RedirectUri` | `redirect_uri` | `string?` | 否 | 授权回调地址 |

返回：`OAuthTokenData?`

| 属性 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `TokenType` | `token_type` | `string` | 通常为 `Bearer` |
| `AccessToken` | `access_token` | `string` | 授权 access token |
| `RefreshToken` | `refresh_token` | `string` | 新 refresh token |
| `ExpiresIn` | `expires_in` | `int` | 有效期，单位秒 |
| `Scope` | `scope` | `string` | 权限范围 |

示例：

```csharp
var oauth = await client.Auth.GetOAuthTokenAsync(new OAuthTokenRequest
{
    ClientId = "app-id",
    ClientSecret = "secret-id",
    GrantType = "authorization_code",
    Code = "oauth-code",
    RedirectUri = "https://example.com/callback"
});
```

## Files

### `client.Files.MkdirAsync(request, cancellationToken?)`

用途：创建目录。

HTTP：`POST /upload/v1/file/mkdir`

参数：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `name` | `string` | 是 | 目录名，不能重名 |
| `parentID` | `number` | 是 | 父目录 ID，根目录传 `0` |

返回：`JsonElement`，成功数据含 `dirID`。

示例：

```csharp
var dir = await client.Files.MkdirAsync(new { name = "SDK-Test", parentID = 0 });
```

### `client.Files.RenameAsync(request, cancellationToken?)`

用途：单个文件或目录重命名。

HTTP：`PUT /api/v1/file/name`

参数：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `fileName` | `string` | 是 | 新文件名 |

返回：`JsonElement`，成功通常为空。

示例：

```csharp
await client.Files.RenameAsync(new { fileId = 123456, fileName = "new-name.txt" });
```

### `client.Files.BatchRenameAsync(request, cancellationToken?)`

用途：批量重命名文件。

HTTP：`POST /api/v1/file/rename`

参数：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `renameList` | `array` | 是 | 重命名数组，最多 30 个 |

返回：`JsonElement`，成功数据含 `successList`、`failList`。

示例：

```csharp
await client.Files.BatchRenameAsync(new
{
    renameList = new[] { new { fileID = 123456, filename = "new-name.txt" } }
});
```

### `client.Files.TrashAsync(request, cancellationToken?)`

用途：删除文件至回收站。

HTTP：`POST /api/v1/file/trash`

参数：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |

返回：`JsonElement`，成功通常为空。

示例：

```csharp
await client.Files.TrashAsync(new { fileIDs = new[] { 123456 } });
```

### `client.Files.CopyAsync(request, cancellationToken?)`

用途：复制单个文件。

HTTP：`POST /api/v1/file/copy`

参数：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 源文件 ID |
| `targetDirId` | `number` | 是 | 目标目录 ID |

返回：`JsonElement`，成功数据含 `sourceFileId`、`targetFileId`。

示例：

```csharp
var copied = await client.Files.CopyAsync(new { fileId = 123456, targetDirId = 0 });
```

### `client.Files.AsyncCopyAsync(request, cancellationToken?)`

用途：批量复制文件。

HTTP：`POST /api/v1/file/async/copy`

参数：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIds` | `number[]` | 是 | 文件 ID 数组 |
| `targetDirId` | `number` | 是 | 目标目录 ID |

返回：`JsonElement`，成功数据含 `taskId`。

示例：

```csharp
var task = await client.Files.AsyncCopyAsync(new { fileIds = new[] { 123456 }, targetDirId = 0 });
```

### `client.Files.AsyncCopyProcessAsync(request, cancellationToken?)`

用途：查询批量复制任务进度。

HTTP：`GET /api/v1/file/async/copy/process`

参数：`taskId`。

返回：`JsonElement`，成功数据含 `taskId`、`status`。

示例：

```csharp
var progress = await client.Files.AsyncCopyProcessAsync(new { taskId = 2020 });
```

### `client.Files.RecoverAsync(request, cancellationToken?)`

用途：从回收站恢复文件到删除前位置。

HTTP：`POST /api/v1/file/recover`

参数：`fileIDs` 文件 ID 数组，最多 100 个。

返回：`JsonElement`，成功数据含 `abnormalFileIDs`。

示例：

```csharp
await client.Files.RecoverAsync(new { fileIDs = new[] { 123456 } });
```

### `client.Files.RecoverByPathAsync(request, cancellationToken?)`

用途：从回收站恢复文件到指定目录。

HTTP：`POST /api/v1/file/recover/by_path`

参数：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |
| `parentFileID` | `number` | 是 | 指定恢复目录 ID |

返回：`JsonElement`，成功通常为空。

示例：

```csharp
await client.Files.RecoverByPathAsync(new { fileIDs = new[] { 123456 }, parentFileID = 0 });
```

### `client.Files.DetailAsync(request, cancellationToken?)`

用途：获取单个文件详情。

HTTP：`GET /api/v1/file/detail`

参数：`fileID` 文件 ID。

返回：`JsonElement`，常见字段含 `fileID`、`filename`、`type`、`size`、`etag`、`status`、`parentFileID`、`trashed`。

示例：

```csharp
var detail = await client.Files.DetailAsync(new { fileID = 123456 });
```

### `client.Files.InfosAsync(request, cancellationToken?)`

用途：获取多个文件详情。

HTTP：`POST /api/v1/file/infos`

参数：`fileIds` 文件 ID 数组。

返回：`JsonElement`，成功数据含 `fileList`。

示例：

```csharp
var infos = await client.Files.InfosAsync(new { fileIds = new[] { 123456, 123457 } });
```

### `client.Files.ListAsync(request, cancellationToken?)`

用途：获取文件列表，推荐接口。

HTTP：`GET /api/v2/file/list`

参数：`FileListRequest`

| 属性 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `ParentFileId` | `parentFileId` | `long` | 是 | 文件夹 ID，根目录传 `0` |
| `Limit` | `limit` | `int` | 是 | 每页数量，最大 100 |
| `SearchData` | `searchData` | `string?` | 否 | 搜索关键字 |
| `SearchMode` | `searchMode` | `int?` | 否 | `0` 模糊搜索，`1` 精准搜索 |
| `LastFileId` | `lastFileId` | `long?` | 否 | 翻页游标 |

返回：`FileListData?`

| 属性 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `LastFileId` | `lastFileId` | `long` | `-1` 表示最后一页 |
| `FileList` | `fileList` | `List<FileInfo>` | 文件列表 |

示例：

```csharp
var list = await client.Files.ListAsync(new FileListRequest
{
    ParentFileId = 0,
    Limit = 100
});
```

注意：此接口可能返回回收站文件，请按 `Trashed` 字段过滤。

### `client.Files.ListLegacyAsync(request, cancellationToken?)`

用途：获取文件列表，旧接口。

HTTP：`GET /api/v1/file/list`

参数：`parentFileId`、`page`、`limit`、`orderBy`、`orderDirection`，可选 `trashed`、`searchData`。

返回：`JsonElement`，成功数据含 `total`、`fileList`。

示例：

```csharp
var list = await client.Files.ListLegacyAsync(new
{
    parentFileId = 0,
    page = 1,
    limit = 100,
    orderBy = "file_id",
    orderDirection = "desc"
});
```

### `client.Files.MoveAsync(request, cancellationToken?)`

用途：移动文件。

HTTP：`POST /api/v1/file/move`

参数：

| 字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |
| `toParentFileID` | `number` | 是 | 目标目录 ID，根目录传 `0` |

返回：`JsonElement`，成功通常为空。

示例：

```csharp
await client.Files.MoveAsync(new { fileIDs = new[] { 123456 }, toParentFileID = 0 });
```

### `client.Files.GetDownloadInfoAsync(request, cancellationToken?)`

用途：获取文件下载链接。

HTTP：`GET /api/v1/file/download_info`

参数：`DownloadInfoRequest`

| 属性 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `FileId` | `fileId` | `long` | 是 | 文件 ID |

返回：`DownloadInfoData?`

| 属性 | 官方字段 | 类型 | 说明 |
| --- | --- | --- | --- |
| `DownloadUrl` | `downloadUrl` | `string` | 下载地址 |

示例：

```csharp
var info = await client.Files.GetDownloadInfoAsync(new DownloadInfoRequest { FileId = 123456 });
```

## Upload V2

### `client.Upload.UploadFileAsync(request, cancellationToken?)`

用途：上传本地文件。SDK 自动计算文件 MD5，自动选择 V2 单步上传或分片上传。

HTTP：高层 helper，内部调用 V2 上传接口。

参数：`UploadFileRequest`

| 属性 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `FilePath` | `string` | 是 | 本地文件路径 |
| `ParentFileID` | `long` | 是 | 云盘父目录 ID，根目录传 `0` |
| `Filename` | `string?` | 否 | 云盘文件名，不传则取本地文件名 |
| `Duplicate` | `int?` | 否 | `1` 保留两者，`2` 覆盖原文件 |
| `ContainDir` | `bool?` | 否 | 文件名是否包含路径 |
| `SingleUploadMaxBytes` | `long` | 否 | 单步上传阈值，默认 1GB |

返回：`UploadFileResult`

| 属性 | 类型 | 说明 |
| --- | --- | --- |
| `FileID` | `long` | 上传后的文件 ID |
| `Completed` | `bool` | 是否完成 |
| `Reuse` | `bool` | 是否秒传 |

示例：

```csharp
var result = await client.Upload.UploadFileAsync(new UploadFileRequest
{
    FilePath = @"E:\Documents\nodejs\Chest123\Test.txt",
    ParentFileID = 0,
    Duplicate = 1
});
```

### `client.Upload.CreateAsync(request, cancellationToken?)`

用途：V2 创建文件，获取预上传信息或秒传结果。

HTTP：`POST /upload/v2/file/create`

参数：`UploadCreateRequest`

| 属性 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `ParentFileID` | `parentFileID` | `long` | 是 | 父目录 ID |
| `Filename` | `filename` | `string` | 是 | 文件名 |
| `Etag` | `etag` | `string` | 是 | 文件 MD5 |
| `Size` | `size` | `long` | 是 | 文件大小 |
| `Duplicate` | `duplicate` | `int?` | 否 | 同名策略 |
| `ContainDir` | `containDir` | `bool?` | 否 | 文件名是否包含路径 |

返回：`UploadCreateData?`，含 `Reuse`、`FileID`、`PreuploadID`、`SliceSize`、`Servers`。

示例：

```csharp
var created = await client.Upload.CreateAsync(new UploadCreateRequest
{
    ParentFileID = 0,
    Filename = "large.bin",
    Etag = "file-md5",
    Size = 1024
});
```

### `client.Upload.SliceAsync(uploadUrl, preuploadID, sliceNo, sliceMD5, slice, fileName, cancellationToken?)`

用途：V2 上传一个分片。

HTTP：`POST /upload/v2/file/slice`，基础域名使用 `uploadUrl`。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `uploadUrl` | `string` | 是 | 上传域名，来自 `CreateAsync` 返回的 `Servers` |
| `preuploadID` | `string` | 是 | 预上传 ID |
| `sliceNo` | `int` | 是 | 分片序号，从 1 开始 |
| `sliceMD5` | `string` | 是 | 当前分片 MD5 |
| `slice` | `Stream` | 是 | 分片内容 |
| `fileName` | `string` | 是 | 表单文件名 |

返回：`JsonElement`，成功通常为空。

示例：

```csharp
await using var stream = File.OpenRead("part.bin");
await client.Upload.SliceAsync("https://upload.example.com", "pre-id", 1, "slice-md5", stream, "part.bin");
```

### `client.Upload.CompleteAsync(preuploadID, cancellationToken?)`

用途：V2 通知上传完成，并查询合并结果。

HTTP：`POST /upload/v2/file/upload_complete`

参数：`preuploadID`。

返回：`UploadCompleteData?`，含 `Completed`、`FileID`。

示例：

```csharp
var completed = await client.Upload.CompleteAsync("pre-id");
```

### `client.Upload.DomainAsync(cancellationToken?)`

用途：获取 V2 单步上传域名。

HTTP：`GET /upload/v2/file/domain`

参数：无。

返回：`List<string>?`

示例：

```csharp
var domains = await client.Upload.DomainAsync();
```

### `client.Upload.SingleAsync(uploadUrl, filePath, request, cancellationToken?)`

用途：V2 单步上传小文件。

HTTP：`POST /upload/v2/file/single/create`，基础域名使用 `uploadUrl`。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `uploadUrl` | `string` | 是 | 上传域名 |
| `filePath` | `string` | 是 | 本地文件路径 |
| `request` | `UploadCreateRequest` | 是 | `ParentFileID`、`Filename`、`Etag`、`Size` 等 |

返回：`UploadCompleteData?`

示例：

```csharp
var result = await client.Upload.SingleAsync(uploadUrl, "./Test.txt", new UploadCreateRequest
{
    ParentFileID = 0,
    Filename = "Test.txt",
    Etag = "file-md5",
    Size = 123
});
```

### `client.Upload.Sha1ReuseAsync(request, cancellationToken?)`

用途：按 SHA1 尝试文件秒传。

HTTP：`POST /upload/v2/file/sha1_reuse`

参数：`parentFileID`、`filename`、`sha1`、`size`，可选 `duplicate`。

返回：`UploadCreateData?`

示例：

```csharp
var reuse = await client.Upload.Sha1ReuseAsync(new
{
    parentFileID = 0,
    filename = "file.bin",
    sha1 = "sha1",
    size = 1024
});
```

## Upload V1

### `client.UploadV1.CreateAsync(request, cancellationToken?)`

用途：V1 创建文件。

HTTP：`POST /upload/v1/file/create`

参数：`parentFileID`、`filename`、`etag`、`size`，可选 `duplicate`、`containDir`。

返回：`JsonElement`，含 `reuse`、`fileID`、`preuploadID`、`sliceSize`。

示例：

```csharp
await client.UploadV1.CreateAsync(new { parentFileID = 0, filename = "file.bin", etag = "md5", size = 1024 });
```

### `client.UploadV1.GetUploadUrlAsync(request, cancellationToken?)`

用途：获取 V1 分片上传地址。

HTTP：`POST /upload/v1/file/get_upload_url`

参数：`preuploadID`、`sliceNo`。

返回：`JsonElement`，含 `presignedURL`。

示例：

```csharp
await client.UploadV1.GetUploadUrlAsync(new { preuploadID = "pre-id", sliceNo = 1 });
```

### `client.UploadV1.ListUploadPartsAsync(request, cancellationToken?)`

用途：列举 V1 已上传分片。

HTTP：`POST /upload/v1/file/list_upload_parts`

参数：`preuploadID`。

返回：`JsonElement`，含 `parts`。

示例：

```csharp
await client.UploadV1.ListUploadPartsAsync(new { preuploadID = "pre-id" });
```

### `client.UploadV1.CompleteAsync(request, cancellationToken?)`

用途：V1 通知上传完成。

HTTP：`POST /upload/v1/file/upload_complete`

参数：`preuploadID`。

返回：`JsonElement`，含 `async`、`completed`、`fileID`。

示例：

```csharp
await client.UploadV1.CompleteAsync(new { preuploadID = "pre-id" });
```

### `client.UploadV1.AsyncResultAsync(request, cancellationToken?)`

用途：V1 异步轮询上传结果。

HTTP：`POST /upload/v1/file/upload_async_result`

参数：`preuploadID`。

返回：`JsonElement`，含 `completed`、`fileID`。

示例：

```csharp
await client.UploadV1.AsyncResultAsync(new { preuploadID = "pre-id" });
```

## Share

### `client.Share.CreateAsync(request, cancellationToken?)`

用途：创建普通分享链接。

HTTP：`POST /api/v1/share/create`

参数：`shareName`、`shareExpire`、`fileIDList`，可选 `sharePwd`、`trafficSwitch`、`trafficLimitSwitch`、`trafficLimit`。

返回：`JsonElement`，含 `shareID`、`shareKey`。

示例：

```csharp
await client.Share.CreateAsync(new { shareName = "share", shareExpire = 7, fileIDList = "123456" });
```

### `client.Share.ListAsync(request, cancellationToken?)`

用途：获取普通分享列表。

HTTP：`GET /api/v1/share/list`

参数：`limit`，可选 `lastShareId`。

返回：`JsonElement`，含 `shareList`、`lastShareId`。

示例：

```csharp
await client.Share.ListAsync(new { limit = 100 });
```

### `client.Share.UpdateAsync(request, cancellationToken?)`

用途：修改普通分享链接配置。

HTTP：`PUT /api/v1/share/list/info`

参数：`shareIdList`，可选 `trafficSwitch`、`trafficLimitSwitch`、`trafficLimit`。

返回：`JsonElement`，成功通常为空。

示例：

```csharp
await client.Share.UpdateAsync(new { shareIdList = new[] { 123456 }, trafficLimitSwitch = 1 });
```

### `client.Share.CreatePaidAsync(request, cancellationToken?)`

用途：创建付费分享链接。

HTTP：`POST /api/v1/share/content-payment/create`

参数：`shareName`、`fileIDList`、`payAmount`，可选 `isReward`、`resourceDesc`、`trafficSwitch`、`trafficLimitSwitch`、`trafficLimit`。

返回：`JsonElement`，含 `shareID`、`shareKey`。

示例：

```csharp
await client.Share.CreatePaidAsync(new { shareName = "paid", fileIDList = "123456", payAmount = 10 });
```

### `client.Share.ListPaidAsync(request, cancellationToken?)`

用途：获取付费分享列表。

HTTP：`GET /api/v1/share/payment/list`

参数：`limit`，可选 `lastShareId`。

返回：`JsonElement`，含 `shareList`、`lastShareId`。

示例：

```csharp
await client.Share.ListPaidAsync(new { limit = 100 });
```

### `client.Share.UpdatePaidAsync(request, cancellationToken?)`

用途：修改付费分享链接配置。

HTTP：`PUT /api/v1/share/list/payment/info`

参数：`shareIdList`，可选 `trafficSwitch`、`trafficLimitSwitch`、`trafficLimit`。

返回：`JsonElement`，成功通常为空。

示例：

```csharp
await client.Share.UpdatePaidAsync(new { shareIdList = new[] { 123456 }, trafficLimitSwitch = 1 });
```

## Offline

### `client.Offline.CreateDownloadTaskAsync(request, cancellationToken?)`

用途：创建离线下载任务。

HTTP：`POST /api/v1/offline/download`

参数：`url`，可选 `fileName`、`dirID`、`callBackUrl`。

返回：`JsonElement`，含 `taskID`。

示例：

```csharp
await client.Offline.CreateDownloadTaskAsync(new { url = "https://example.com/file.zip" });
```

### `client.Offline.GetDownloadProcessAsync(request, cancellationToken?)`

用途：查询离线下载进度。

HTTP：`GET /api/v1/offline/download/process`

参数：`taskID`。

返回：`JsonElement`，含 `process`、`status`。

示例：

```csharp
await client.Offline.GetDownloadProcessAsync(new { taskID = 394756 });
```

## User

### `client.User.GetInfoAsync(cancellationToken?)`

用途：获取用户信息。

HTTP：`GET /api/v1/user/info`

参数：无。

返回：`JsonElement`，常见字段含 `uid`、`nickname`、`spaceUsed`、`vip`、`directTraffic`、`developerInfo`。

示例：

```csharp
var user = await client.User.GetInfoAsync();
```

## DirectLink

### `client.DirectLink.EnableAsync(request, cancellationToken?)`

用途：启用直链空间。

HTTP：`POST /api/v1/direct-link/enable`

参数：`fileID`，要启用直链空间的文件夹 ID。

返回：`JsonElement`，含 `filename`。

示例：

```csharp
await client.DirectLink.EnableAsync(new { fileID = 123456 });
```

### `client.DirectLink.DisableAsync(request, cancellationToken?)`

用途：禁用直链空间。

HTTP：`POST /api/v1/direct-link/disable`

参数：`fileID`，要禁用直链空间的文件夹 ID。

返回：`JsonElement`，含 `filename`。

示例：

```csharp
await client.DirectLink.DisableAsync(new { fileID = 123456 });
```

### `client.DirectLink.GetUrlAsync(request, cancellationToken?)`

用途：获取文件直链 URL。

HTTP：`GET /api/v1/direct-link/url`

参数：`DirectLinkUrlRequest`

| 属性 | 官方字段 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- | --- |
| `FileID` | `fileID` | `long` | 是 | 文件 ID |

返回：`DirectLinkUrlData?`，含 `Url`。

示例：

```csharp
var link = await client.DirectLink.GetUrlAsync(new DirectLinkUrlRequest { FileID = 123456 });
```

### `client.DirectLink.RefreshCacheAsync(cancellationToken?)`

用途：刷新直链缓存。

HTTP：`POST /api/v1/direct-link/cache/refresh`

参数：无。

返回：`JsonElement`。

示例：

```csharp
await client.DirectLink.RefreshCacheAsync();
```

### `client.DirectLink.GetTrafficLogsAsync(request, cancellationToken?)`

用途：获取直链流量日志。

HTTP：`GET /api/v1/direct-link/log`

参数：`pageNum`、`pageSize`、`startTime`、`endTime`。

返回：`JsonElement`，含 `total`、`list`。

示例：

```csharp
await client.DirectLink.GetTrafficLogsAsync(new
{
    pageNum = 1,
    pageSize = 100,
    startTime = "2025-01-01 00:00:00",
    endTime = "2025-01-01 23:59:59"
});
```

### `client.DirectLink.GetOfflineLogsAsync(request, cancellationToken?)`

用途：获取直链离线日志。

HTTP：`GET /api/v1/direct-link/offline/logs`

参数：`startHour`、`endHour`、`pageNum`、`pageSize`。

返回：`JsonElement`，含 `total`、`list`。

示例：

```csharp
await client.DirectLink.GetOfflineLogsAsync(new
{
    startHour = "2025010115",
    endHour = "2025010116",
    pageNum = 1,
    pageSize = 100
});
```

### `client.DirectLink.SetIpBlacklistEnabledAsync(request, cancellationToken?)`

用途：启用或禁用 IP 黑名单。

HTTP：`POST /api/v1/developer/config/forbide-ip/switch`

参数：`Status`，`1` 启用，`2` 禁用。

返回：`JsonElement`，含 `Done`。

示例：

```csharp
await client.DirectLink.SetIpBlacklistEnabledAsync(new { Status = 1 });
```

### `client.DirectLink.UpdateIpBlacklistAsync(request, cancellationToken?)`

用途：更新 IP 黑名单列表。

HTTP：`POST /api/v1/developer/config/forbide-ip/update`

参数：`IpList`，IPv4 地址列表，最多 2000 个。

返回：`JsonElement`。

示例：

```csharp
await client.DirectLink.UpdateIpBlacklistAsync(new { IpList = new[] { "192.168.1.1" } });
```

### `client.DirectLink.ListIpBlacklistAsync(request, cancellationToken?)`

用途：获取 IP 黑名单列表。

HTTP：`GET /api/v1/developer/config/forbide-ip/list`

参数：官方未列出请求参数，传 `{}`。

返回：`JsonElement`，含 `ipList`、`status`。

示例：

```csharp
var list = await client.DirectLink.ListIpBlacklistAsync(new { });
```

## Oss 图床

### `client.Oss.MkdirAsync(request, cancellationToken?)`

用途：创建图床目录。

HTTP：`POST /upload/v1/oss/file/mkdir`

参数：`name` 字符串数组、`parentID`、`type: 1`。

返回：`JsonElement`，含 `list`。

示例：

```csharp
await client.Oss.MkdirAsync(new { name = new[] { "images" }, parentID = "", type = 1 });
```

### `client.Oss.CreateAsync(request, cancellationToken?)`

用途：创建图床文件上传任务。

HTTP：`POST /upload/v1/oss/file/create`

参数：`parentFileID`、`filename`、`etag`、`size`、`type: 1`。

返回：`JsonElement`，含 `reuse`、`fileID`、`preuploadID`、`sliceSize`。

示例：

```csharp
await client.Oss.CreateAsync(new { parentFileID = "", filename = "image.jpg", etag = "md5", size = 1024, type = 1 });
```

### `client.Oss.GetUploadUrlAsync(request, cancellationToken?)`

用途：获取图床上传分片地址。

HTTP：`POST /upload/v1/oss/file/get_upload_url`

参数：`preuploadID`、`sliceNo`。

返回：`JsonElement`，含 `presignedURL`。

示例：

```csharp
await client.Oss.GetUploadUrlAsync(new { preuploadID = "pre-id", sliceNo = 1 });
```

### `client.Oss.CompleteAsync(request, cancellationToken?)`

用途：图床上传完成。

HTTP：`POST /upload/v1/oss/file/upload_complete`

参数：`preuploadID`。

返回：`JsonElement`，含 `async`、`completed`、`fileID`。

示例：

```csharp
await client.Oss.CompleteAsync(new { preuploadID = "pre-id" });
```

### `client.Oss.AsyncResultAsync(request, cancellationToken?)`

用途：轮询图床上传异步结果。

HTTP：`POST /upload/v1/oss/file/upload_async_result`

参数：`preuploadID`。

返回：`JsonElement`，含 `completed`、`fileID`。

示例：

```csharp
await client.Oss.AsyncResultAsync(new { preuploadID = "pre-id" });
```

### `client.Oss.CreateCopyTaskAsync(request, cancellationToken?)`

用途：从云盘复制图片到图床。

HTTP：`POST /api/v1/oss/source/copy`

参数：`fileIDs`、`toParentFileID`、`sourceType: "1"`、`type: 1`。

返回：`JsonElement`，含 `taskID`。

示例：

```csharp
await client.Oss.CreateCopyTaskAsync(new
{
    fileIDs = new[] { "123456" },
    toParentFileID = "",
    sourceType = "1",
    type = 1
});
```

### `client.Oss.GetCopyProcessAsync(request, cancellationToken?)`

用途：查询图床复制任务状态。

HTTP：`GET /api/v1/oss/source/copy/process`

参数：`taskID`。

返回：`JsonElement`，含 `status`、`failMsg`。

示例：

```csharp
await client.Oss.GetCopyProcessAsync(new { taskID = "task-id" });
```

### `client.Oss.GetCopyFailListAsync(request, cancellationToken?)`

用途：获取图床复制失败列表。

HTTP：`GET /api/v1/oss/source/copy/fail`

参数：`taskID`、`limit`、`page`。

返回：`JsonElement`，含 `total`、`list`。

示例：

```csharp
await client.Oss.GetCopyFailListAsync(new { taskID = "task-id", limit = 100, page = 1 });
```

### `client.Oss.MoveAsync(request, cancellationToken?)`

用途：移动图床图片。

HTTP：`POST /api/v1/oss/file/move`

参数：`fileIDs`、`toParentFileID`。

返回：`JsonElement`，成功通常为空。

示例：

```csharp
await client.Oss.MoveAsync(new { fileIDs = new[] { "file-id" }, toParentFileID = "target-dir-id" });
```

### `client.Oss.DeleteAsync(request, cancellationToken?)`

用途：删除图床图片。

HTTP：`POST /api/v1/oss/file/delete`

参数：`fileIDs`，最多 100 个。

返回：`JsonElement`，成功通常为空。

示例：

```csharp
await client.Oss.DeleteAsync(new { fileIDs = new[] { "file-id" } });
```

### `client.Oss.DetailAsync(request, cancellationToken?)`

用途：获取图床图片详情。

HTTP：`GET /api/v1/oss/file/detail`

参数：`fileID` 字符串。

返回：`JsonElement`，含 `fileId`、`filename`、`downloadURL`、`userSelfURL`、`totalTraffic` 等。

示例：

```csharp
await client.Oss.DetailAsync(new { fileID = "file-id" });
```

### `client.Oss.ListAsync(request, cancellationToken?)`

用途：获取图床图片列表。

HTTP：`POST /api/v1/oss/file/list`

参数：`limit`、`type: 1`，可选 `parentFileId`、`startTime`、`endTime`、`lastFileId`。

返回：`JsonElement`，含 `lastFileId`、`fileList`。

示例：

```csharp
await client.Oss.ListAsync(new { limit = 100, type = 1 });
```

### `client.Oss.CreateOfflineMigrationAsync(request, cancellationToken?)`

用途：创建图床离线迁移任务。

HTTP：`POST /api/v1/oss/offline/download`

参数：`url`、`type: 1`，可选 `fileName`、`businessDirID`、`callBackUrl`。

返回：`JsonElement`，含 `taskID`。

示例：

```csharp
await client.Oss.CreateOfflineMigrationAsync(new { url = "https://example.com/image.jpg", type = 1 });
```

### `client.Oss.GetOfflineMigrationAsync(request, cancellationToken?)`

用途：查询图床离线迁移进度。

HTTP：`GET /api/v1/oss/offline/download/process`

参数：`taskID`。

返回：`JsonElement`，含 `process`、`status`。

示例：

```csharp
await client.Oss.GetOfflineMigrationAsync(new { taskID = 403316 });
```

## Transcode

### `client.Transcode.ListCloudDiskVideosAsync(request, cancellationToken?)`

用途：获取云盘视频文件列表。

HTTP：`GET /api/v2/file/list`

参数：`FileListRequest`，即 `ParentFileId`、`Limit`、可选 `SearchData`、`SearchMode`、`LastFileId`。

返回：`FileListData?`

示例：

```csharp
await client.Transcode.ListCloudDiskVideosAsync(new FileListRequest { ParentFileId = 0, Limit = 100 });
```

注意：官方视频筛选字段 `category: 2` 当前可用 `SendAsync<T>` 兜底传入。

### `client.Transcode.UploadFromCloudDiskAsync(request, cancellationToken?)`

用途：从云盘空间导入视频到转码空间。

HTTP：`POST /api/v1/transcode/upload/from_cloud_disk`

参数：`fileId` 数组，一次最多 100 个。

返回：`JsonElement`。

示例：

```csharp
await client.Transcode.UploadFromCloudDiskAsync(new { fileId = new[] { new { fileId = 123456 } } });
```

### `client.Transcode.ListFilesAsync(request, cancellationToken?)`

用途：获取转码空间文件列表。

HTTP：`GET /api/v2/file/list`

参数：`FileListRequest`。

返回：`FileListData?`

示例：

```csharp
await client.Transcode.ListFilesAsync(new FileListRequest { ParentFileId = 0, Limit = 100 });
```

注意：官方转码空间字段 `businessType: 2` 当前可用 `SendAsync<T>` 兜底传入。

### `client.Transcode.FolderInfoAsync(request, cancellationToken?)`

用途：获取转码空间文件夹信息。

HTTP：`POST /api/v1/transcode/folder/info`

参数：官方未列出独立请求参数，传 `{}`。

返回：`JsonElement`，含 `fileID`。

示例：

```csharp
await client.Transcode.FolderInfoAsync(new { });
```

### `client.Transcode.VideoResolutionsAsync(request, cancellationToken?)`

用途：获取视频文件可转码分辨率。

HTTP：`POST /api/v1/transcode/video/resolutions`

参数：`fileId`。

返回：`JsonElement`，含 `IsGetResolution`、`Resolutions`、`NowOrFinishedResolutions`、`CodecNames`、`VideoTime`。

示例：

```csharp
await client.Transcode.VideoResolutionsAsync(new { fileId = 123456 });
```

### `client.Transcode.ListAsync(request, cancellationToken?)`

用途：获取视频转码列表，第三方挂载应用授权使用。

HTTP：`GET /api/v1/video/transcode/list`

参数：`fileId`。

返回：`JsonElement`，含 `status`、`list`。

示例：

```csharp
await client.Transcode.ListAsync(new { fileId = 123456 });
```

### `client.Transcode.TranscodeAsync(request, cancellationToken?)`

用途：发起视频转码。

HTTP：`POST /api/v1/transcode/video`

参数：`fileId`、`codecName`、`videoTime`、`resolutions`。

返回：`JsonElement`。

示例：

```csharp
await client.Transcode.TranscodeAsync(new
{
    fileId = 123456,
    codecName = "H.264",
    videoTime = 60,
    resolutions = "1080P,720P"
});
```

### `client.Transcode.RecordAsync(request, cancellationToken?)`

用途：查询某个视频的转码记录。

HTTP：`POST /api/v1/transcode/video/record`

参数：`fileId`。

返回：`JsonElement`，含 `UserTranscodeVideoRecordList`。

示例：

```csharp
await client.Transcode.RecordAsync(new { fileId = 123456 });
```

### `client.Transcode.ResultAsync(request, cancellationToken?)`

用途：查询某个视频的转码结果。

HTTP：`POST /api/v1/transcode/video/result`

参数：`fileId`。

返回：`JsonElement`，含 `UserTranscodeVideoList`。

示例：

```csharp
await client.Transcode.ResultAsync(new { fileId = 123456 });
```

### `client.Transcode.DeleteAsync(request, cancellationToken?)`

用途：删除转码视频。

HTTP：`POST /api/v1/transcode/delete`

参数：`fileId`、`businessType: 2`、`trashed`。`trashed = 1` 删除原文件，`2` 删除原文件和转码文件。

返回：`JsonElement`。

示例：

```csharp
await client.Transcode.DeleteAsync(new { fileId = 123456, businessType = 2, trashed = 2 });
```

### `client.Transcode.DownloadOriginalAsync(request, cancellationToken?)`

用途：获取转码空间原文件下载地址。

HTTP：`POST /api/v1/transcode/file/download`

参数：`fileId`。

返回：`JsonElement`，含 `downloadUrl`、`isFull`。

示例：

```csharp
await client.Transcode.DownloadOriginalAsync(new { fileId = 123456 });
```

### `client.Transcode.DownloadM3u8OrTsAsync(request, cancellationToken?)`

用途：下载单个转码文件，m3u8 或 ts。

HTTP：`POST /api/v1/transcode/m3u8_ts/download`

参数：`fileId`、`resolution`、`type`，下载 ts 时传 `tsName`。`type = 1` 下载 m3u8，`2` 下载 ts。

返回：`JsonElement`，含 `downloadUrl`、`isFull`。

示例：

```csharp
await client.Transcode.DownloadM3u8OrTsAsync(new
{
    fileId = 123456,
    resolution = "1080P",
    type = 1
});
```

### `client.Transcode.DownloadAllAsync(request, cancellationToken?)`

用途：下载某个视频全部转码文件。

HTTP：`POST /api/v1/transcode/file/download/all`

参数：`fileId`、`zipName`。

返回：`JsonElement`，含 `isDownloading`、`isFull`、`downloadUrl`。

示例：

```csharp
await client.Transcode.DownloadAllAsync(new { fileId = 123456, zipName = "video-transcode.zip" });
```

## Low-Level Request

### `client.SendAsync<T>(method, path, options?, cancellationToken?)`

用途：SDK 未封装新接口时的底层请求。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `method` | `HttpMethod` | 是 | HTTP 方法 |
| `path` | `string` | 是 | API 路径 |
| `options.Query` | `object?` | 否 | URL query |
| `options.Body` | `object?` | 否 | JSON body |
| `options.Multipart` | `MultipartFormDataContent?` | 否 | multipart/form-data |
| `options.Headers` | `Dictionary<string,string>` | 否 | 额外请求头 |
| `options.BaseUrl` | `string?` | 否 | 覆盖基础域名 |
| `options.Auth` | `bool` | 否 | 是否自动带 Bearer token，默认 `true` |

返回：`Task<T?>`，官方响应中的 `data`。

示例：

```csharp
var data = await client.SendAsync<object>(
    HttpMethod.Get,
    "/api/v1/file/detail",
    new Pan123RequestOptions { Query = new { fileID = 123456 } });
```

## 错误处理

```csharp
try
{
    await client.Files.DetailAsync(new { fileID = 404 });
}
catch (Pan123ApiException ex)
{
    Console.WriteLine(ex.Code);
    Console.WriteLine(ex.TraceId);
    Console.WriteLine(ex.StatusCode);
    Console.WriteLine(ex.ResponseBody);
}
```

常见业务错误：

| code | 说明 |
| --- | --- |
| `401` | access_token 无效 |
| `429` | 请求太频繁 |
| `5066` | 文件不存在 |
| `5113` | 流量超限 |

## 本地开发

```bash
cd sdk-dotnet
dotnet restore
dotnet build Chest123.PanSdk.sln -c Release
dotnet test Chest123.PanSdk.sln -c Release
dotnet pack src/Chest123.PanSdk/Chest123.PanSdk.csproj -c Release
```

Live 测试：

```powershell
$env:PAN123_CLIENT_ID="your-client-id"
$env:PAN123_CLIENT_SECRET="your-client-secret"
$env:PAN123_PARENT_FILE_ID="0"
dotnet test .\tests\Chest123.PanSdk.Tests\Chest123.PanSdk.Tests.csproj -c Release --filter Live
```

Live 测试会上传仓库根目录 `Test.txt`，不会删除或回收测试文件，也不会启用或禁用直链空间。
