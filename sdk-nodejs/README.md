# chest123-pan-sdk

123 云盘开放平台 Node.js SDK。本文档按 SDK 函数逐个说明参数、返回值和示例，不按业务流程混写。

## 安装与初始化

```bash
npm install chest123-pan-sdk
```

要求 Node.js `>= 18`。

```ts
import { createPan123Client } from 'chest123-pan-sdk';

const client = createPan123Client({
  clientId: process.env.PAN123_CLIENT_ID,
  clientSecret: process.env.PAN123_CLIENT_SECRET
});
```

## 客户端配置

| 参数 | 类型 | 必填 | 默认值 | 说明 |
| --- | --- | --- | --- | --- |
| `clientId` | `string` | 否 | 无 | 123 云盘开放平台 `clientID`，自动获取 token 时必填 |
| `clientSecret` | `string` | 否 | 无 | 123 云盘开放平台 `clientSecret`，自动获取 token 时必填 |
| `accessToken` | `string` | 否 | 无 | 已有 access token |
| `tokenExpiresAt` | `string \| Date \| number` | 否 | 无 | access token 过期时间 |
| `baseURL` | `string` | 否 | `https://open-api.123pan.com` | API 基础地址 |
| `platform` | `string` | 否 | `open_platform` | `Platform` 请求头 |
| `timeoutMs` | `number` | 否 | `30000` | 请求超时时间，单位毫秒 |

SDK 默认返回官方响应中的 `data` 字段。官方响应 `code !== 0` 或 HTTP 请求失败时抛出 `Pan123ApiError`。

## 导出项

| 导出 | 说明 |
| --- | --- |
| `Pan123Client` | SDK 客户端类 |
| `createPan123Client(options)` | 创建 SDK 客户端 |
| `Pan123ApiError` | SDK API 异常类型 |
| 类型导出 | `Pan123ClientOptions`、`Pan123Response<T>`、上传/文件等 DTO 类型 |

## Auth

### `client.auth.getAccessToken()`

用途：使用客户端配置中的 `clientId` 和 `clientSecret` 获取开发者 `access_token`。

HTTP：`POST /api/v1/access_token`

参数：无。

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `accessToken` | `string` | 访问凭证 |
| `expiredAt` | `string` | 过期时间 |

示例：

```ts
const token = await client.auth.getAccessToken();
console.log(token.accessToken, token.expiredAt);
```

注意：SDK 会自动缓存 token，普通业务代码通常不需要手动调用。

### `client.auth.ensureAccessToken()`

用途：确保当前客户端有可用 access token；没有或即将过期时自动获取。

HTTP：本地 helper，必要时调用 `POST /api/v1/access_token`。

参数：无。

返回：`Promise<string>`，可用 access token。

示例：

```ts
const token = await client.auth.ensureAccessToken();
```

### `client.auth.setAccessToken(token, expiresAt?)`

用途：手动设置已有 access token。

HTTP：本地 helper。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `token` | `string` | 是 | access token |
| `expiresAt` | `string \| Date \| number` | 否 | token 过期时间 |

返回：`void`

示例：

```ts
client.auth.setAccessToken(existingToken, '2026-05-01T00:00:00+08:00');
```

### `client.auth.getOAuthToken(params)`

用途：OAuth 授权码或 refresh token 换取授权 token。

HTTP：`POST /api/v1/oauth2/access_token`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `client_id` | `string` | 是 | OAuth 应用 appId |
| `client_secret` | `string` | 是 | OAuth 应用 secretId |
| `grant_type` | `'authorization_code' \| 'refresh_token'` | 是 | 授权类型 |
| `code` | `string` | 否 | 授权码，`authorization_code` 时使用 |
| `refresh_token` | `string` | 否 | 刷新 token，`refresh_token` 时使用 |
| `redirect_uri` | `string` | 否 | 授权回调地址 |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `token_type` | `string` | 通常为 `Bearer` |
| `access_token` | `string` | 授权 access token |
| `refresh_token` | `string` | 新 refresh token |
| `expires_in` | `number` | access token 有效期，单位秒 |
| `scope` | `string` | 权限范围 |

示例：

```ts
const oauth = await client.auth.getOAuthToken({
  client_id: 'app-id',
  client_secret: 'secret-id',
  grant_type: 'authorization_code',
  code: 'oauth-code',
  redirect_uri: 'https://example.com/callback'
});
```

## Files

### `client.files.mkdir(params)`

用途：创建目录。

HTTP：`POST /upload/v1/file/mkdir`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `name` | `string` | 是 | 目录名，不能重名 |
| `parentID` | `number` | 是 | 父目录 ID，根目录传 `0` |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `dirID` | `number` | 创建的目录 ID |

示例：

```ts
const dir = await client.files.mkdir({ name: 'SDK-Test', parentID: 0 });
```

### `client.files.rename(params)`

用途：单个文件或目录重命名。

HTTP：`PUT /api/v1/file/name`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `fileName` | `string` | 是 | 新文件名 |

返回：成功通常为 `null`。

示例：

```ts
await client.files.rename({ fileId: 123456, fileName: 'new-name.txt' });
```

### `client.files.batchRename(params)`

用途：批量重命名文件。

HTTP：`POST /api/v1/file/rename`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `renameList` | `array` | 是 | 重命名数组，按官方格式传文件 ID 和新文件名，最多 30 个 |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `successList` | `array` | 成功列表 |
| `failList` | `array` | 失败列表，含失败原因 |

示例：

```ts
await client.files.batchRename({
  renameList: [{ fileID: 123456, filename: 'new-name.txt' }]
});
```

### `client.files.trash(params)`

用途：删除文件至回收站。

HTTP：`POST /api/v1/file/trash`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |

返回：成功通常为 `null`。

示例：

```ts
await client.files.trash({ fileIDs: [123456] });
```

### `client.files.copy(params)`

用途：复制单个文件。

HTTP：`POST /api/v1/file/copy`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 源文件 ID |
| `targetDirId` | `number` | 是 | 目标目录 ID |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `sourceFileId` | `number` | 源文件 ID |
| `targetFileId` | `number` | 新复制文件 ID |

示例：

```ts
const copied = await client.files.copy({ fileId: 123456, targetDirId: 0 });
```

### `client.files.asyncCopy(params)`

用途：批量复制文件。

HTTP：`POST /api/v1/file/async/copy`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIds` | `number[]` | 是 | 文件 ID 数组 |
| `targetDirId` | `number` | 是 | 目标目录 ID |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `taskId` | `number` | 异步任务 ID |

示例：

```ts
const task = await client.files.asyncCopy({ fileIds: [123456], targetDirId: 0 });
```

### `client.files.asyncCopyProcess(params)`

用途：查询批量复制任务进度。

HTTP：`GET /api/v1/file/async/copy/process`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `taskId` | `number` | 是 | 批量复制任务 ID |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `taskId` | `number` | 任务 ID |
| `status` | `number` | `0` 待处理，`1` 进行中，`2` 已完成，`3` 失败 |

示例：

```ts
const progress = await client.files.asyncCopyProcess({ taskId: 2020 });
```

### `client.files.recover(params)`

用途：从回收站恢复文件到删除前位置。

HTTP：`POST /api/v1/file/recover`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `abnormalFileIDs` | `number[]` | 父级目录不存在等异常文件 ID |

示例：

```ts
await client.files.recover({ fileIDs: [123456] });
```

### `client.files.recoverByPath(params)`

用途：从回收站恢复文件到指定目录。

HTTP：`POST /api/v1/file/recover/by_path`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |
| `parentFileID` | `number` | 是 | 指定恢复目录 ID |

返回：成功通常为 `null`。

示例：

```ts
await client.files.recoverByPath({ fileIDs: [123456], parentFileID: 0 });
```

### `client.files.detail(params)`

用途：获取单个文件详情。

HTTP：`GET /api/v1/file/detail`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileID` | `number` | 是 | 文件 ID |

返回：文件详情。

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

```ts
const detail = await client.files.detail({ fileID: 123456 });
```

### `client.files.infos(params)`

用途：获取多个文件详情。

HTTP：`POST /api/v1/file/infos`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIds` | `number[]` | 是 | 文件 ID 数组 |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `fileList` | `array` | 文件详情列表 |

示例：

```ts
const infos = await client.files.infos({ fileIds: [123456, 123457] });
```

### `client.files.list(params)`

用途：获取文件列表，推荐接口。

HTTP：`GET /api/v2/file/list`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `parentFileId` | `number` | 是 | 文件夹 ID，根目录传 `0` |
| `limit` | `number` | 是 | 每页数量，最大 100 |
| `searchData` | `string` | 否 | 搜索关键字，传入后会全局搜索 |
| `searchMode` | `number` | 否 | `0` 模糊搜索，`1` 精准搜索 |
| `lastFileId` | `number` | 否 | 翻页游标 |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `lastFileId` | `number` | `-1` 表示最后一页 |
| `fileList` | `FileInfo[]` | 文件列表 |

示例：

```ts
const list = await client.files.list({ parentFileId: 0, limit: 100 });
```

注意：此接口可能返回回收站文件，请按 `trashed` 字段过滤。

### `client.files.listLegacy(params)`

用途：获取文件列表，旧接口。

HTTP：`GET /api/v1/file/list`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `parentFileId` | `number` | 是 | 文件夹 ID，根目录传 `0` |
| `page` | `number` | 是 | 页码 |
| `limit` | `number` | 是 | 每页数量，最大 100 |
| `orderBy` | `string` | 是 | 排序字段，如 `file_id`、`size`、`file_name` |
| `orderDirection` | `string` | 是 | `asc` 或 `desc` |
| `trashed` | `boolean` | 否 | 是否查看回收站 |
| `searchData` | `string` | 否 | 搜索关键字 |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `total` | `number` | 总数 |
| `fileList` | `array` | 文件列表 |

示例：

```ts
const list = await client.files.listLegacy({
  parentFileId: 0,
  page: 1,
  limit: 100,
  orderBy: 'file_id',
  orderDirection: 'desc'
});
```

### `client.files.move(params)`

用途：移动文件。

HTTP：`POST /api/v1/file/move`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `number[]` | 是 | 文件 ID 数组，最多 100 个 |
| `toParentFileID` | `number` | 是 | 目标目录 ID，根目录传 `0` |

返回：成功通常为 `null`。

示例：

```ts
await client.files.move({ fileIDs: [123456], toParentFileID: 0 });
```

### `client.files.downloadInfo(params)`

用途：获取文件下载链接。

HTTP：`GET /api/v1/file/download_info`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `downloadUrl` | `string` | 下载地址 |

示例：

```ts
const info = await client.files.downloadInfo({ fileId: 123456 });
```

## Upload V2

### `client.upload.uploadFile(options)`

用途：上传本地文件。SDK 自动计算文件 MD5，自动选择 V2 单步上传或分片上传。

HTTP：高层 helper，内部会调用 V2 上传接口。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `filePath` | `string` | 是 | 本地文件路径 |
| `parentFileID` | `number` | 是 | 云盘父目录 ID，根目录传 `0` |
| `filename` | `string` | 否 | 云盘文件名，不传则取本地文件名 |
| `duplicate` | `number` | 否 | `1` 保留两者，`2` 覆盖原文件 |
| `containDir` | `boolean` | 否 | 文件名是否包含路径 |
| `singleUploadMaxBytes` | `number` | 否 | 单步上传阈值，默认 1GB |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `fileID` | `number` | 上传后的文件 ID |
| `completed` | `boolean` | 是否上传完成 |
| `reuse` | `boolean` | 是否秒传，可能不存在 |

示例：

```ts
const result = await client.upload.uploadFile({
  filePath: './Test.txt',
  parentFileID: 0,
  duplicate: 1
});
```

### `client.upload.create(params)`

用途：V2 创建文件，获取预上传信息或秒传结果。

HTTP：`POST /upload/v2/file/create`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `parentFileID` | `number` | 是 | 父目录 ID，根目录传 `0` |
| `filename` | `string` | 是 | 文件名 |
| `etag` | `string` | 是 | 文件 MD5 |
| `size` | `number` | 是 | 文件大小，单位 byte |
| `duplicate` | `number` | 否 | `1` 保留两者，`2` 覆盖原文件 |
| `containDir` | `boolean` | 否 | 文件名是否包含路径 |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `reuse` | `boolean` | 是否秒传 |
| `fileID` | `number` | 秒传成功时的文件 ID |
| `preuploadID` | `string` | 预上传 ID |
| `sliceSize` | `number` | 分片大小 |
| `servers` | `string[]` | 上传域名 |

示例：

```ts
const created = await client.upload.create({
  parentFileID: 0,
  filename: 'large.bin',
  etag: 'file-md5',
  size: 1024
});
```

### `client.upload.slice(params)`

用途：V2 上传一个分片。

HTTP：`POST /upload/v2/file/slice`，基础域名使用 `params.uploadURL`。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `uploadURL` | `string` | 是 | 上传域名，来自 `create` 返回的 `servers` |
| `preuploadID` | `string` | 是 | 预上传 ID |
| `sliceNo` | `number` | 是 | 分片序号，从 1 开始 |
| `sliceMD5` | `string` | 是 | 当前分片 MD5 |
| `slice` | `Buffer \| NodeJS.ReadableStream` | 是 | 分片内容 |
| `filename` | `string` | 否 | 表单文件名 |

返回：成功通常为 `null`。

示例：

```ts
await client.upload.slice({
  uploadURL: 'https://upload.example.com',
  preuploadID: 'preupload-id',
  sliceNo: 1,
  sliceMD5: 'slice-md5',
  slice: buffer,
  filename: 'large.bin.part1'
});
```

### `client.upload.complete(params)`

用途：V2 通知上传完成，并查询合并结果。

HTTP：`POST /upload/v2/file/upload_complete`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `preuploadID` | `string` | 是 | 预上传 ID |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `completed` | `boolean` | 是否完成 |
| `fileID` | `number` | 完成后的文件 ID |

示例：

```ts
const completed = await client.upload.complete({ preuploadID: 'preupload-id' });
```

注意：返回 `completed: false` 时，至少间隔 1 秒后继续轮询。

### `client.upload.domain()`

用途：获取 V2 单步上传域名。

HTTP：`GET /upload/v2/file/domain`

参数：无。

返回：`string[]`，上传域名列表。

示例：

```ts
const domains = await client.upload.domain();
```

### `client.upload.single(params)`

用途：V2 单步上传小文件。

HTTP：`POST /upload/v2/file/single/create`，基础域名使用 `uploadURL` 或自动获取。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `filePath` | `string` | 是 | 本地文件路径 |
| `parentFileID` | `number` | 是 | 父目录 ID |
| `filename` | `string` | 是 | 文件名 |
| `etag` | `string` | 是 | 文件 MD5 |
| `size` | `number` | 是 | 文件大小 |
| `uploadURL` | `string` | 否 | 上传域名，不传则自动调用 `domain()` |
| `duplicate` | `number` | 否 | 同名策略 |
| `containDir` | `boolean` | 否 | 文件名是否包含路径 |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `completed` | `boolean` | 是否完成 |
| `fileID` | `number` | 文件 ID |

示例：

```ts
const result = await client.upload.single({
  filePath: './Test.txt',
  parentFileID: 0,
  filename: 'Test.txt',
  etag: 'file-md5',
  size: 123
});
```

注意：官方限制单步上传最大 1GB。

### `client.upload.sha1Reuse(params)`

用途：按 SHA1 尝试文件秒传。

HTTP：`POST /upload/v2/file/sha1_reuse`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `parentFileID` | `number` | 是 | 父目录 ID |
| `filename` | `string` | 是 | 文件名 |
| `sha1` | `string` | 是 | 文件 SHA1 |
| `size` | `number` | 是 | 文件大小 |
| `duplicate` | `number` | 否 | 同名策略 |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `reuse` | `boolean` | 是否秒传 |
| `fileID` | `number` | 秒传成功时的文件 ID |

示例：

```ts
const reuse = await client.upload.sha1Reuse({
  parentFileID: 0,
  filename: 'file.bin',
  sha1: 'sha1',
  size: 1024
});
```

## Upload V1

### `client.uploadV1.create(params)`

用途：V1 创建文件。

HTTP：`POST /upload/v1/file/create`

参数：同 V2 `upload.create`，字段为 `parentFileID`、`filename`、`etag`、`size`、可选 `duplicate`、`containDir`。

返回：`reuse`、`fileID`、`preuploadID`、`sliceSize`。

示例：

```ts
await client.uploadV1.create({
  parentFileID: 0,
  filename: 'file.bin',
  etag: 'file-md5',
  size: 1024
});
```

### `client.uploadV1.getUploadUrl(params)`

用途：获取 V1 分片上传地址。

HTTP：`POST /upload/v1/file/get_upload_url`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `preuploadID` | `string` | 是 | 预上传 ID |
| `sliceNo` | `number` | 是 | 分片序号，从 1 开始 |

返回：`presignedURL`。

示例：

```ts
const url = await client.uploadV1.getUploadUrl({ preuploadID: 'pre-id', sliceNo: 1 });
```

注意：向 `presignedURL` PUT 分片时不要携带 `Authorization` 和 `Platform`。

### `client.uploadV1.listUploadParts(params)`

用途：列举 V1 已上传分片。

HTTP：`POST /upload/v1/file/list_upload_parts`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `preuploadID` | `string` | 是 | 预上传 ID |

返回：`parts`，每项含 `partNumber`、`size`、`etag`。

示例：

```ts
const parts = await client.uploadV1.listUploadParts({ preuploadID: 'pre-id' });
```

### `client.uploadV1.complete(params)`

用途：V1 通知上传完成。

HTTP：`POST /upload/v1/file/upload_complete`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `preuploadID` | `string` | 是 | 预上传 ID |

返回：`async`、`completed`、`fileID`。

示例：

```ts
const completed = await client.uploadV1.complete({ preuploadID: 'pre-id' });
```

### `client.uploadV1.asyncResult(params)`

用途：V1 异步轮询上传结果。

HTTP：`POST /upload/v1/file/upload_async_result`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `preuploadID` | `string` | 是 | 预上传 ID |

返回：`completed`、`fileID`。

示例：

```ts
const result = await client.uploadV1.asyncResult({ preuploadID: 'pre-id' });
```

## Share

### `client.share.create(params)`

用途：创建普通分享链接。

HTTP：`POST /api/v1/share/create`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `shareName` | `string` | 是 | 分享名称 |
| `shareExpire` | `number` | 是 | `1`、`7`、`30`、`0`，`0` 表示永久 |
| `fileIDList` | `string` | 是 | 文件 ID 字符串，英文逗号分隔，最多 100 个 |
| `sharePwd` | `string` | 否 | 提取码 |
| `trafficSwitch` | `number` | 否 | 分享提取流量包开关，按官方枚举 |
| `trafficLimitSwitch` | `number` | 否 | 流量限制开关 |
| `trafficLimit` | `number` | 否 | 限制流量，单位 byte |

返回：`shareID`、`shareKey`。

示例：

```ts
const share = await client.share.create({
  shareName: 'share-name',
  shareExpire: 7,
  fileIDList: '123456'
});
```

### `client.share.list(params)`

用途：获取普通分享列表。

HTTP：`GET /api/v1/share/list`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `limit` | `number` | 是 | 每页数量，最大 100 |
| `lastShareId` | `number` | 否 | 翻页游标 |

返回：`shareList`、`lastShareId`。

示例：

```ts
const list = await client.share.list({ limit: 100 });
```

### `client.share.update(params)`

用途：修改普通分享链接配置。

HTTP：`PUT /api/v1/share/list/info`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `shareIdList` | `number[]` | 是 | 分享 ID 数组，最多 100 个 |
| `trafficSwitch` | `number` | 否 | 分享提取流量包开关 |
| `trafficLimitSwitch` | `number` | 否 | 流量限制开关 |
| `trafficLimit` | `number` | 否 | 限制流量，单位 byte |

返回：成功通常为 `null`。

示例：

```ts
await client.share.update({ shareIdList: [123456], trafficLimitSwitch: 1 });
```

### `client.share.createPaid(params)`

用途：创建付费分享链接。

HTTP：`POST /api/v1/share/content-payment/create`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `shareName` | `string` | 是 | 分享名称，小于 35 字符 |
| `fileIDList` | `string` | 是 | 文件 ID 字符串，英文逗号分隔，最多 100 个 |
| `payAmount` | `number` | 是 | 付费金额，整数，1 到 1000 |
| `isReward` | `number` | 否 | 是否开启打赏，`0` 否，`1` 是 |
| `resourceDesc` | `string` | 否 | 资源描述 |
| `trafficSwitch` | `number` | 否 | 分享提取流量包开关 |
| `trafficLimitSwitch` | `number` | 否 | 流量限制开关 |
| `trafficLimit` | `number` | 否 | 限制流量，单位 byte |

返回：`shareID`、`shareKey`。

示例：

```ts
const paid = await client.share.createPaid({
  shareName: 'paid-share',
  fileIDList: '123456',
  payAmount: 10
});
```

### `client.share.listPaid(params)`

用途：获取付费分享列表。

HTTP：`GET /api/v1/share/payment/list`

参数：同 `client.share.list(params)`。

返回：`shareList`、`lastShareId`。

示例：

```ts
const paidList = await client.share.listPaid({ limit: 100 });
```

### `client.share.updatePaid(params)`

用途：修改付费分享链接配置。

HTTP：`PUT /api/v1/share/list/payment/info`

参数：同 `client.share.update(params)`。

返回：成功通常为 `null`。

示例：

```ts
await client.share.updatePaid({ shareIdList: [123456], trafficLimitSwitch: 1 });
```

## Offline

### `client.offline.createDownloadTask(params)`

用途：创建离线下载任务。

HTTP：`POST /api/v1/offline/download`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `url` | `string` | 是 | http/https 下载资源地址 |
| `fileName` | `string` | 否 | 自定义文件名 |
| `dirID` | `number` | 否 | 指定下载目录；官方说明不支持根目录 |
| `callBackUrl` | `string` | 否 | 下载成功或失败后的回调地址 |

返回：`taskID`。

示例：

```ts
const task = await client.offline.createDownloadTask({ url: 'https://example.com/file.zip' });
```

### `client.offline.getDownloadProcess(params)`

用途：查询离线下载进度。

HTTP：`GET /api/v1/offline/download/process`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `taskID` | `number` | 是 | 离线下载任务 ID |

返回：

| 字段 | 类型 | 说明 |
| --- | --- | --- |
| `process` | `number` | 下载进度百分比 |
| `status` | `number` | `0` 进行中，`1` 失败，`2` 成功，`3` 重试中 |

示例：

```ts
const progress = await client.offline.getDownloadProcess({ taskID: 394756 });
```

## User

### `client.user.info()`

用途：获取用户信息。

HTTP：`GET /api/v1/user/info`

参数：无。

返回：用户信息。

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

```ts
const user = await client.user.info();
```

## DirectLink

### `client.directLink.enable(params)`

用途：启用直链空间。

HTTP：`POST /api/v1/direct-link/enable`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileID` | `number` | 是 | 要启用直链空间的文件夹 ID |

返回：`filename`，启用成功的文件夹名。

示例：

```ts
await client.directLink.enable({ fileID: 123456 });
```

### `client.directLink.disable(params)`

用途：禁用直链空间。

HTTP：`POST /api/v1/direct-link/disable`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileID` | `number` | 是 | 要禁用直链空间的文件夹 ID |

返回：`filename`。

示例：

```ts
await client.directLink.disable({ fileID: 123456 });
```

### `client.directLink.url(params)`

用途：获取文件直链 URL。

HTTP：`GET /api/v1/direct-link/url`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileID` | `number` | 是 | 文件 ID |

返回：`url`。

示例：

```ts
const link = await client.directLink.url({ fileID: 123456 });
```

### `client.directLink.refreshCache()`

用途：刷新直链缓存。

HTTP：`POST /api/v1/direct-link/cache/refresh`

参数：无。

返回：空对象。

示例：

```ts
await client.directLink.refreshCache();
```

### `client.directLink.getTrafficLogs(params)`

用途：获取直链流量日志。

HTTP：`GET /api/v1/direct-link/log`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `pageNum` | `number` | 是 | 页码 |
| `pageSize` | `number` | 是 | 每页数量 |
| `startTime` | `string` | 是 | 开始时间，如 `2025-01-01 00:00:00` |
| `endTime` | `string` | 是 | 结束时间，如 `2025-01-01 23:59:59` |

返回：`total`、`list`。

示例：

```ts
const logs = await client.directLink.getTrafficLogs({
  pageNum: 1,
  pageSize: 100,
  startTime: '2025-01-01 00:00:00',
  endTime: '2025-01-01 23:59:59'
});
```

### `client.directLink.getOfflineLogs(params)`

用途：获取直链离线日志。

HTTP：`GET /api/v1/direct-link/offline/logs`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `startHour` | `string` | 是 | 开始小时，如 `2025010115` |
| `endHour` | `string` | 是 | 结束小时，如 `2025010116` |
| `pageNum` | `number` | 是 | 页码 |
| `pageSize` | `number` | 是 | 每页数量 |

返回：`total`、`list`。

示例：

```ts
const logs = await client.directLink.getOfflineLogs({
  startHour: '2025010115',
  endHour: '2025010116',
  pageNum: 1,
  pageSize: 100
});
```

### `client.directLink.setIpBlacklistEnabled(params)`

用途：启用或禁用 IP 黑名单。

HTTP：`POST /api/v1/developer/config/forbide-ip/switch`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `Status` | `number` | 是 | `1` 启用，`2` 禁用 |

返回：`Done`。

示例：

```ts
await client.directLink.setIpBlacklistEnabled({ Status: 1 });
```

### `client.directLink.updateIpBlacklist(params)`

用途：更新 IP 黑名单列表。

HTTP：`POST /api/v1/developer/config/forbide-ip/update`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `IpList` | `string[]` | 是 | IPv4 地址列表，最多 2000 个 |

返回：空对象。

示例：

```ts
await client.directLink.updateIpBlacklist({ IpList: ['192.168.1.1'] });
```

### `client.directLink.listIpBlacklist(params)`

用途：获取 IP 黑名单列表。

HTTP：`GET /api/v1/developer/config/forbide-ip/list`

参数：无。SDK 方法接受 `params` 是为了兼容扩展，可传 `{}`。

返回：`ipList`、`status`。

示例：

```ts
const list = await client.directLink.listIpBlacklist({});
```

## Oss 图床

### `client.oss.mkdir(params)`

用途：创建图床目录。

HTTP：`POST /upload/v1/oss/file/mkdir`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `name` | `string[]` | 是 | 目录名数组 |
| `parentID` | `string` | 是 | 父目录 ID，根目录为空字符串 |
| `type` | `number` | 是 | 固定为 `1` |

返回：`list`，含创建的目录信息。

示例：

```ts
await client.oss.mkdir({ name: ['images'], parentID: '', type: 1 });
```

### `client.oss.create(params)`

用途：创建图床文件上传任务。

HTTP：`POST /upload/v1/oss/file/create`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `parentFileID` | `string` | 是 | 父目录 ID，根目录为空字符串 |
| `filename` | `string` | 是 | 文件名 |
| `etag` | `string` | 是 | 文件 MD5 |
| `size` | `number` | 是 | 文件大小 |
| `type` | `number` | 是 | 固定为 `1` |

返回：`reuse`、`fileID`、`preuploadID`、`sliceSize`。

示例：

```ts
await client.oss.create({
  parentFileID: '',
  filename: 'image.jpg',
  etag: 'md5',
  size: 1024,
  type: 1
});
```

### `client.oss.getUploadUrl(params)`

用途：获取图床上传分片地址。

HTTP：`POST /upload/v1/oss/file/get_upload_url`

参数：`preuploadID`、`sliceNo`。

返回：`presignedURL`。

示例：

```ts
await client.oss.getUploadUrl({ preuploadID: 'pre-id', sliceNo: 1 });
```

### `client.oss.complete(params)`

用途：图床上传完成。

HTTP：`POST /upload/v1/oss/file/upload_complete`

参数：`preuploadID`。

返回：`async`、`completed`、`fileID`。

示例：

```ts
await client.oss.complete({ preuploadID: 'pre-id' });
```

### `client.oss.asyncResult(params)`

用途：轮询图床上传异步结果。

HTTP：`POST /upload/v1/oss/file/upload_async_result`

参数：`preuploadID`。

返回：`completed`、`fileID`。

示例：

```ts
await client.oss.asyncResult({ preuploadID: 'pre-id' });
```

### `client.oss.createCopyTask(params)`

用途：从云盘复制图片到图床。

HTTP：`POST /api/v1/oss/source/copy`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileIDs` | `string[]` | 是 | 云盘文件 ID 数组 |
| `toParentFileID` | `string` | 是 | 图床目标目录 ID，根目录为空 |
| `sourceType` | `string` | 是 | 固定为 `"1"`，表示云盘 |
| `type` | `number` | 是 | 固定为 `1` |

返回：`taskID`。

示例：

```ts
await client.oss.createCopyTask({
  fileIDs: ['123456'],
  toParentFileID: '',
  sourceType: '1',
  type: 1
});
```

### `client.oss.getCopyProcess(params)`

用途：查询图床复制任务状态。

HTTP：`GET /api/v1/oss/source/copy/process`

参数：`taskID`。

返回：`status`、`failMsg`。

示例：

```ts
await client.oss.getCopyProcess({ taskID: 'task-id' });
```

### `client.oss.getCopyFailList(params)`

用途：获取图床复制失败列表。

HTTP：`GET /api/v1/oss/source/copy/fail`

参数：`taskID`、`limit`、`page`。

返回：`total`、`list`。

示例：

```ts
await client.oss.getCopyFailList({ taskID: 'task-id', limit: 100, page: 1 });
```

### `client.oss.move(params)`

用途：移动图床图片。

HTTP：`POST /api/v1/oss/file/move`

参数：`fileIDs`、`toParentFileID`。

返回：成功通常为 `null`。

示例：

```ts
await client.oss.move({ fileIDs: ['file-id'], toParentFileID: 'target-dir-id' });
```

### `client.oss.delete(params)`

用途：删除图床图片。

HTTP：`POST /api/v1/oss/file/delete`

参数：`fileIDs`，最多 100 个。

返回：成功通常为 `null`。

示例：

```ts
await client.oss.delete({ fileIDs: ['file-id'] });
```

### `client.oss.detail(params)`

用途：获取图床图片详情。

HTTP：`GET /api/v1/oss/file/detail`

参数：`fileID` 字符串。

返回：图片详情，常见字段含 `fileId`、`filename`、`downloadURL`、`userSelfURL`、`totalTraffic`。

示例：

```ts
await client.oss.detail({ fileID: 'file-id' });
```

### `client.oss.list(params)`

用途：获取图床图片列表。

HTTP：`POST /api/v1/oss/file/list`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `limit` | `number` | 是 | 每页数量，最大 100 |
| `type` | `number` | 是 | 固定为 `1` |
| `parentFileId` | `string` | 否 | 父目录 ID，根目录可为空 |
| `startTime` | `number` | 否 | 开始时间戳 |
| `endTime` | `number` | 否 | 结束时间戳 |
| `lastFileId` | `string` | 否 | 翻页游标 |

返回：`lastFileId`、`fileList`。

示例：

```ts
await client.oss.list({ limit: 100, type: 1 });
```

### `client.oss.createOfflineMigration(params)`

用途：创建图床离线迁移任务。

HTTP：`POST /api/v1/oss/offline/download`

参数：`url`、`type: 1`，可选 `fileName`、`businessDirID`、`callBackUrl`。

返回：`taskID`。

示例：

```ts
await client.oss.createOfflineMigration({
  url: 'https://example.com/image.jpg',
  type: 1
});
```

### `client.oss.getOfflineMigration(params)`

用途：查询图床离线迁移进度。

HTTP：`GET /api/v1/oss/offline/download/process`

参数：`taskID`。

返回：`process`、`status`。

示例：

```ts
await client.oss.getOfflineMigration({ taskID: 403316 });
```

## Transcode

### `client.transcode.listCloudDiskVideos(params)`

用途：获取云盘视频文件列表。

HTTP：`GET /api/v2/file/list`

参数：同 `client.files.list(params)`，官方视频场景可额外传 `category: 2`。

返回：`FileListData`。

示例：

```ts
await client.transcode.listCloudDiskVideos({
  parentFileId: 0,
  limit: 100,
  category: 2
});
```

### `client.transcode.uploadFromCloudDisk(params)`

用途：从云盘空间导入视频到转码空间。

HTTP：`POST /api/v1/transcode/upload/from_cloud_disk`

参数：`fileId` 数组，一次最多 100 个。

返回：官方导入结果。

示例：

```ts
await client.transcode.uploadFromCloudDisk({ fileId: [{ fileId: 123456 }] });
```

### `client.transcode.listFiles(params)`

用途：获取转码空间文件列表。

HTTP：`GET /api/v2/file/list`

参数：同 `client.files.list(params)`，官方转码空间场景可额外传 `businessType: 2`。

返回：`FileListData`。

示例：

```ts
await client.transcode.listFiles({ parentFileId: 0, limit: 100, businessType: 2 });
```

### `client.transcode.folderInfo(params)`

用途：获取转码空间文件夹信息。

HTTP：`POST /api/v1/transcode/folder/info`

参数：官方未列出独立请求参数，传 `{}`。

返回：`fileID`。

示例：

```ts
await client.transcode.folderInfo({});
```

### `client.transcode.videoResolutions(params)`

用途：获取视频文件可转码分辨率。

HTTP：`POST /api/v1/transcode/video/resolutions`

参数：`fileId`。

返回：`IsGetResolution`、`Resolutions`、`NowOrFinishedResolutions`、`CodecNames`、`VideoTime`。

示例：

```ts
await client.transcode.videoResolutions({ fileId: 123456 });
```

注意：官方建议轮询查询，约 10 秒一次。

### `client.transcode.list(params)`

用途：获取视频转码列表，第三方挂载应用授权使用。

HTTP：`GET /api/v1/video/transcode/list`

参数：`fileId`。

返回：`status`、`list`。

示例：

```ts
await client.transcode.list({ fileId: 123456 });
```

### `client.transcode.transcode(params)`

用途：发起视频转码。

HTTP：`POST /api/v1/transcode/video`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `codecName` | `string` | 是 | 编码方式 |
| `videoTime` | `number` | 是 | 视频时长，单位秒 |
| `resolutions` | `string` | 是 | 分辨率，多个用逗号分隔，如 `2160P,1080P,720P` |

返回：官方提示字符串或对象。

示例：

```ts
await client.transcode.transcode({
  fileId: 123456,
  codecName: 'H.264',
  videoTime: 60,
  resolutions: '1080P,720P'
});
```

### `client.transcode.record(params)`

用途：查询某个视频的转码记录。

HTTP：`POST /api/v1/transcode/video/record`

参数：`fileId`。

返回：`UserTranscodeVideoRecordList`。

示例：

```ts
await client.transcode.record({ fileId: 123456 });
```

### `client.transcode.result(params)`

用途：查询某个视频的转码结果。

HTTP：`POST /api/v1/transcode/video/result`

参数：`fileId`。

返回：`UserTranscodeVideoList`。

示例：

```ts
await client.transcode.result({ fileId: 123456 });
```

### `client.transcode.delete(params)`

用途：删除转码视频。

HTTP：`POST /api/v1/transcode/delete`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `businessType` | `number` | 是 | 固定为 `2` |
| `trashed` | `number` | 是 | `1` 删除原文件，`2` 删除原文件和转码文件 |

返回：官方提示字符串。

示例：

```ts
await client.transcode.delete({ fileId: 123456, businessType: 2, trashed: 2 });
```

### `client.transcode.downloadOriginal(params)`

用途：获取转码空间原文件下载地址。

HTTP：`POST /api/v1/transcode/file/download`

参数：`fileId`。

返回：`downloadUrl`、`isFull`。

示例：

```ts
await client.transcode.downloadOriginal({ fileId: 123456 });
```

### `client.transcode.downloadM3u8OrTs(params)`

用途：下载单个转码文件，m3u8 或 ts。

HTTP：`POST /api/v1/transcode/m3u8_ts/download`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `resolution` | `string` | 是 | 分辨率 |
| `type` | `number` | 是 | `1` 下载 m3u8，`2` 下载 ts |
| `tsName` | `string` | 否 | 下载 ts 时必填 |

返回：`downloadUrl`、`isFull`。

示例：

```ts
await client.transcode.downloadM3u8OrTs({
  fileId: 123456,
  resolution: '1080P',
  type: 1
});
```

### `client.transcode.downloadAll(params)`

用途：下载某个视频全部转码文件。

HTTP：`POST /api/v1/transcode/file/download/all`

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `fileId` | `number` | 是 | 文件 ID |
| `zipName` | `string` | 是 | 下载 zip 文件名 |

返回：`isDownloading`、`isFull`、`downloadUrl`。

示例：

```ts
await client.transcode.downloadAll({ fileId: 123456, zipName: 'video-transcode.zip' });
```

## Low-Level Request

### `client.request(method, path, options?)`

用途：SDK 未封装新接口时的底层请求。

参数：

| 参数 | 类型 | 必填 | 说明 |
| --- | --- | --- | --- |
| `method` | `'GET' \| 'POST' \| 'PUT' \| 'DELETE' \| 'PATCH'` | 是 | HTTP 方法 |
| `path` | `string` | 是 | API 路径 |
| `options.query` | `object` | 否 | URL query |
| `options.body` | `unknown` | 否 | JSON body |
| `options.form` | `FormData \| object` | 否 | multipart/form-data |
| `options.headers` | `Record<string,string>` | 否 | 额外请求头 |
| `options.baseURL` | `string` | 否 | 覆盖基础域名 |
| `options.auth` | `boolean` | 否 | 是否自动带 Bearer token，默认 `true` |
| `options.responseType` | `string` | 否 | axios responseType |

返回：`Promise<T>`，官方响应中的 `data`。

示例：

```ts
const data = await client.request('GET', '/api/v1/file/detail', {
  query: { fileID: 123456 }
});
```

## 错误处理

```ts
import { Pan123ApiError } from 'chest123-pan-sdk';

try {
  await client.files.detail({ fileID: 404 });
} catch (error) {
  if (error instanceof Pan123ApiError) {
    console.error(error.code);
    console.error(error.message);
    console.error(error.traceId);
    console.error(error.status);
    console.error(error.responseBody);
  }
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
npm install
npm run typecheck
npm test
npm run build
```

Live 测试：

```bash
PAN123_CLIENT_ID=your_client_id PAN123_CLIENT_SECRET=your_client_secret npm run test:live
```

Live 测试会上传 `../Test.txt`，不会删除或回收测试文件，也不会启用或禁用直链空间。
