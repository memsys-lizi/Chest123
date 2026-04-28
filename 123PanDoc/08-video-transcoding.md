# 视频转码

官方来源：[123云盘开放平台](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced)

## 本模块接口

- 说明页 无 - 上传流程
- GET /api/v2/file/list - 获取云盘视频文件
- POST /api/v1/transcode/upload/from_cloud_disk - 从云盘空间上传
- GET /api/v2/file/list - 获取转码空间文件列表
- POST /api/v1/transcode/folder/info - 获取转码空间文件夹信息
- POST /api/v1/transcode/video/resolutions - 获取视频文件可转码的分辨率
- GET /api/v1/video/transcode/list - 视频转码列表（三方挂载应用授权使用）
- POST /api/v1/transcode/video - 视频转码操作
- POST /api/v1/transcode/video/record - 查询某个视频的转码记录
- POST /api/v1/transcode/video/result - 查询某个视频的转码结果
- POST /api/v1/transcode/delete - 删除转码视频
- POST /api/v1/transcode/file/download - 原文件下载
- POST /api/v1/transcode/m3u8_ts/download - 单个转码文件下载（m3u8或ts）
- POST /api/v1/transcode/file/download/all - 某个视频全部转码文件下载

## 详细说明

### 上传流程

- 用途：上传流程。
- HTTP：无独立 HTTP 接口
- 官方来源：[上传流程](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/kh4ovskpumzn8r07)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| parentFileID | number | 必填 | 父目录id，必须填写转码空间的文件夹ID |

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取云盘视频文件

- 用途：获取云盘视频文件。
- HTTP：GET 域名 + /api/v2/file/list
- 官方来源：[获取云盘视频文件](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/yqyi3rqrmrpvdf0d)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| --- | --- | --- | --- |
| parentFileId | number | 必填 | 文件夹ID，根目录传 0 |
| limit | number | 必填 | 每页文件数量，最大不超过100 |
| searchData | string | 选填 | 搜索关键字将无视文件夹ID参数。将会进行全局查找 |
| searchMode | number | 选填 | 0:全文模糊搜索(注:将会根据搜索项分词,查找出相似的匹配项)<br>1:精准搜索(注:精准搜索需要提供完整的文件名) |
|  lastFileId | number | 选填 | 翻页查询时需要填写 |
| category | number | 必填 | 固定为2，2代表视频 |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| --- | --- | :---: | :---: | --- |
|  lastFileId<br> | | number | 必填 | -1代表最后一页（无需再翻页查询）<br>其他代表下一页开始的文件id，携带到请求参数中 |
| fileList | | array | 必填 | 文件列表 |
|  | fileId | number | 必填 | 文件Id |
|  | filename | string | 必填 | 文件名 |
|  | type | number | 必填 | 0-文件  1-文件夹 |
|  | size | number | 必填 | 文件大小 |
|  | etag | string | 必填 | md5 |
|  | status | number | 必填 | 文件审核状态。 大于 100 为审核驳回文件 |
|  | parentFileId | number | 必填 | 目录ID |
|  | category | number | 必填 | 文件分类：0-未知 1-音频 2-视频 3-图片 |

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 从云盘空间上传

- 用途：从云盘空间上传。
- HTTP：POST 域名 + /api/v1/transcode/upload/from_cloud_disk
- 官方来源：[从云盘空间上传](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/tqy2xatoo4qmdbz7)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | []objet | 必填 | 云盘空间文件ID<br>注意：一次性最多支持100个文件上传 |

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
```json
[
    {
        "fileId": 2875051
    },
    {
        "fileId": 2875052
    }
]
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取转码空间文件列表

- 用途：获取转码空间文件列表。
- HTTP：GET 域名 + /api/v2/file/list
- 官方来源：[获取转码空间文件列表](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ux9wct58lvllxm1n)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| parentFileId | number | 必填 | 文件夹ID，根目录传 0 |
| limit | number | 必填 | 每页文件数量，最大不超过100 |
| searchData | string | 选填 | 搜索关键字将无视文件夹ID参数。将会进行全局查找 |
| searchMode | number | 选填 | 0:全文模糊搜索(注:将会根据搜索项分词,查找出相似的匹配项)<br>1:精准搜索(注:精准搜索需要提供完整的文件名) |
| lastFileId | number | 选填 | 翻页查询时需要填写 |
| businessType | number | 必填 | 固定为2，2代表转码空间 |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| --- | --- | :---: | :---: | --- |
|  lastFileId<br> | | number | 必填 | -1代表最后一页（无需再翻页查询）<br>其他代表下一页开始的文件id，携带到请求参数中 |
| fileList | | array | 必填 | 文件列表 |
|  | fileId | number | 必填 | 文件Id |
|  | filename | string | 必填 | 文件名 |
|  | type | number | 必填 | 0-文件  1-文件夹 |
|  | size | number | 必填 | 文件大小 |
|  | etag | string | 必填 | md5 |
|  | status | number | 必填 | 文件审核状态。 大于 100 为审核驳回文件 |
|  | parentFileId | number | 必填 | 目录ID |
|  | category | number | 必填 | 文件分类：0-未知 1-音频 2-视频 3-图片 |
|  | trashed | number | 必填 | 是否在回收站：0-否 1-是 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "lastFileId": -1,
        "fileList": [
            {
                "fileId": 13210718,
                "filename": "transcode",
                "parentFileId": 0,
                "type": 1,
                "etag": "",
                "size": 0,
                "category": 0,
                "status": 0,
                "punishFlag": 0,
                "s3KeyFlag": "1815309870-0",
                "storageNode": "m0",
                "trashed": 0,
                "createAt": "2025-01-08 14:07:37",
                "updateAt": "2025-01-08 14:07:37"
            }
        ]
    },
    "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取转码空间文件夹信息

- 用途：获取转码空间文件夹信息。
- HTTP：POST 域名 + /api/v1/transcode/folder/info
- 官方来源：[获取转码空间文件夹信息](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/kaalgke88r9y7nlt)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileID | number | 必填 | 转码空间文件夹ID |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "fileID": 13210718
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取视频文件可转码的分辨率

- 用途：获取视频文件可转码的分辨率。
- HTTP：POST 域名 + /api/v1/transcode/video/resolutions
- 官方来源：[获取视频文件可转码的分辨率](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/apzlsgyoggmqwl36)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | number | 必填 | 文件Id |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| :---: | --- | :---: | :---: | --- |
|  IsGetResolution | | boolean | 必填 | true 代表正在获取<br>false 代表已经获取结束 |
| Resolutions | | string | 必填 | 可转码的分辨率 |
| NowOrFinishedResolutions | | string | 可填 | 已经转码的分辨率，如果为空则代表该视频从未转码过，后续在转码时候，如果已经有正在或者已经转码的分辨率之后，就无需在视频转码中传递传递已有的分辨率，避免重复转码 |
| CodecNames | | string | 必填 | 编码方式 |
| VideoTime | | number | 必填 | 视频时长，单位：秒 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "IsGetResolution": false,
        "Resolutions": "480p,720p,1080p",
        "NowOrFinishedResolutions": "1080p,720p",
        "CodecNames": "H.264",
        "VideoTime": 51
    },
    "x-traceID": ""
}
```

**注意事项**
- 注意：该接口需要轮询去查询结果，建议10s一次

### 视频转码列表（三方挂载应用授权使用）

- 用途：视频转码列表（三方挂载应用授权使用）。
- HTTP：GET 域名 + /api/v1/video/transcode/list
- 官方来源：[视频转码列表（三方挂载应用授权使用）](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/tgg6g84gdrmyess5)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | number | 是 | 文件id |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| --- | :---: | :---: | --- |
| status | number | 是 | 转码状态[1：待转码；3：转码失败；254：部分成功；255：全部成功] |
| list | array | 是 | 视频转码列表 |
| list[*].url | string | 是 | 地址 |
| list[*].resolution | string | 是 | 分辨率 |
| list[*].duration | float | 是 | 转码后的时长（秒） |
| list[*].height | number | 是 | 高度 |
| list[*].status | number | 是 | 转码状态[255：全部成功] |
| list[*].mc | string | 是 | 存储集群 |
| list[*].bitRate | number | 是 | 码率 |
| list[*].progress | number | 是 | 转码进度 |
| list[*].updateAt | string | 是 | 更新时间 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "list": [
      {
        "url": "https://...",
        "resolution": "2160p",
        "duration": 19.83300018310547,
        "height": 2160,
        "status": 255,
        "mc": "m88_123-hls-888",
        "bitRate": 4000000,
        "progress": 100,
        "updateAt": "2024-07-11 14:56:19"
      }
    ],
    "status": 255
  },
  "x-traceID": ""
}
```

**注意事项**
- 此接口仅限授权`access_token`调用

### 视频转码操作

- 用途：视频转码操作。
- HTTP：POST 域名 + /api/v1/transcode/video
- 官方来源：[视频转码操作](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/xy42nv2x8wav9n5l)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | number | 必填 | 文件Id |
| codecName | string | 必填 | 编码方式 |
| videoTime | number | 必填 | 视频时长，单位：秒 |
| resolutions | string | 必填 | 要转码的分辨率，多个之间以逗号分割，如："2160P,1080P,720P",注意：p是大写，如果之前已经转码过别的分辨率，那就无需再传 |

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": "2160P&1080P&720P已成功开始转码，请在转码结果中查询",
    "x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 查询某个视频的转码记录

- 用途：查询某个视频的转码记录。
- HTTP：POST 域名 + /api/v1/transcode/video/record
- 官方来源：[查询某个视频的转码记录](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ost1m82sa9chh0mc)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | number | 必填 | 文件Id |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| UserTranscodeVideoRecordList | array | 必填 | 用户转码记录列表 |
| create_at | string | 必填 | 创建时间 |
| resolution | string | 必填 | 分辨率 |
| status | number | 必填 | 1：准备转码   2：正在转码中    3-254：转码失败，时长会自动回退 255：转码成功 |
| link | string | 选填 | 视频转码成功之后的m3u8的链接 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "UserTranscodeVideoRecordList": [
            {
                "create_at": "2024-12-19 13:19:33",
                "resolution": "720P",
                "status": 255,
                "link": "https://..."
            },
            {
                "create_at": "2024-12-19 13:19:33",
                "resolution": "1080P",
                "status": 255,
                "link": "https://..."
            }
        ]
    },
    "x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 查询某个视频的转码结果

- 用途：查询某个视频的转码结果。
- HTTP：POST 域名 + /api/v1/transcode/video/result
- 官方来源：[查询某个视频的转码结果](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/iucbqgge0dgfc8sv)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | number | 必填 | 文件Id |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| --- | --- | :---: | :---: | --- |
| UserTranscodeVideoList | | array | 必填 | 用户转码结果列表 |
| Uid | | number | 必填 | 用户id |
| Resolution | | string | 必填 | 分辨率 |
| Status | | number | 必填 | 1：准备转码   2：正在转码中    3-254：转码失败，时长会自动回退255：转码成功 |
| Files | | Array |  |  |
|  | FileName | string | 必填 | 转码文件名称 |
|  | FileSize | string | 必填 | 转码文件大小 |
|  | Resolution | string | 必填 | 转码文件分辨率 |
|  | CreateAt | string | 必填 | 转码文件创建时间 |
|  | Url | string | 必填 | 转码文件播放地址，只有m3u8文件有播放地址，ts文件没有 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "UserTranscodeVideoList": [
            {
                "Id": 497,
                "Uid": 1814435971,
                "Resolution": "720P",
                "Status": 255,
                "CreateAt": "2024-12-19 11:45:04",
                "UpdateAt": "2024-12-19 11:45:23",
                "Files": [
                    {
                        "FileName": "stream.m3u8",
                        "FileSize": "177B",
                        "Resolution": "720P",
                        "CreateAt": "2024-12-19 11:45:09",
                        "Url": "https://..."
                    },
                    {
                        "FileName": "000.ts",
                        "FileSize": "497.17KB",
                        "Resolution": "720P",
                        "CreateAt": "2024-12-19 11:45:09",
                        "Url": ""
                    }
                ]
            },
            {
                "Id": 498,
                "Uid": 1814435971,
                "Resolution": "1080P",
                "Status": 255,
                "CreateAt": "2024-12-19 11:45:04",
                "UpdateAt": "2024-12-19 11:45:23",
                "Files": [
                    {
                        "FileName": "stream.m3u8",
                        "FileSize": "177B",
                        "Resolution": "1080P",
                        "CreateAt": "2024-12-19 11:45:09",
                        "Url": "https://..."
                    },
                    {
                        "FileName": "000.ts",
                        "FileSize": "452.93KB",
                        "Resolution": "1080P",
                        "CreateAt": "2024-12-19 11:45:09",
                        "Url": ""
                    }
                ]
            }
        ]
    },
    "x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 删除转码视频

- 用途：删除转码视频。
- HTTP：POST 域名 + /api/v1/transcode/delete
- 官方来源：[删除转码视频](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/tg2xgotkgmgpulrp)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| fileId | number | 必填 | 文件Id |
| businessType | number | 必填 | businessType只能是2 |
| trashed | number | 必填 | 1：删除原文件<br>2：删除原文件+转码后的文件 |

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": "删除文件成功",
    "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 原文件下载

- 用途：原文件下载。
- HTTP：POST 域名 + /api/v1/transcode/file/download
- 官方来源：[原文件下载](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/mlltlx57sty6g9gf)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | number | 必填 | 文件Id |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| :---: | --- | :---: | :---: | --- |
|  downloadUrl | | string | 必填 | 下载地址，如果转码空间满了，则返回空 |
| isFull | | boolean | 必填 | 转码空间容量是否满了，如果满了，则不会返回下载地址，需要用户购买转码空间之后才能下载 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "downloadUrl": "https://...",
        "isFull": false
    },
    "x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 单个转码文件下载（m3u8或ts）

- 用途：单个转码文件下载（m3u8或ts）。
- HTTP：POST 域名 + /api/v1/transcode/m3u8_ts/download
- 官方来源：[单个转码文件下载（m3u8或ts）](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/yf97p60yyzb8mzbr)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| fileId | number | 必填 | 文件Id |
| resolution | string | 必填 | 分辨率 |
| type | number | 必填 | 1代表下载m3u8文件<br>2代表下载ts文件 |
| tsName | string | 选填 | 下载ts文件时必须要指定ts文件的名称，ts的名称参考查询某个视频的转码结果 |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| :---: | --- | :---: | :---: | --- |
|  downloadUrl | | string | 必填 | 下载地址，如果转码空间满了，则返回空 |
| isFull | | boolean | 必填 | 转码空间容量是否满了，如果满了，则不会返回下载地址，需要用户购买转码空间之后才能下载 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "downloadUrl": "https://...",
        "isFull": false
    },
    "x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 某个视频全部转码文件下载

- 用途：某个视频全部转码文件下载。
- HTTP：POST 域名 + /api/v1/transcode/file/download/all
- 官方来源：[某个视频全部转码文件下载](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/yb7hrb0x2gym7xic)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | number | 必填 | 文件Id |
| zipName | string | 必填 | 下载zip文件的名字 |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| :---: | --- | :---: | :---: | --- |
|  isDownloading | | boolean | 必填 | true 代表正在下载中<br>false 下载完毕了 |
| isFull | | boolean | 必填 | true 代表转码空间满了<br>false 代表转码空间未满 |
| downloadUrl | | string | 必填 | 下载地址，注意：只有在转码空间未满，并且已经下载完毕才有值 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "isDownloading": false,
        "isFull": false,
        "downloadUrl": "https://... eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3MzU3ODMyODEsImlhdCI6MTczNTc4Mjk4MSwiaWQiOjE4MTQ0MzU5NzEsIm1haWwiOiIiLCJuaWNrbmFtZSI6IjE1MzIzNzAxMTM0IiwidXNlcm5hbWUiOjE1MzIzNzAxMTM0LCJ2Ijo5OH0.IqgK-7vN-QbpjwE1W4NRcb8QVm3FJvTqwO53fsggE14"
    },
    "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。
