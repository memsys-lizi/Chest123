# 文件管理

官方来源：[123云盘开放平台](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced)

## 本模块接口

- POST /upload/v1/file/mkdir - 创建目录
- PUT /api/v1/file/name - 单个文件重命名
- POST /api/v1/file/rename - 批量文件重命名
- POST /api/v1/file/trash - 删除文件至回收站
- 说明页 无 - 复制
- POST /api/v1/file/copy - 复制单个文件
- POST /api/v1/file/async/copy - 批量复制文件
- GET /api/v1/file/async/copy/process - 批量复制文件进度
- POST /api/v1/file/recover - 从回收站恢复文件
- POST /api/v1/file/recover/by_path - 还原文件到指定目录
- GET /api/v1/file/detail - 获取单个文件详情
- POST /api/v1/file/infos - 获取多个文件详情
- GET /api/v2/file/list - 获取文件列表（推荐）
- GET /api/v1/file/list - 获取文件列表（旧）
- POST /api/v1/file/move - 移动
- GET /api/v1/file/download_info - 下载

## 详细说明

### 创建目录

- 用途：创建目录。
- HTTP：POST 域名 + /upload/v1/file/mkdir
- 官方来源：[创建目录](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ouyvcxqg3185zzk4)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| name | string | 必填 | 目录名(注:不能重名) |
| parentID | number | 必填 | 父目录id，上传到根目录时填写 0 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| dirID | number | 必填 | 创建的目录ID |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "dirID": 14663228
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 单个文件重命名

- 用途：单个文件重命名。
- HTTP：PUT 域名 + /api/v1/file/name
- 官方来源：[单个文件重命名](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/ha6mfe9tteht5skc)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | number | 是 | 文件id |
| fileName | string | 是 | 文件名 |

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

### 批量文件重命名

- 用途：批量文件重命名。
- HTTP：POST 域名 + /api/v1/file/rename
- 官方来源：[批量文件重命名](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/imhguepnr727aquk)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| renameList | array | 必填 | 数组,每个成员的格式为 文件ID|新的文件名 |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| --- | --- | :---: | :---: | --- |
| successList | | array | 必填 | 成功文件列表 |
|  | fileID | number | 必填 | 文件Id |
|  | updateAt | string | 必填 | 更新时间 |
| failList | | array | 必填 | 成功文件列表 |
|  | fileID | number | 必填 | 文件Id |
|  | message | string | 必填 | 错误原因 |

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
- 批量重命名文件，最多支持同时30个文件重命名

### 删除文件至回收站

- 用途：删除文件至回收站。
- HTTP：POST 域名 + /api/v1/file/trash
- 官方来源：[删除文件至回收站](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/en07662k2kki4bo6)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileIDs | array | 必填 | 文件id数组,一次性最大不能超过 100 个文件 |

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
- 删除的文件，会放入回收站中

### 复制

- 用途：复制。
- HTTP：无独立 HTTP 接口
- 官方来源：[复制](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/bh1q00065hg305al)

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

### 复制单个文件

- 用途：复制单个文件。
- HTTP：POST 域名 + /api/v1/file/copy
- 官方来源：[复制单个文件](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/thpz0w9er500pob9)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileId | number | 必填 | 文件id |
| targetDirId | number | 必填 | 要复制到的目标文件夹id |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| sourceFileId | number | 必填 | 源文件id |
| targetFileId | number | 必填 | 新复制文件id |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "sourceFileId": 1758176090915,
        "targetFileId": 1758176091289
    },
    "x-traceID": "..."
}
```

**注意事项**
- 此接口仅支持单个文件复制

### 批量复制文件

- 用途：批量复制文件。
- HTTP：POST 域名 + /api/v1/file/async/copy
- 官方来源：[批量复制文件](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/pik0i4lvxw4lkkc7)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileIds | array | 必填 | 文件id数组 |
| targetDirId | number | 必填 | 要复制到的目标文件夹id |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| taskId | number | 必填 | 任务id，后续用来查询任务进度 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "taskId": 2020
    },
    "x-traceID": "..."
}
```

**注意事项**
- 批量复制文件，单级最多支持3000个

### 批量复制文件进度

- 用途：批量复制文件进度。
- HTTP：GET 域名 + /api/v1/file/async/copy/process
- 官方来源：[批量复制文件进度](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/fqh9vk1esg4uomly)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| taskId | number | 必填 | 任务id |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| taskId | number | 必填 | 任务id |
| status | number | 必填 | 任务状态：0-待处理 1-进行中 2-已完成 3-失败 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "taskId": 2016,
        "status": 2
    },
    "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 从回收站恢复文件

- 用途：从回收站恢复文件。
- HTTP：POST 域名 + /api/v1/file/recover
- 官方来源：[从回收站恢复文件](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/kx9f8b6wk6g55uwy)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileIDs | array | 必填 | 文件id数组,一次性最大不能超过 100 个文件 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| abnormalFileIDs | array | 必填 | 异常文件目录ID（父级目录不存在），可使用**还原文件到指定目录**接口；无异常文件则为空 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "abnormalFileIDs": [123,456]
  },
  "x-traceID": "..."
}
```

**注意事项**
- 将回收站的文件恢复至删除前的位置

### 还原文件到指定目录

- 用途：还原文件到指定目录。
- HTTP：POST 域名 + /api/v1/file/recover/by_path
- 官方来源：[还原文件到指定目录](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/cl24atug2sviq12z)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileIDs | array | 必填 | 文件id数组,一次性最大不能超过 100 个文件 |
| parentFileID | integer | 必填 | 指定目录id |

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
- 将回收站的文件恢复至指定位置

### 获取单个文件详情

- 用途：获取单个文件详情。
- HTTP：GET 域名 + /api/v1/file/detail
- 官方来源：[获取单个文件详情](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/owapsz373dzwiqbp)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileID | number | 必填 | 文件ID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| fileID | number | 必填 | 文件ID |
| filename | string | 必填 | 文件名 |
| type | number | 必填 | 0-文件  1-文件夹 |
| size | number | 必填 | 文件大小 |
| etag | string | 必填 | md5 |
| status | number | 必填 | 文件审核状态。 大于 100 为审核驳回文件 |
| parentFileID | number | 必填 | 父级ID |
| createAt | string | 必填 | 文件创建时间 |
| trashed | number | 必填 | 该文件是否在回收站<br>0否、1是 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "fileID": 14749954,
    "filename": "Keyshot_win64_2024.exe",
    "type": 0,
    "size": 1163176272,
    "etag": "...",
    "status": 2,
    "parentFileID": 14749926,
    "createAt": "2025-02-28 09:25:54",
    "trashed": 0
  },
  "x-traceID": "..."
}
```

**注意事项**
- 支持查询单文件夹包含文件大小

### 获取多个文件详情

- 用途：获取多个文件详情。
- HTTP：POST 域名 + /api/v1/file/infos
- 官方来源：[获取多个文件详情](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/cqqayfuxybegrlru)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileIds | []number | 是 | 文件id |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| --- | :---: | :---: | --- |
| fileList | array | 是 |  |
| fileList[*].fileId | number | 是 | 文件ID |
| fileList[*].filename | string | 是 | 文件名 |
| fileList[*].parentFileId | number | 是 | 目录ID |
| fileList[*].type | number | 是 | 0-文件  1-文件夹 |
| fileList[*].etag | string | 是 | md5 |
| fileList[*].size | number | 是 | 文件大小 |
| fileList[*].category | number | 是 | 文件分类：0-未知 1-音频 2-视频 3-图片 |
| fileList[*].status | number | 是 | 文件审核状态。 大于 100 为审核驳回文件 |
| fileList[*].punishFlag | number | 是 | 惩罚标记 |
| fileList[*].s3KeyFlag | string | 是 | 关联s3_key的初始用户标识 |
| fileList[*].storageNode | string | 是 | m0是ceph，m1以上为minio |
| fileList[*].trashed | number | 是 | 是否在回收站：[0：否，1：是] |
| fileList[*].createAt | string | 是 | 创建时间 |
| fileList[*].updateAt | number | 是 | 更新时间 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "fileList": [
      {
        "fileId": 144851864,
        "filename": "work",
        "parentFileId": 0,
        "type": 1,
        "etag": "",
        "size": 0,
        "category": 0,
        "status": 0,
        "punishFlag": 0,
        "s3KeyFlag": "1814435920-0",
        "storageNode": "m0",
        "trashed": 0,
        "createAt": "2024-11-08 16:33:50",
        "updateAt": "2024-11-08 16:33:50"
      }
    ]
  },
  "x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取文件列表（推荐）

- 用途：获取文件列表（推荐）。
- HTTP：GET 域名 + /api/v2/file/list
- 官方来源：[获取文件列表（推荐）](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/zrip9b0ye81zimv4)

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
| <br> lastFileId | <br>number | <br>选填 | <br>翻页查询时需要填写 |

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
|  | trashed | int | 必填 | 文件是否在回收站标识：0 否 1是 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "lastFileId": -1,
    "fileList": [
      {
        "fileId": 5373646,
        "filename": "download.mp4",
        "parentFileId": 14663228,
        "type": 0,
        "etag": "af..(过长省略)",
        "size": 518564433,
        "category": 2,
        "status": 2,
        "punishFlag": 0,
        "s3KeyFlag": "x-0",
        "storageNode": "m16",
        "trashed": 0,
        "createAt": "2024-04-30 11:58:36",
        "updateAt": "2025-02-24 17:56:45"
      },
      {
        "fileId": 8903127,
        "filename": "2.json.gz",
        "parentFileId": 14663228,
        "type": 0,
        "etag": "46..(过长省略)",
        "size": 221476024,
        "category": 10,
        "status": 0,
        "punishFlag": 0,
        "s3KeyFlag": "x-0",
        "storageNode": "m50",
        "trashed": 0,
        "createAt": "2024-08-16 13:18:09",
        "updateAt": "2025-02-24 17:56:29"
      },
      {
        "fileId": 10171597,
        "filename": "6-m.mp4",
        "parentFileId": 14663228,
        "type": 0,
        "etag": "7a..(过长省略)",
        "size": 367628427,
        "category": 2,
        "status": 2,
        "punishFlag": 0,
        "s3KeyFlag": "x-0",
        "storageNode": "m51",
        "trashed": 0,
        "createAt": "2024-09-27 09:39:46",
        "updateAt": "2025-02-24 17:56:24"
      },
      {
        "fileId": 14710240,
        "filename": "测试二级目录",
        "parentFileId": 14663228,
        "type": 1,
        "etag": "",
        "size": 0,
        "category": 0,
        "status": 0,
        "punishFlag": 0,
        "s3KeyFlag": "x-0",
        "storageNode": "m0",
        "trashed": 0,
        "createAt": "2025-02-24 17:57:01",
        "updateAt": "2025-02-24 17:57:01"
      }
    ]
  },
  "x-traceID": "..."
}
```

**注意事项**
- 注意：此接口查询结果包含回收站的文件，需自行根据字段`trashed`判断处理

### 获取文件列表（旧）

- 用途：获取文件列表（旧）。
- HTTP：GET 域名 + /api/v1/file/list
- 官方来源：[获取文件列表（旧）](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/hosdqqax0knovnm2)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| parentFileId | number | 必填 | 文件夹ID，根目录传 0 |
| page | number | 必填 | 页码数 |
| limit | number | 必填 | 每页文件数量，最大不超过100 |
| orderBy | string | 必填 | 排序字段,例如:file_id、size、file_name |
| orderDirection | string | 必填 | 排序方向:asc、desc |
| trashed | bool | 选填 | 是否查看回收站的文件 |
| searchData | string | 选填 | 搜索关键字 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| fileList | array | 必填 |  |
| fileList[*].fileID | number | 必填 | 文件ID |
| fileList[*].filename | string | 必填 | 文件名 |
| fileList[*].type | number | 必填 | 0-文件  1-文件夹 |
| fileList[*].size | number | 必填 | 文件大小 |
| fileList[*].etag | boolean | 必填 | md5 |
| fileList[*].status | number | 必填 | 文件审核状态。 大于 100 为审核驳回文件 |
| fileList[*].parentFileId | number | 必填 | 目录ID |
| fileList[*].parentName | string | 必填 | 目录名 |
| fileList[*].category | number | 必填 | 文件分类：0-未知 1-音频 2-视频 3-图片 |
| fileList[*].contentType | number | 必填 | 文件类型 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "total": 4,
    "fileList": [
      {
        "fileID": 14710240,
        "filename": "测试二级目录",
        "parentFileId": 14663228,
        "parentName": "",
        "type": 1,
        "etag": "",
        "size": 0,
        "contentType": "0",
        "category": 0,
        "hidden": false,
        "status": 0,
        "punishFlag": 0,
        "s3KeyFlag": "x-0",
        "storageNode": "m0",
        "createAt": "2025-02-24 17:57:01 +0800 CST",
        "updateAt": "2025-02-24 17:57:01 +0800 CST",
        "thumbnail": "",
        "downloadUrl": ""
      },
      {
        "fileID": 10171597,
        "filename": "6-m.mp4",
        "parentFileId": 14663228,
        "parentName": "",
        "type": 0,
        "etag": "7a...(过长省略)",
        "size": 367628427,
        "contentType": "0",
        "category": 2,
        "hidden": false,
        "status": 2,
        "punishFlag": 0,
        "s3KeyFlag": "x-0",
        "storageNode": "m51",
        "createAt": "2024-09-27 09:39:46 +0800 CST",
        "updateAt": "2025-02-24 17:56:24 +0800 CST",
        "thumbnail": "",
        "downloadUrl": ""
      },
      {
        "fileID": 8903127,
        "filename": "2.json.gz",
        "parentFileId": 14663228,
        "parentName": "",
        "type": 0,
        "etag": "46...(过长省略)",
        "size": 221476024,
        "contentType": "0",
        "category": 10,
        "hidden": false,
        "status": 0,
        "punishFlag": 0,
        "s3KeyFlag": "x-0",
        "storageNode": "m50",
        "createAt": "2024-08-16 13:18:09 +0800 CST",
        "updateAt": "2025-02-24 17:56:29 +0800 CST",
        "thumbnail": "",
        "downloadUrl": ""
      },
      {
        "fileID": 5373646,
        "filename": "download.mp4",
        "parentFileId": 14663228,
        "parentName": "",
        "type": 0,
        "etag": "af...(过长省略)",
        "size": 518564433,
        "contentType": "0",
        "category": 2,
        "hidden": false,
        "status": 2,
        "punishFlag": 0,
        "s3KeyFlag": "x-0",
...
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 移动

- 用途：移动。
- HTTP：POST 域名 + /api/v1/file/move
- 官方来源：[移动](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/rsyfsn1gnpgo4m4f)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| fileIDs | array | 必填 | 文件id数组 |
| toParentFileID | number | 必填 | 要移动到的目标文件夹id，移动到根目录时填写 0 |

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

### 下载

- 用途：下载。
- HTTP：GET 域名 + /api/v1/file/download_info
- 官方来源：[下载](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/fnf60phsushn8ip2)

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
| :---: | :---: | :---: | :---: |
| downloadUrl | string | 是 | 下载地址 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "downloadUrl": "https://..."
    },
    "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。
