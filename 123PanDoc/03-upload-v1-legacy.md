# 上传 V1（旧）

官方来源：[123云盘开放平台](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced)

## 本模块接口

- 说明页 无 - 上传流程说明
- POST /upload/v1/file/create - 创建文件
- POST /upload/v1/file/get_upload_url - 获取上传地址&上传分片
- POST /upload/v1/file/list_upload_parts - 列举已上传分片（非必需）
- POST /upload/v1/file/upload_complete - 上传完毕
- POST /upload/v1/file/upload_async_result - 异步轮询获取上传结果

## 详细说明

### 上传流程说明

- 用途：上传流程说明。
- HTTP：无独立 HTTP 接口
- 官方来源：[上传流程说明](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/il16qi0opiel4889)

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

### 创建文件

- 用途：创建文件。
- HTTP：POST 域名 + /upload/v1/file/create
- 官方来源：[创建文件](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/lrfuu3qe7q1ul8ig)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| parentFileID | number | 必填 | 父目录id，上传到根目录时填写 0 |
| filename | string | 必填 | 文件名要小于255个字符且不能包含以下任何字符："\/:*?\|><。（注：不能重名）<br>containDir 为 true 时，传入路径+文件名，例如：/你好/123/测试文件.mp4 |
| etag | string | 必填 | 文件md5 |
| size | number | 必填 | 文件大小，单位为 byte 字节 |
| duplicate | number | 非必填 | 当有相同文件名时，文件处理策略（1保留两者，新文件名将自动添加后缀，2覆盖原文件） |
| containDir | bool | 非必填 | 上传文件是否包含路径，默认false |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| fileID | number | 非必填 | 文件ID。当123云盘已有该文件,则会发生秒传。此时会将文件ID字段返回。唯一 |
| preuploadID | string | 必填 | 预上传ID(如果 reuse 为 true 时,该字段不存在) |
| reuse | boolean | 必填 | 是否秒传，返回true时表示文件已上传成功 |
| sliceSize | number | 必填 | 分片大小，必须按此大小生成文件分片再上传 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "fileID": 0,
    "reuse": false,
    "preuploadID": "WvjyUgonimrlBq2PVJ3bSyjPVJYP4IGeSxGdSly...(过长省略)",
    "sliceSize": 16777216
  },
  "x-traceID": "..."
}
```

**注意事项**
- 文件名要小于256个字符且不能包含以下任何字符："\/:*?\|><
- 文件名不能全部是空格
- 开发者上传单文件大小限制10GB

### 获取上传地址&上传分片

- 用途：获取上传地址&上传分片。
- HTTP：POST 域名 + /upload/v1/file/get_upload_url
- 官方来源：[获取上传地址&上传分片](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/sonz9n085gnz0n3m)

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
    "isMultipart": true
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 列举已上传分片（非必需）

- 用途：列举已上传分片（非必需）。
- HTTP：POST 域名 + /upload/v1/file/list_upload_parts
- 官方来源：[列举已上传分片（非必需）](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/dd28ws4bfn644cny)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| preuploadID | string | 必填 | 预备上传ID |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| parts | array | 必填 | 分片列表 |
| parts[*].partNumber | number | 必填 | 分片编号 |
| parts[*].size | number | 必填 | 分片大小 |
| parts[*].etag | string | 必填 | 分片md5 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "parts": [
      {
        "partNumber": "1",
        "size": 16777216,
        "etag": "..."
      }
    ]
  },
  "x-traceID": "..."
}
```

**注意事项**
- 该接口用于最后一片分片上传完成时，列出云端分片供用户自行比对。比对正确后调用上传完毕接口。当文件大小小于 sliceSize 分片大小时，无需调用该接口，该结果将返回空值。

### 上传完毕

- 用途：上传完毕。
- HTTP：POST 域名 + /upload/v1/file/upload_complete
- 官方来源：[上传完毕](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/hkdmcmvg437rfu6x)

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
| async | bool | 必填 | 是否需要异步查询上传结果。false为无需异步查询,已经上传完毕。true 为需要异步查询上传结果。 |
| completed | bool | 必填 | 上传是否完成 |
| fileID | number | 必填 | 上传完成文件id |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "async": true,
    "completed": false,
    "fileID": 0
  },
  "x-traceID": "..."
}
```

**注意事项**
- 文件上传完成后请求
- 调用该接口前,请优先列举已上传的分片,在本地进行 md5 比对。

### 异步轮询获取上传结果

- 用途：异步轮询获取上传结果。
- HTTP：POST 域名 + /upload/v1/file/upload_async_result
- 官方来源：[异步轮询获取上传结果](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/qgcosr6adkmm51h7)

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
| :---: | :---: | :---: | :---: |
| completed | bool | 必填 | 上传合并是否完成,如果为false,请至少1秒后发起轮询 |
| fileID | number | 必填 | 上传完成返回对应fileID |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "fileID": 14665463,
    "completed": true
  },
  "x-traceID": "..."
}
```

**注意事项**
- 异步轮询获取上传结果
