# 123 云盘开放平台文档集

- 官方来源：[123云盘开放平台](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced)
- 生成依据：官方语雀目录中的 86 个 DOC 页面。
- 官方页面更新时间：2026-04-26。
- 接口正式域名：`https://open-api.123pan.com`。
- 授权正式域名：`https://www.123pan.com`。

## 公共请求头

| 名称 | 值 | 说明 |
| --- | --- | --- |
| `Platform` | `open_platform` | 开放平台固定值 |
| `Authorization` | `Bearer <access_token>` | 除获取 token 等少数接口外，业务接口均需携带 |
| `Content-Type` | `application/json` 或 `multipart/form-data` | 按接口类型选择 |

## 鉴权模式

- 开发者接入：用 `clientID` 和 `clientSecret` 调用 `/api/v1/access_token`，缓存返回的 `accessToken`，按 `expiredAt` 刷新。
- 第三方挂载/OAuth：跳转 `https://www.123pan.com/auth` 获取 `code`，再调用 `/api/v1/oauth2/access_token` 换取 `access_token` 和一次性 `refresh_token`。
- 所有业务调用优先检查响应体 `code` 与 `message`；常见错误包括 `401` token 无效、`429` 请求过频、`5066` 文件不存在、`5113` 流量超限。

## 模块导航

- [接入与授权](./01-access-and-oauth.md)
- [文件管理](./02-file-management.md)
- [上传 V1（旧）](./03-upload-v1-legacy.md)
- [上传 V2（推荐）](./04-upload-v2-recommended.md)
- [分享、离线下载与用户管理](./05-share-offline-user.md)
- [直链](./06-direct-link.md)
- [图床](./07-image-hosting.md)
- [视频转码](./08-video-transcoding.md)
- [接口总索引](./99-endpoint-index.md)
