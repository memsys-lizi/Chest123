# 图床

官方来源：[123云盘开放平台](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced)

## 本模块接口

- 说明页 无 - 上传流程说明
- POST /upload/v1/oss/file/mkdir - 创建目录
- POST /upload/v1/oss/file/create - 创建文件
- POST /upload/v1/oss/file/get_upload_url - 获取上传地址&上传分片
- POST /upload/v1/oss/file/upload_complete - 上传完毕
- POST /upload/v1/oss/file/upload_async_result - 异步轮询获取上传结果
- POST /api/v1/oss/source/copy - 创建复制任务
- GET /api/v1/oss/source/copy/process - 获取复制任务详情
- GET /api/v1/oss/source/copy/fail - 获取复制失败文件列表
- POST /api/v1/oss/file/move - 移动图片
- POST /api/v1/oss/file/delete - 删除图片
- GET /api/v1/oss/file/detail - 获取图片详情
- POST /api/v1/oss/file/list - 获取图片列表
- POST /api/v1/oss/offline/download - 创建离线迁移任务
- GET /api/v1/oss/offline/download/process - 获取离线迁移任务

## 详细说明

### 上传流程说明

- 用途：上传流程说明。
- HTTP：无独立 HTTP 接口
- 官方来源：[上传流程说明](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/di0url3qn13tk28t)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 非秒传情况将会返回预上传ID`preuploadID`与分片大小`sliceSize`，请将文件根据分片大小切分。
- 非秒传时，携带返回的`preuploadID`，自定义分片序号`sliceNo`(从数字1开始)。
- 获取上传地址`presignedURL`。
- 向返回的地址`presignedURL`发送PUT请求，上传文件分片。
- PUT请求的header中请不要携带Authorization、Platform参数。
- 所有分片上传后，调用列举已上传分片接口，将本地与云端的分片MD5比对。
- 如果您的文件小于`sliceSize`** ，**该操作将会返回空值**，**可以跳过此步。
- 若接口返回的`async`为true时，则需下一步，调用异步轮询获取上传结果接口，获取上传最终结果。
- 该步骤需要等待，建议轮询获取结果。123云盘服务器会校验用户预上传时的MD5与实际上传成功的MD5是否一致。

### 创建目录

- 用途：创建目录。
- HTTP：POST 域名 + /upload/v1/oss/file/mkdir
- 官方来源：[创建目录](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/tpqqm04ocqwvonrk)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| name | []string | 必填 | 目录名(注:不能重名) |
| parentID | string | 必填 | 父目录id，上传到根目录时为空 |
| type | number | 必填 | 固定为 1 |

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "list": [
      {
        "filename": "测试图床目录",
        "dirID": "..."
      }
    ]
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 创建文件

- 用途：创建文件。
- HTTP：POST 域名 + /upload/v1/oss/file/create
- 官方来源：[创建文件](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/xwfka5kt6vtmgs8r)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| parentFileID | string | 必填 | 父目录id，上传到根目录时填写 空 |
| filename | string | 必填 | 文件名要小于255个字符且不能包含以下任何字符："\/:*?\|><。（注：不能重名） |
| etag | string | 必填 | 文件md5 |
| size | number | 必填 | 文件大小，单位为 byte 字节 |
| type | number | 必填 | 固定为 1 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| fileID | string | 非必填 | 文件ID。当123云盘已有该文件,则会发生秒传。此时会将文件ID字段返回。唯一 |
| preuploadID | string | 必填 | 预上传ID(如果 reuse 为 true 时,该字段不存在) |
| reuse | boolean | 必填 | 是否秒传，返回true时表示文件已上传成功 |
| sliceSize | number | 必填 | 分片大小，必须按此大小生成文件分片再上传 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "fileID": "",
    "reuse": false,
    "preuploadID": "h1Kiaaaaaaac/0IDD87IFbIf8T0UWrTNwNNGbGoeklBYFtnlDwBIhd9OfdMjm4abJfDPccrScqQIPdjFasHxGxV//V7bzfUbEEaEt8N6RT2PI/dC/gv...(过长省略)",
    "sliceSize": 104857600
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取上传地址&上传分片

- 用途：获取上传地址&上传分片。
- HTTP：POST 域名 + /upload/v1/oss/file/get_upload_url
- 官方来源：[获取上传地址&上传分片](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/pyfo3a39q6ac0ocd)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| preuploadID | string | 必填 | 预上传ID |
| sliceNo | number | 必填 | 分片序号，从1开始自增 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| presignedURL | string | 必填 | 上传地址 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "presignedURL": "https://...",
    "isMultipart": false
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 上传完毕

- 用途：上传完毕。
- HTTP：POST 域名 + /upload/v1/oss/file/upload_complete
- 官方来源：[上传完毕](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/yhgo0kt3nkngi8r2)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| preuploadID | string | 必填 | 预上传ID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| fileID | string | 非必填 | 当下方 completed 字段为true时,此处的 fileID 就为文件的真实ID(唯一) |
| async | bool | 必填 | 是否需要异步查询上传结果。false为无需异步查询,已经上传完毕。true 为需要异步查询上传结果。 |
| completed | bool | 必填 | 上传是否完成 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "async": true,
    "completed": false,
    "fileID": ""
  },
  "x-traceID": "..."
}
```

**注意事项**
- 文件上传完成后请求
- 调用该接口前,请优先列举已上传的分片,在本地进行 md5 比对

### 异步轮询获取上传结果

- 用途：异步轮询获取上传结果。
- HTTP：POST 域名 + /upload/v1/oss/file/upload_async_result
- 官方来源：[异步轮询获取上传结果](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/lbdq2cbyzfzayipu)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| preuploadID | string | 必填 | 预上传ID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| completed | bool | 必填 | 上传合并是否完成,如果为false,请至少1秒后发起轮询 |
| fileID | string | 必填 | 上传成功的文件ID |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "completed": true,
    "fileID": "..."
  },
  "x-traceID": "..."
}
```

**注意事项**
- 异步轮询获取上传结果

### 创建复制任务

- 用途：创建复制任务。
- HTTP：POST 域名 + /api/v1/oss/source/copy
- 官方来源：[创建复制任务](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/trahy3lmds4o0i3r)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileIDs | array | 必填 | 文件id数组(string 数组) |
| toParentFileID | string | 必填 | 要移动到的图床目标文件夹id，移动到根目录时为空 |
| sourceType | string | 必填 | 复制来源(1=云盘) |
| type | number | 必填 | 业务类型，固定为 1 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| taskID | string | 必填 | 复制任务ID,可通过该ID,调用查询复制任务状态 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "taskID": "..."
    },
    "x-traceID": "..."
}
```

**注意事项**
- 图床复制任务创建（可创建的任务数：3，fileIDs 长度限制：100，当前一个任务处理完后将会继续处理下个任务）
- 如果图床目录下存在相同 etag、size 的图片将会视为同一张图片，将覆盖原图片

### 获取复制任务详情

- 用途：获取复制任务详情。
- HTTP：GET 域名 + /api/v1/oss/source/copy/process
- 官方来源：[获取复制任务详情](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/rissl4ewklaui4th)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| taskID | string | 必填 | 复制任务ID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| status | int | 必填 | 任务状态:  1进行中,2结束,3失败,4等待 |
| failMsg | string | 必填 | 失败原因 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "status": 2,
        "failMsg": ""
    },
    "x-traceID": "..."
}
```

**注意事项**
- 该接口将会获取图床复制任务执行情况

### 获取复制失败文件列表

- 用途：获取复制失败文件列表。
- HTTP：GET 域名 + /api/v1/oss/source/copy/fail
- 官方来源：[获取复制失败文件列表](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/tlug9od3xlw2w23v)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| taskID | string | 必填 | 复制任务ID |
| limit | number | 必填 | 每页文件数量，最大不超过100 |
| page | number | 必填 | 页码数 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| total | number | 必填 | 总数 |
| list | array | 必填 | 失败文件列表 |
|  | fileId | number | 文件Id |
|  | filename | string | 文件名 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "total": 0,
        "list": null
    },
    "x-traceID": "..."
}
```

**注意事项**
- 查询图床复制任务失败文件列表（注：记录的是符合对应格式、大小的图片的复制失败原因）

### 移动图片

- 用途：移动图片。
- HTTP：POST 域名 + /api/v1/oss/file/move
- 官方来源：[移动图片](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/eqeargimuvycddna)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileIDs | array | 必填 | 文件id数组 |
| toParentFileID | string | 必填 | 要移动到的目标文件夹id，不能为空 |

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": null,
    "x-traceID": "..."
}
```

**注意事项**
- 批量移动文件，单级最多支持100个

### 删除图片

- 用途：删除图片。
- HTTP：POST 域名 + /api/v1/oss/file/delete
- 官方来源：[删除图片](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ef8yluqdzm2yttdn)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileIDs | array | 必填 | 文件id数组,参数长度最大不超过 100 |

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": null,
    "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取图片详情

- 用途：获取图片详情。
- HTTP：GET 域名 + /api/v1/oss/file/detail
- 官方来源：[获取图片详情](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/rgf2ndfaxc2gugp8)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileID | string | 必填 | 文件ID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | string | 必填 | 文件ID |
| filename | string | 必填 | 文件名 |
| type | number | 必填 | 0-文件  1-文件夹 |
| size | number | 必填 | 文件大小 |
| etag | string | 必填 | md5 |
| status | number | 必填 | 文件审核状态。 大于 100 为审核驳回文件 |
| createAt | string | 必填 | 创建时间 |
| updateAt | string | 必填 | 更新时间 |
| downloadURL | string | 必填 | 下载链接 |
| userSelfURL | string | 必填 | 自定义域名链接 |
| totalTraffic | number | 必填 | 流量统计 |
| parentFileId | string | 必填 | 父级ID |
| parentFilename | string | 必填 | 父级文件名称 |
| extension | string | 必填 | 后缀名称 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "fileId": "...",
        "filename": "测试图床.jpg",
        "parentFileId": "...",
        "type": 0,
        "etag": "...",
        "size": 22027358,
        "status": 2,
        "s3KeyFlag": "1817178140-0",
        "storageNode": "m76",
        "createAt": "2025-03-03 16:38:26",
        "updateAt": "2025-03-03 16:38:26",
        "downloadURL": "https://...",
        "ossIndex": 43,
        "totalTraffic": 0,
        "parentFilename": "测试图床目录",
        "extension": "jpg",
        "userSelfURL": "https://..."
    },
    "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取图片列表

- 用途：获取图片列表。
- HTTP：POST 域名 + /api/v1/oss/file/list
- 官方来源：[获取图片列表](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/zayr72q8xd7gg4f8)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| parentFileId | string | 选填 | 父级目录Id, 默认为空表示筛选根目录下的文件 |
| limit | number | 必填 | 每页文件数量，最大不超过100 |
| startTime | number | 选填 | 筛选开始时间（时间戳格式，例如 1730390400） |
| endTime | number | 选填 | 筛选结束时间（时间戳格式，例如 1730390400） |
| lastFileId | string | 选填 | 翻页查询时需要填写 |
| type | number | 必填 | 固定为1 |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| :---: | --- | :---: | :---: | --- |
|  lastFileId | | string | 必填 | -1代表最后一页（无需再翻页查询）   其他代表下一页开始的文件id，携带到请求参数中 |
| fileList | | array | 必填 | 文件列表 |
|  | fileId | string | 必填 | 文件ID |
|  | filename | string | 必填 | 文件名 |
|  | type | number | 必填 | 0-文件 1-文件 |
|  | size | number | 必填 | 文件大小 |
|  | etag | string | 必填 | md5 |
|  | status | number | 必填 | 文件审核状态。 大于 100 为审核驳回文件 |
|  | createAt | string | 必填 | 创建时间 |
|  | updateAt | string | 必填 | 更新时间 |
|  | downloadURL | string | 必填 | 下载链接 |
|  | userSelfURL | string | 必填 | 自定义域名链接 |
|  | totalTraffic | number | 必填 | 流量统计 |
|  | parentFileId | string | 必填 | 父级ID |
|  | parentFilename | string | 必填 | 父级文件名称 |
|  | extension | string | 必填 | 后缀名称 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "lastFileId": "-1",
        "fileList": [
            {
                "fileId": "...",
                "filename": "测试图床目录1",
                "parentFileId": "...",
                "type": 1,
                "etag": "",
                "size": 0,
                "status": 0,
                "s3KeyFlag": "",
                "storageNode": "",
                "createAt": "2025-03-03 15:43:46",
                "updateAt": "2025-03-03 15:43:46",
                "downloadURL": "",
                "ossIndex": 42,
                "totalTraffic": 0,
                "parentFilename": "img_oss",
                "extension": "",
                "userSelfURL": ""
            },
            {
                "fileId": "...",
                "filename": "测试图床目录",
                "parentFileId": "...",
                "type": 1,
                "etag": "",
                "size": 0,
                "status": 0,
                "s3KeyFlag": "",
                "storageNode": "",
                "createAt": "2025-03-03 15:07:54",
                "updateAt": "2025-03-03 15:07:54",
                "downloadURL": "",
                "ossIndex": 42,
                "totalTraffic": 0,
                "parentFilename": "img_oss",
                "extension": "",
                "userSelfURL": ""
            }
        ]
    },
    "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 创建离线迁移任务

- 用途：创建离线迁移任务。
- HTTP：POST 域名 + /api/v1/oss/offline/download
- 官方来源：[创建离线迁移任务](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ctigc3a08lqzsfnq)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| url | string | 必填 | 下载资源地址(http/https) |
| fileName | string | 非必填 |  自定义文件名称 （需携带图片格式，支持格式：png, gif, jpeg, tiff, webp,jpg,tif,svg,bmp）   |
| businessDirID | string | 非必填 | 选择下载到指定目录ID。 示例:10023<br>注:不支持下载到根目录,默认下载到名为"来自:离线下载"的目录中 |
| callBackUrl | string | 非必填 | 回调地址,当文件下载成功或者失败,均会通过回调地址通知。回调内容如下      url: 下载资源地址<br>status: 0 成功，1 失败<br>fileReason：失败原因<br>fileID:成功后,该文件在云盘上的ID      请求类型：POST   {<br>	"url": "[http://dc.com/resource.jpg",](http://dc.com/resource.jpg",)<br>	"status": 0, <br>	"failReason": "",<br>        "fileID":100<br>} |
| type | number | 必填 | 业务类型，固定为 1 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| taskID | number | 必填 | 离线下载任务ID,可通过该ID,调用查询任务进度接口获取下载进度 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "taskID": 403316
    },
    "x-traceID": "..."
}
```

**注意事项**
- 如果图床目录下存在相同 etag、size 的图片将会覆盖原图片

### 获取离线迁移任务

- 用途：获取离线迁移任务。
- HTTP：GET 域名 + /api/v1/oss/offline/download/process
- 官方来源：[获取离线迁移任务](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/svo92desugbyhrgq)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| taskID | number | 必填 | 离线下载任务ID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| status | int | 必填 | 下载状态:  0进行中、1下载失败、2下载成功、3重试中 |
| process | int | 必填 | 离线进度百分比0-100 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "process": 100,
    "status": 2
  },
  "x-traceID": "..."
}
```

**注意事项**
- 获取当前离线下载任务状态
