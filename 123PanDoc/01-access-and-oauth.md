# 接入与授权

官方来源：[123云盘开放平台](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced)

## 本模块接口

- 说明页 无 - 概述
- 说明页 无 - 更新记录
- 说明页 无 - 接入流程
- 说明页 无 - 开发须知
- POST /api/v1/access_token - 获取access_token
- 说明页 无 - 授权须知
- 说明页 无 - 授权地址
- POST /api/v1/oauth2/access_token - 授权code获取access_token
- 说明页 无 - 优秀实践
- 说明页 无 - 常见问题

## 详细说明

### 概述

- 用途：概述。
- HTTP：无独立 HTTP 接口
- 官方来源：[概述](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ppsuasz6rpioqbyt)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 123云盘提供简单好用的云存储接口，同时满足开发者的各种需求。
- 支持大文件断点续传与并行上传
- 获取文件列表

### 更新记录

- 用途：更新记录。
- HTTP：无独立 HTTP 接口
- 官方来源：[更新记录](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ewgaoswrngr1amb1)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 接入流程

- 用途：接入流程。
- HTTP：无独立 HTTP 接口
- 官方来源：[接入流程](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/hpengmyg32blkbg8)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 完成上述步骤后，就可以调用 API 了

### 开发须知

- 用途：开发须知。
- HTTP：无独立 HTTP 接口
- 官方来源：[开发须知](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/txgcvbfgh0gtuad5)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
| **参数名称** | **是否必填** | **类型** | **示例值** | **描述** |
| :---: | :---: | :---: | :---: | --- |
| code | 是 | int | 0 | code字段等于0 标识成功响应，其他code为失败响应 |
| message | 是 | string | ok | 请求成功为ok；异常时为具体异常信息 |
| data | 是 | any | - | 返回的响应内容；异常时为null |
| x-traceID | 是 | string | eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9 | 接口响应异常需要技术支持请提供接口返回的x-traceID |

| **body 中的 code** | **描述** |
| :---: | :---: |
| 401 | access_token无效 |
| 429 | 请求太频繁 |

| **API** | **QPS** |
| --- | :---: |
| api/v1/user/info | 10 |
| api/v1/file/move | 20 |
| api/v1/file/delete | 10 |
| api/v1/file/list | 10 |
| api/v2/file/list | 15 |
| upload/v1/file/mkdir | 20 |
| api/v1/access_token | 10 |
| api/v1/transcode/folder/info  | 20 |
| api/v1/transcode/upload/from_cloud_disk  | 1 |
| api/v1/transcode/delete  | 10 |
| api/v1/transcode/video/resolutions  | 1 |
| api/v1/transcode/video | 3 |
| api/v1/transcode/video/record  | 20 |
| api/v1/transcode/video/result  | 20 |
| api/v1/transcode/file/download  | 10 |
| api/v1/transcode/m3u8_ts/download  | 20 |
| api/v1/transcode/file/download/all  | 1 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": null,
  "x-traceID":"..."
}
```

**注意事项**
- 调用服务端接口时，需使用 HTTPS 协议、JSON 数据格式、UTF-8 编码，POST 请求请在 HTTP Header 中设置 Content-Type:application/json。

### 获取access_token

- 用途：获取access_token。
- HTTP：POST 域名 +/api/v1/access_token
- 官方来源：[获取access_token](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/gn1nai4x0v0ry9ki)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
|  Platform | string | 是 |  open_platform  |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| clientID | string | 必填 |  |
| clientSecret | string | 必填 |  |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| accessToken | string | 必填 | 访问凭证 |
| expiredAt | string | 必填 | access_token过期时间 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyxxxxx...(过长已省略)",
    "expiredAt": "2025-03-23T15:48:37+08:00"
  },
  "x-traceID": "..."
}
```

**注意事项**
- 此接口有访问频率限制。请获取到access_token后本地保存使用，并在access_token过期前及时重新获取。access_token有效期根据返回的expiredAt字段判断。

### 授权须知

- 用途：授权须知。
- HTTP：无独立 HTTP 接口
- 官方来源：[授权须知](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/kf05anzt1r0qnudd)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
| **body 中的 code** | **描述** |
| :---: | :---: |
| 1 | 内部错误 |
| 401 | access_token无效 |
| 429 | 请求太频繁 |
| 5066 | 文件不存在 |
| 5113 | 流量超限 |

| **API** | **限制QPS（同一个uid，每秒最大请求次数）** |
| --- | --- |
| upload/v1/file/create | 5 |
| upload/v1/file/get_upload_url | 20 |
| upload/v1/file/list_upload_parts | 20 |
| upload/v1/file/mkdir | 5 |
| upload/v1/file/upload_async_result | 5 |
| upload/v1/file/upload_complete | 20 |
| api/v1/file/delete | 1 |
| api/v1/access_token | 8 |
| api/v1/file/list | 1 |
| api/v1/file/trash | 5 |
| api/v1/file/move | 10 |
| api/v1/user/info | 10 |
| api/v2/file/list | 15 |
| api/v1/video/transcode/list | 1 |
| api/v1/file/infos | 10 |
| api/v1/file/download_info | 5 |

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 授权正式域名：[https://www.123pan.com](about:blank)
- 为了确保对接的高效性，对接应用需提供如下应用信息：
- 申请人经123云盘商务邮箱 bd@123pan.com对接确认，并向123云盘提供“应用授权回调地址”和“对接应用的logo（圆角）“
- 123云盘收到所提供的信息后，会向申请人提供`appId`，`secretId`
- 申请人开始对接，过程中如有任何疑问，可与商务进行沟通确认

### 授权地址

- 用途：授权地址。
- HTTP：无独立 HTTP 接口
- 官方来源：[授权地址](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/gr7ggimkcysm18ap)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 授权code获取access_token

- 用途：授权code获取access_token。
- HTTP：POST 域名 + /api/v1/oauth2/access_token
- 官方来源：[授权code获取access_token](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/gammzlhe6k4qtwd9)

**鉴权**
默认使用 `Authorization: Bearer <access_token>` 与 `Platform: open_platform`，以官方页面为准。

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| client_id | string | 是 | 应用标识，创建应用时分配的 appId |
| client_secret | string | 是 | 应用密钥，创建应用时分配的 secretId |
| grant_type | string | 是 | 身份类型 authorization_code 或 refresh_token |
| code | string | 否 | 授权码 |
| refresh_token | string | 否 | 刷新 token，单次请求有效 |
| redirect_uri | string | 否 | authorization_code时必携带，应用注册的回调地址 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| token_type | string | 是 | Bearer |
| access_token | string | 是 | 用来获取用户信息的 access_token。 刷新后，旧 access_token 立即失效 |
| refresh_token | string | 是 | 单次有效，用来刷新 access_token，90 天有效期。刷新后，返回新的 refresh_token，请保存以便下一次刷新使用。 |
| expires_in | number | 是 | access_token的过期时间，单位秒。 |
| scope | string | 是 | 权限 |

**成功示例结构**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCIs...(过长省略)",
  "expires_in": 7200,
  "refresh_token": "...",
  "scope": "user:base,file:all:read,file:all:write",
  "token_type": "Bearer"
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 优秀实践

- 用途：优秀实践。
- HTTP：无独立 HTTP 接口
- 官方来源：[优秀实践](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/gg705bew0t80ccse)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 常见问题

- 用途：常见问题。
- HTTP：无独立 HTTP 接口
- 官方来源：[常见问题](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ghfd4h0l6c6y6oi8)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。
