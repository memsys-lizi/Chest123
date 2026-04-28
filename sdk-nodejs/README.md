# chest123-pan-sdk

123 云盘开放平台 Node.js SDK。它面向服务端开发者，封装了鉴权、文件管理、上传、下载链接、直链、分享、离线下载、图床和视频转码等接口，减少直接拼 HTTP 请求的工作量。

SDK 依据仓库中的 [`../123PanDoc`](../123PanDoc) 官方 API 整理文档实现。方法参数尽量保持官方字段名，例如 `fileID`、`fileId`、`parentFileID`、`parentFileId`、`preuploadID`，方便和官方文档互相对照。

## 安装

```bash
npm install chest123-pan-sdk
```

要求 Node.js `>= 18`。

## 快速开始

```ts
import { createPan123Client } from 'chest123-pan-sdk';

const client = createPan123Client({
  clientId: process.env.PAN123_CLIENT_ID,
  clientSecret: process.env.PAN123_CLIENT_SECRET
});

const user = await client.user.info();

const root = await client.files.list({
  parentFileId: 0,
  limit: 100
});

console.log(user);
console.log(root.fileList);
```

## 客户端配置

```ts
import { Pan123Client } from 'chest123-pan-sdk';

const client = new Pan123Client({
  clientId: 'your_client_id',
  clientSecret: 'your_client_secret',
  timeoutMs: 60000
});
```

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `clientId` | `string` | 无 | 123 云盘开放平台 `clientID` |
| `clientSecret` | `string` | 无 | 123 云盘开放平台 `clientSecret` |
| `accessToken` | `string` | 无 | 已有 access token |
| `tokenExpiresAt` | `string \| Date \| number` | 无 | access token 过期时间 |
| `baseURL` | `string` | `https://open-api.123pan.com` | API 基础地址 |
| `platform` | `string` | `open_platform` | 123 云盘开放平台固定请求头 |
| `timeoutMs` | `number` | `30000` | 请求超时时间 |

SDK 会自动获取并缓存 `access_token`，业务请求会自动带上：

```text
Platform: open_platform
Authorization: Bearer <access_token>
```

## 鉴权

通常不需要手动调用鉴权，SDK 会在第一次业务请求前自动获取 token。

手动获取 token：

```ts
const token = await client.auth.getAccessToken();
console.log(token.accessToken);
console.log(token.expiredAt);
```

使用已有 token：

```ts
client.auth.setAccessToken(existingToken, expiredAt);
```

OAuth 授权换 token：

```ts
const oauth = await client.auth.getOAuthToken({
  client_id: 'app_id',
  client_secret: 'secret_id',
  grant_type: 'authorization_code',
  code: 'authorization_code',
  redirect_uri: 'https://example.com/callback'
});
```

## 文件管理

### 列出文件

```ts
const list = await client.files.list({
  parentFileId: 0,
  limit: 100
});

for (const file of list.fileList) {
  console.log(file.fileId, file.filename, file.type, file.trashed);
}
```

官方推荐的 `/api/v2/file/list` 可能返回回收站文件，可以根据 `trashed` 字段过滤。

### 创建目录

```ts
const dir = await client.files.mkdir({
  name: 'SDK测试目录',
  parentID: 0
});

console.log(dir.dirID);
```

### 获取文件详情

```ts
const detail = await client.files.detail({
  fileID: 123456
});
```

### 获取下载链接

```ts
const download = await client.files.downloadInfo({
  fileId: 123456
});

console.log(download.downloadUrl);
```

### 其他文件接口

```ts
await client.files.rename(params);
await client.files.batchRename(params);
await client.files.trash(params);
await client.files.copy(params);
await client.files.asyncCopy(params);
await client.files.asyncCopyProcess(params);
await client.files.recover(params);
await client.files.recoverByPath(params);
await client.files.infos(params);
await client.files.listLegacy(params);
await client.files.move(params);
```

这些方法是官方接口的薄封装，`params` 请按 123 云盘官方字段传入。

## 上传文件

推荐使用 V2 上传。

### 一行上传

```ts
const uploaded = await client.upload.uploadFile({
  filePath: './Test.txt',
  parentFileID: 0,
  duplicate: 1
});

console.log(uploaded.fileID);
```

`uploadFile` 会自动：

- 计算文件 MD5 作为 `etag`。
- 使用 `path.basename(filePath)` 作为默认文件名。
- 小于等于 1GB 时走 V2 单步上传。
- 大于 1GB 时走 V2 分片上传。
- 按官方返回的 `sliceSize` 切片并计算每片 `sliceMD5`。

### V2 单步上传

```ts
const domains = await client.upload.domain();

const result = await client.upload.single({
  uploadURL: domains[0],
  filePath: './Test.txt',
  parentFileID: 0,
  filename: 'Test.txt',
  etag: 'file_md5',
  size: 33,
  duplicate: 1
});
```

### V2 分片上传

```ts
const created = await client.upload.create({
  parentFileID: 0,
  filename: 'large.bin',
  etag: 'file_md5',
  size: 1024 * 1024 * 1024 * 2
});

if (created.reuse) {
  console.log('秒传完成', created.fileID);
} else {
  // 按 created.sliceSize 切片
  // 调用 client.upload.slice(...)
  // 所有分片上传后调用 client.upload.complete(...)
}
```

## 下载文件

SDK 提供下载链接获取。真正下载文件可以使用 Node.js 原生 `fetch`。

```ts
import { writeFile } from 'node:fs/promises';

const info = await client.files.downloadInfo({
  fileId: 123456
});

const response = await fetch(info.downloadUrl);
if (!response.ok) {
  throw new Error(`download failed: ${response.status}`);
}

const bytes = Buffer.from(await response.arrayBuffer());
await writeFile('./downloaded-file.bin', bytes);
```

下载链接通常是临时链接，建议获取后尽快使用。

## 直链

123 云盘直链是文件夹级能力。通常要先对文件所在文件夹启用直链空间，再获取文件直链。

```ts
await client.directLink.enable({
  fileID: folderId
});

const direct = await client.directLink.url({
  fileID: fileId
});

console.log(direct.url);
```

其他直链接口：

```ts
await client.directLink.disable({ fileID: folderId });
await client.directLink.refreshCache();
await client.directLink.getTrafficLogs(params);
await client.directLink.getOfflineLogs(params);
await client.directLink.setIpBlacklistEnabled(params);
await client.directLink.updateIpBlacklist(params);
await client.directLink.listIpBlacklist(params);
```

## 分享、离线下载、用户

```ts
await client.share.create(params);
await client.share.list(params);
await client.share.update(params);
await client.share.createPaid(params);
await client.share.listPaid(params);
await client.share.updatePaid(params);

await client.offline.createDownloadTask(params);
await client.offline.getDownloadProcess(params);

const user = await client.user.info();
```

## 图床 OSS

```ts
await client.oss.mkdir(params);
await client.oss.create(params);
await client.oss.getUploadUrl(params);
await client.oss.complete(params);
await client.oss.asyncResult(params);

await client.oss.createCopyTask(params);
await client.oss.getCopyProcess(params);
await client.oss.getCopyFailList(params);

await client.oss.move(params);
await client.oss.delete(params);
await client.oss.detail(params);
await client.oss.list(params);

await client.oss.createOfflineMigration(params);
await client.oss.getOfflineMigration(params);
```

## 视频转码

```ts
await client.transcode.listCloudDiskVideos({ parentFileId: 0, limit: 100 });
await client.transcode.uploadFromCloudDisk(params);
await client.transcode.listFiles({ parentFileId: 0, limit: 100 });
await client.transcode.folderInfo(params);
await client.transcode.videoResolutions(params);
await client.transcode.list(params);
await client.transcode.transcode(params);
await client.transcode.record(params);
await client.transcode.result(params);
await client.transcode.delete(params);
await client.transcode.downloadOriginal(params);
await client.transcode.downloadM3u8OrTs(params);
await client.transcode.downloadAll(params);
```

## 底层 request

如果官方新增接口但 SDK 尚未封装，可以使用底层请求方法。

```ts
const data = await client.request('GET', '/api/v1/file/detail', {
  query: { fileID: 123456 }
});
```

规则：

- `query` 会作为 URL 查询参数。
- `body` 会作为 JSON body。
- `form` 会作为 multipart/form-data。
- 默认自动鉴权。
- 无需鉴权的接口可以传 `auth: false`。

## 错误处理

当 HTTP 请求失败，或 123 云盘响应体 `code !== 0` 时，SDK 会抛出 `Pan123ApiError`。

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

## 本地开发与测试

```bash
npm install
npm run typecheck
npm test
npm run build
```

Live 测试需要真实 123 云盘开放平台凭证：

```bash
PAN123_CLIENT_ID=your_client_id PAN123_CLIENT_SECRET=your_client_secret npm run test:live
```

Live 测试会获取 token、读取用户信息、列根目录、上传 `../Test.txt`、获取上传文件详情和下载链接。它不会删除或回收测试文件，也不会启用或禁用直链空间。
