# @chest123/pan-sdk

123 云盘开放平台 Node.js SDK。SDK 目标是让服务端开发者少写 HTTP 对接细节：初始化客户端后，直接调用模块方法即可完成鉴权、文件管理、上传、下载链接、直链、图床、分享、离线下载和视频转码等操作。

本 SDK 依据仓库内 `../123PanDoc` 的官方文档整理实现，不会自行编造接口字段。方法参数尽量保持 123 云盘官方字段名，例如 `fileID`、`fileId`、`parentFileID`、`parentFileId`、`preuploadID`。

## 特性

- TypeScript 编写，输出 ESM、CommonJS 和类型声明。
- 自动获取、缓存、刷新 `access_token`。
- 自动添加 `Platform: open_platform` 和 `Authorization: Bearer <token>`。
- 统一处理 123 云盘响应体，`code !== 0` 时抛出 `Pan123ApiError`。
- 完整封装当前官方文档中的 HTTP API，并提供常用高层 helper。
- `upload.uploadFile(...)` 支持 V2 单步上传和分片上传流程。
- 保留 `client.request(...)` 底层方法，方便临时调用新增接口。

## 环境要求

- Node.js `>= 18`
- 推荐 TypeScript 项目使用；JavaScript 项目也可以直接使用。

## 安装

```bash
npm install @chest123/pan-sdk
```

当前仓库本地开发：

```bash
cd sdk-nodejs
npm install
npm run build
```

## 快速开始

```ts
import { createPan123Client } from '@chest123/pan-sdk';

const client = createPan123Client({
  clientId: process.env.PAN123_CLIENT_ID,
  clientSecret: process.env.PAN123_CLIENT_SECRET
});

const user = await client.user.info();
console.log(user);

const root = await client.files.list({
  parentFileId: 0,
  limit: 100
});
console.log(root.fileList);
```

## 初始化客户端

```ts
import { Pan123Client } from '@chest123/pan-sdk';

const client = new Pan123Client({
  clientId: 'your_client_id',
  clientSecret: 'your_client_secret',
  timeoutMs: 60000
});
```

可选配置：

| 参数 | 类型 | 默认值 | 说明 |
| --- | --- | --- | --- |
| `clientId` | `string` | 无 | 123 云盘开放平台 `clientID` |
| `clientSecret` | `string` | 无 | 123 云盘开放平台 `clientSecret` |
| `accessToken` | `string` | 无 | 已有 token，可跳过首次获取 |
| `tokenExpiresAt` | `string \| Date \| number` | 无 | token 过期时间 |
| `baseURL` | `string` | `https://open-api.123pan.com` | API 基础地址 |
| `platform` | `string` | `open_platform` | 官方要求的 Platform 值 |
| `timeoutMs` | `number` | `30000` | 请求超时时间 |

## 鉴权

SDK 会在业务请求前自动调用 `/api/v1/access_token` 并缓存 token。token 接近过期时会自动重新获取。

手动获取 token：

```ts
const token = await client.auth.getAccessToken();
console.log(token.accessToken, token.expiredAt);
```

如果你已经有 token：

```ts
client.auth.setAccessToken(existingToken, expiredAt);
```

OAuth token：

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

### 获取文件列表

```ts
const list = await client.files.list({
  parentFileId: 0,
  limit: 100
});

for (const item of list.fileList) {
  console.log(item.fileId, item.filename, item.type, item.trashed);
}
```

注意：官方推荐的 `/api/v2/file/list` 可能返回回收站文件，请根据 `trashed` 字段自行过滤。

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

这些方法是官方接口的薄封装，参数字段请按 123 云盘官方文档传入。

## 上传文件

推荐使用 V2 上传。

### 高层上传 helper

```ts
const uploaded = await client.upload.uploadFile({
  filePath: './Test.txt',
  parentFileID: 0,
  duplicate: 1
});

console.log(uploaded.fileID);
```

`uploadFile` 行为：

- 自动计算文件 MD5 作为 `etag`。
- 文件名默认使用 `path.basename(filePath)`。
- 小于等于 1GB 时走 V2 单步上传。
- 大于 1GB 时走 V2 分片上传。
- 分片上传会按官方返回的 `sliceSize` 切片，并为每片计算 `sliceMD5`。
- `duplicate` 不默认传；如果想保留同名文件，建议显式传 `duplicate: 1`。

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

// created.reuse 为 true 时表示秒传完成。
// 非秒传时，按 created.sliceSize 切片，并调用 client.upload.slice(...)
// 所有分片上传后调用 client.upload.complete(...)
```

### V1 旧上传接口

```ts
await client.uploadV1.create(params);
await client.uploadV1.getUploadUrl(params);
await client.uploadV1.listUploadParts(params);
await client.uploadV1.complete(params);
await client.uploadV1.asyncResult(params);
```

## 下载文件

SDK 负责获取下载链接；真正下载文件可以用 Node.js 原生 `fetch` 或其他下载工具。

```ts
import { writeFile } from 'node:fs/promises';

const info = await client.files.downloadInfo({ fileId: 123456 });
const response = await fetch(info.downloadUrl);

if (!response.ok) {
  throw new Error(`download failed: ${response.status}`);
}

const bytes = Buffer.from(await response.arrayBuffer());
await writeFile('./downloaded-file.bin', bytes);
```

## 直链

123 云盘直链是文件夹级能力。通常需要先对文件所在文件夹启用直链空间，再为文件获取直链 URL。

```ts
await client.directLink.enable({ fileID: folderId });

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

如果官方新增接口但 SDK 还没封装，可以临时使用底层方法：

```ts
const data = await client.request('GET', '/api/v1/file/detail', {
  query: { fileID: 123456 }
});
```

规则：

- `query` 会作为 URL 查询参数。
- `body` 会作为 JSON body。
- `form` 会作为 multipart/form-data。
- 默认自动鉴权；如果是无需鉴权接口，可传 `auth: false`。

## 错误处理

当 123 云盘返回 HTTP 错误或响应体 `code !== 0` 时，SDK 会抛出 `Pan123ApiError`。

```ts
import { Pan123ApiError } from '@chest123/pan-sdk';

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

## 测试

安装依赖：

```bash
npm install
```

类型检查：

```bash
npm run typecheck
```

单元测试：

```bash
npm test
```

Live 测试默认不会执行真实网络请求，只有设置环境变量时才会运行：

```bash
PAN123_CLIENT_ID=your_client_id PAN123_CLIENT_SECRET=your_client_secret npm run test:live
```

Live 测试会：

- 获取 `access_token`。
- 获取用户信息。
- 列根目录。
- 上传 `../Test.txt`，文件名会加时间戳。
- 获取上传文件详情。
- 获取上传文件下载链接。

Live 测试不会：

- 删除或回收测试文件。
- 启用或禁用直链空间。
- 修改你的直链配置。

## 构建

```bash
npm run build
```

构建产物：

- `dist/index.js`：ESM
- `dist/index.cjs`：CommonJS
- `dist/index.d.ts`：TypeScript 类型声明

## Git 与发布说明

`dist/` 是构建产物，默认不建议提交到 git。本仓库的推荐维护方式是：

- 提交 `src/`、`test/`、`examples/`、`package.json`、`package-lock.json`、配置文件和 README。
- 不提交 `node_modules/`、`dist/`、`.env`。
- 发布 npm 前先执行 `npm run build`。
- `package.json` 中的 `files` 字段会确保发布包包含 `dist` 和 `README.md`。

如果未来需要支持用户直接从 git 安装，例如 `npm install git+https://...`，可以二选一：

- 提交 `dist/`。
- 或添加 `prepare` 脚本，让 git 安装时自动构建。

当前项目按 npm 发布包方式维护，所以 `dist/` 不同步到 git。
