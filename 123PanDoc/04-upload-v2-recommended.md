# 上传 V2（推荐）

官方来源：[123云盘开放平台](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced)

## 本模块接口

- 说明页 无 - 上传流程说明
- POST /upload/v2/file/create - 创建文件
- POST /upload/v2/file/slice - 上传分片
- POST /upload/v2/file/upload_complete - 上传完毕
- GET /upload/v2/file/domain - 获取上传域名
- POST /upload/v2/file/single/create - 单步上传
- POST /upload/v2/file/sha1_reuse - sha1哈希值文件上传

## 详细说明

### 上传流程说明

- 用途：上传流程说明。
- HTTP：无独立 HTTP 接口
- 官方来源：[上传流程说明](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/xogi45g7okqk7svr)

**鉴权**
说明页，无独立鉴权参数。

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
官方页面未提供独立 JSON 响应示例。

**注意事项**
- 调用创建文件接口，接口返回的`reuse`为true时，表示秒传成功，上传结束。
- 非秒传情况将会返回预上传ID`preuploadID`与分片大小`sliceSize`，请将文件根据分片大小切分。
- 非秒传情况下返回`servers`为后续上传文件的对应域名（重要），多个任选其一。
- 该步骤准备工作，按照`sliceSize`将文件切分，并计算每个分片的MD5。
- 调用上传分片接口，传入对应参数，注意此步骤 `Content-Type: multipart/form-data`**。**
- 调用上传完毕接口，若接口返回的`completed`为 ture 且`fileID`不为0时，上传完成。
- 若接口返回的`completed`为 false 时，则需间隔1秒继续轮询此接口，获取上传最终结果。
- 调用该接口，在接口返回中你将获得多个上传域名，后续上传任务需要使用。
- 使用获取到的上传域名发起上传；
- 注意此步骤 `Content-Type: multipart/form-data`**。**

### 创建文件

- 用途：创建文件。
- HTTP：POST 域名 + /upload/v2/file/create
- 官方来源：[创建文件](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/txow0iqviqsgotfl)

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
| servers | array | 必填 | 上传地址 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "fileID": 0,
    "reuse": false,
    "preuploadID": "WvjyUgonimrlBq2PVJ3bSyjPVJYP4IGeSxGdSly...(过长省略)",
    "sliceSize": 16777216,
    "servers": [
      "https://..."
    ]
  },
  "x-traceID": "..."
}
```

**注意事项**
- 文件名要小于256个字符且不能包含以下任何字符："\/:*?\|><
- 文件名不能全部是空格
- 开发者上传单文件大小限制10GB

### 上传分片

- 用途：上传分片。
- HTTP：POST 上传域名 + /upload/v2/file/slice
- 官方来源：[上传分片](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/scs8yg89yz8immus)

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
| sliceMD5 | string | 必填 | 当前分片md5 |
| slice | file | 必填 | 分片二进制流 |

**响应字段**
无或官方未列出独立响应字段。

**成功示例结构**
```json
{
	"code": 0,
	"message": "ok",
	"data": null,
	"x-traceID": ""
}
```

**注意事项**
- 上传域名是`创建文件`接口响应中的`servers`

### 上传完毕

- 用途：上传完毕。
- HTTP：POST 域名 + /upload/v2/file/upload_complete
- 官方来源：[上传完毕](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/fzzc5o8gok517720)

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
| completed | bool | 必填 | 上传是否完成 |
| fileID | number | 必填 | 上传完成文件id |

**成功示例结构**
```json
{
	"code": 0,
	"message": "ok",
	"data": {
		"completed": true,
		"fileID": 11522654
	},
	"x-traceID": "..."
}
```

**注意事项**
- 分片上传完成后请求

### 获取上传域名

- 用途：获取上传域名。
- HTTP：GET 域名 + /upload/v2/file/domain
- 官方来源：[获取上传域名](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/agn8lolktbqie7p9)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
无或官方未列出独立请求参数。

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| data | array | 必填 | 上传域名，存在多个可以任选其一 |

**成功示例结构**
```json
{
	"code": 0,
	"message": "ok",
	"data": [
		"https://..."
	],
	"x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 单步上传

- 用途：单步上传。
- HTTP：POST 上传域名 + /upload/v2/file/single/create
- 官方来源：[单步上传](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/xhiht1uh3yp92pzc)

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
| file | file | 必填 | 文件二进制流 |
| duplicate | number | 非必填 | 当有相同文件名时，文件处理策略（1保留两者，新文件名将自动添加后缀，2覆盖原文件） |
| containDir | bool | 非必填 | 上传文件是否包含路径，默认false |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| fileID | number | 必填 | 文件ID。当123云盘已有该文件,则会发生秒传。此时会将文件ID字段返回。唯一 |
| completed | bool | 必填 | 是否上传完成（如果 completed 为 true 时，则说明上传完成） |

**成功示例结构**
```json
{
	"code": 0,
	"message": "ok",
	"data": {
		"fileID": 11522653,
		"completed": true
	},
	"x-traceID": ""
}
```

**注意事项**
- 文件名要小于256个字符且不能包含以下任何字符："\/:*?\|><
- 文件名不能全部是空格
- 此接口限制开发者上传单文件大小为1GB
- 上传域名是`获取上传域名`接口响应中的域名
- 此接口用于实现小文件单步上传一次HTTP请求交互即可完成上传

### sha1哈希值文件上传

- 用途：sha1哈希值文件上传。
- HTTP：POST 域名 + /upload/v2/file/sha1_reuse
- 官方来源：[sha1哈希值文件上传](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/de0et33ct3uhdfqs)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| parentFileID | number | 必填 | 父目录id，上传到根目录时填写 0 |
| filename | string | 必填 | 文件名要小于255个字符且不能包含以下任何字符："\/:*?\|><。（注：不能重名） |
| sha1 | string | 必填 | 文件sha1 |
| size | number | 必填 | 文件大小，单位为 byte 字节 |
| duplicate | number | 非必填 | 当有相同文件名时，文件处理策略（1保留两者，新文件名将自动添加后缀，2覆盖原文件） |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| fileID | number | 非必填 | 文件ID。当123云盘已有该文件,则会发生秒传。此时会将文件ID字段返回。唯一 |
| reuse | boolean | 必填 | 是否秒传，返回true时表示文件已上传成功 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "fileID": 0,
    "reuse": false
  }
  "x-traceID": "..."
}
```

**注意事项**
- 文件名要小于256个字符且不能包含以下任何字符："\/:*?\|><
- 文件名不能全部是空格
