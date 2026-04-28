# 直链

官方来源：[123云盘开放平台](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced)

## 本模块接口

- POST /api/v1/developer/config/forbide-ip/switch - 开启关闭ip黑名单
- POST /api/v1/developer/config/forbide-ip/update - 更新ip黑名单列表
- GET /api/v1/developer/config/forbide-ip/list - ip黑名单列表
- GET /api/v1/direct-link/offline/logs - 获取直链离线日志
- GET /api/v1/direct-link/log - 获取直链流量日志
- POST /api/v1/direct-link/enable - 启用直链空间
- GET /api/v1/direct-link/url - 获取直链链接
- POST /api/v1/direct-link/disable - 禁用直链空间
- POST /api/v1/direct-link/cache/refresh - 直链缓存刷新

## 详细说明

### 开启关闭ip黑名单

- 用途：开启关闭ip黑名单。
- HTTP：POST 域名 + /api/v1/developer/config/forbide-ip/switch
- 官方来源：[开启关闭ip黑名单](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/xwx77dbzrkxquuxm)

**鉴权**
| 名称 | 类型 | 是否必填 | 说明 |
| --- | --- | --- | --- |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |
| Content-Type | string | 必填 | application/json |

**请求参数**
| 名称 | 类型 | 是否必填 | 说明 |
| --- | --- | --- | --- |
| Status | number | 必填 | 状态：2禁用 1启用 |

**响应字段**
| 名称 | 类型 | 是否必填 | 说明 |
| --- | --- | --- | --- |
| Done | boolean | 必填 | 操作是否完成 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "Done": true
    },
    "x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 更新ip黑名单列表

- 用途：更新ip黑名单列表。
- HTTP：POST 域名 + /api/v1/developer/config/forbide-ip/update
- 官方来源：[更新ip黑名单列表](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/tt3s54slh87q8wuh)

**鉴权**
| 名称 | 类型 | 是否必填 | 说明 |
| --- | --- | --- | --- |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |
| Content-Type | string | 必填 | application/json |

**请求参数**
| 名称 | 类型 | 是否必填 | 说明 |
| --- | --- | --- | --- |
| IpList | array | 必填 | IP地址列表，最多2000个IPv4地址 |

**响应字段**
| 名称 | 类型 | 是否必填 | 说明 |
| --- | --- | --- | --- |
| - | - | - | 操作成功无特定返回数据 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {},
    "x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### ip黑名单列表

- 用途：ip黑名单列表。
- HTTP：GET 域名 + /api/v1/developer/config/forbide-ip/list
- 官方来源：[ip黑名单列表](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/mxldrm9d5gpw5h2d)

**鉴权**
| 名称 | 类型 | 是否必填 | 说明 |
| --- | --- | --- | --- |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
| 名称 | 类型 | 是否必填 | 说明 |
| --- | --- | --- | --- |
| ipList | array | 必填 | IP地址列表 |
| status | number | 必填 | 状态：2禁用 1启用 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "ipList": ["192.168.1.1", "10.0.0.1"],
        "status": 1
    },
    "x-traceID": ""
}
```

**注意事项**
- 获取开发者功能IP配置黑名单

### 获取直链离线日志

- 用途：获取直链离线日志。
- HTTP：GET 域名 + /api/v1/direct-link/offline/logs
- 官方来源：[获取直链离线日志](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/yz4bdynw9yx5erqb)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| startHour | string | 必填 | 开始时间，格式：2025010115 |
| endHour | string | 必填 | 结束时间，格式：2025010116 |
| pageNum | number | 必填 | 页数，从1开始 |
| pageSize | number | 必填 | 分页大小 |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| --- | --- | :---: | :---: | :---: |
| total | | number | 必填 | 总数 |
| list | | array | 必填 |  |
|  | id | string | 必填 | 唯一id |
|  | fileName | string | 必填 | 文件名 |
|  | fileSize | number | 必填 | 文件大小（字节） |
|  | logTimeRange | string | 必填 | 日志时间范围 |
|  | downloadURL | string | 必填 | 下载地址 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "list": [
            {
                "id": 12,
                "fileName": "202506201516.gz",
                "fileSize": 317,
                "logTimeRange": "2025-06-20 15:00~16:00",
                "downloadURL": "https://..."
            },
            {
                "id": 11,
                "fileName": "202506201516.gz",
                "fileSize": 195,
                "logTimeRange": "2025-06-20 15:00~16:00",
                "downloadURL": "https://..."
            },
            {
                "id": 10,
                "fileName": "202506201314.log.gz",
                "fileSize": 208,
                "logTimeRange": "2025-06-20 13:00~14:00",
                "downloadURL": "https://..."
            },
            {
                "id": 8,
                "fileName": "202506201213.gz",
                "fileSize": 195,
                "logTimeRange": "2025-06-20 12:00~13:00",
                "downloadURL": "https://..."
            },
            {
                "id": 7,
                "fileName": "202506201213.gz",
                "fileSize": 195,
                "logTimeRange": "2025-06-20 12:00~13:00",
                "downloadURL": "https://..."
            }
        ],
        "total": 0
    },
    "x-traceID": ""
}
```

**注意事项**
- 此接口需要开通开发者权益，并且仅限查询近30天的日志数据

### 获取直链流量日志

- 用途：获取直链流量日志。
- HTTP：GET 域名 + /api/v1/direct-link/log
- 官方来源：[获取直链流量日志](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/agmqpmu0dm0iogc9)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| pageNum | number | 必填 | 页数 |
| pageSize | number | 必填 | 分页大小 |
| startTime | string | 必填 | 开始时间，格式：2025-01-01 00:00:00 |
| endTime | string | 必填 | 结束时间，格式：2025-01-01 23:59:59 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| total | number | 必填 | 总数 |
| list | array | 必填 |  |
| uniqueID | string | 必填 | 唯一id |
| fileName | string | 必填 | 文件名 |
| fileSize | number | 必填 | 文件大小（字节） |
| filePath | string | 必填 | 文件路径 |
| directLinkURL | string | 必填 | 直链URL |
| fileSource | number | 必填 | 文件来源 1全部文件 2图床 |
| totalTraffic | number | 必填 | 消耗流量（字节） |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "list": [
      {
        "uniqueID": "...",
        "fileName": "...",
        "fileSize": 2595421554,
        "filePath": "/测试图片/Planet.Earth.III.S01E06.Extremes.2160p.iP.WEB-DL.AAC2.0.HLG.H.265-Q66.mkv",
        "directLinkURL": "https://...",
        "fileSource": 1,
        "totalTraffic": 2859077463
      },
      {
        "uniqueID": "...",
        "fileName": "video.mp4",
        "fileSize": 729699,
        "filePath": "/测试图片/video.mp4",
        "directLinkURL": "https://...",
        "fileSource": 1,
        "totalTraffic": 2971802
      }
    ],
    "total": 2
  },
  "x-traceID": ""
}
```

**注意事项**
- 此接口需要开通开发者权益，并且仅限查询近三天的日志数据

### 启用直链空间

- 用途：启用直链空间。
- HTTP：POST 域名 + /api/v1/direct-link/enable
- 官方来源：[启用直链空间](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/cl3gvdmho288d376)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileID | number | 必填 | 启用直链空间的文件夹的fileID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| filename | string | 必填 | 成功启用直链空间的文件夹的名称 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "filename": "测试直链目录"
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取直链链接

- 用途：获取直链链接。
- HTTP：GET 域名 + /api/v1/direct-link/url
- 官方来源：[获取直链链接](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/tdxfsmtemp4gu4o2)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileID | number | 必填 | 需要获取直链链接的文件的fileID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| url | string | 必填 | 文件对应的直链链接 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "url": "https://..."
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 禁用直链空间

- 用途：禁用直链空间。
- HTTP：POST 域名 + /api/v1/direct-link/disable
- 官方来源：[禁用直链空间](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ccgz6fwf25nd9psl)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileID | number | 必填 | 禁用直链空间的文件夹的fileID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| filename | string | 必填 | 成功禁用直链空间的文件夹的名称 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "filename": "测试禁用直链目录"
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 直链缓存刷新

- 用途：直链缓存刷新。
- HTTP：POST 域名 + /api/v1/direct-link/cache/refresh
- 官方来源：[直链缓存刷新](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ptgqvx45rtaxry5v)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| --- | --- | --- | --- |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| --- | --- | --- | --- |
| code | number | 必填 | 响应码 |
| message | string | 必填 | 响应信息 |
| data | object | 必填 | 响应数据 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {},
    "x-traceID": ""
}
```

**注意事项**
- 此接口无需请求参数
