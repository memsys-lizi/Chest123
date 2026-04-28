# 123 云盘 SDK 项目

这是一个面向 123 云盘开放平台的 SDK 项目，目标是把官方开放 API 封装成更容易在业务服务端中使用的多语言 SDK。

当前已经完成 Node.js 版本 SDK，后续可以在同一仓库中继续增加 Go、C# 等其他语言实现。每种语言 SDK 都放在独立目录中，并维护自己的 README、测试和构建方式。

## 当前内容

| 路径 | 说明 |
| --- | --- |
| [`sdk-nodejs`](./sdk-nodejs) | Node.js / TypeScript SDK，npm 包名为 `@chest123/pan-sdk` |
| [`sdk-nodejs/README.md`](./sdk-nodejs/README.md) | Node.js SDK 使用文档 |
| [`123PanDoc`](./123PanDoc) | 123 云盘官方开放平台 API 的本地整理文档 |
| [`123PanDoc/99-endpoint-index.md`](./123PanDoc/99-endpoint-index.md) | API 接口总索引 |

## Node.js SDK

Node.js SDK 封装了 123 云盘开放平台的鉴权、文件管理、上传、下载链接、直链、分享、离线下载、图床、视频转码等能力。

最小示例：

```ts
import { createPan123Client } from '@chest123/pan-sdk';

const client = createPan123Client({
  clientId: process.env.PAN123_CLIENT_ID,
  clientSecret: process.env.PAN123_CLIENT_SECRET
});

const files = await client.files.list({
  parentFileId: 0,
  limit: 100
});
```

更多用法见 [`sdk-nodejs/README.md`](./sdk-nodejs/README.md)。

## 官方 API 文档

`123PanDoc` 是基于 123 云盘官方语雀文档整理出的本地 Markdown 文档集，方便开发 SDK 时查阅接口路径、请求参数、响应字段、上传流程和直链流程。

建议开发新语言 SDK 时先阅读：

- [`123PanDoc/00-README.md`](./123PanDoc/00-README.md)
- [`123PanDoc/99-endpoint-index.md`](./123PanDoc/99-endpoint-index.md)
- [`123PanDoc/04-upload-v2-recommended.md`](./123PanDoc/04-upload-v2-recommended.md)
- [`123PanDoc/06-direct-link.md`](./123PanDoc/06-direct-link.md)

## 仓库组织约定

新增语言 SDK 时建议按语言创建独立目录：

```text
sdk-nodejs/
sdk-go/
sdk-csharp/
```

每个 SDK 目录应独立包含：

- SDK 源码
- README 使用文档
- 示例代码
- 单元测试和必要的 live test
- 对应语言生态的包管理配置

通用 API 事实来源统一放在 `123PanDoc`，避免不同语言 SDK 对接口字段和行为理解不一致。
