# 分享、离线下载与用户管理

官方来源：[123云盘开放平台](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced)

## 本模块接口

- POST /api/v1/share/create - 创建分享链接
- GET /api/v1/share/list - 获取分享链接列表
- PUT /api/v1/share/list/info - 修改分享链接
- POST /api/v1/share/content-payment/create - 创建付费分享链接
- GET /api/v1/share/payment/list - 获取付费分享链接列表
- PUT /api/v1/share/list/payment/info - 修改付费分享链接
- POST /api/v1/offline/download - 创建离线下载任务
- GET /api/v1/offline/download/process - 获取离线下载进度
- GET /api/v1/user/info - 获取用户信息

## 详细说明

### 创建分享链接

- 用途：创建分享链接。
- HTTP：POST 域名 + /api/v1/share/create
- 官方来源：[创建分享链接](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/gzco1pi656ha792z)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| shareName | string | 必填 | 分享链接 |
| shareExpire | number | 必填 | 分享链接有效期天数,该值为枚举<br>固定只能填写:1、7、30、0<br>填写0时代表永久分享 |
| fileIDList | string | 必填 | 分享文件ID列表,以逗号分割,最大只支持拼接100个文件ID,示例:1,2,3 |
| sharePwd | string | 选填 | 设置分享链接提取码 |
| trafficSwitch   | int | 选填 | 分享提取流量包   1 全部关闭<br>2 打开游客免登录提取<br>3 打开超流量用户提取<br>4 全部开启 |
| trafficLimitSwitch | int | 选填 | 分享提取流量包流量限制开关<br>1 关闭限制<br>2 打开限制 |
| trafficLimit   | int64 | 选填 | 分享提取流量包限制流量<br>单位：字节 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| shareID | number | 必填 | 分享ID |
| shareKey | string | 必填 | 分享码,请将分享码拼接至 https://www.123pan.com/s/ 后面访问,即是分享页面 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "shareID": 87187530,
    "shareKey": "PvitVv-nPeLH"
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取分享链接列表

- 用途：获取分享链接列表。
- HTTP：GET 域名 + /api/v1/share/list
- 官方来源：[获取分享链接列表](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/wyb121eub7cq6lym)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| limit | number | 必填 | 每页文件数量，最大不超过100 |
|  lastShareId | number | 选填 | 翻页查询时需要填写，默认为 0 |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| --- | --- | :---: | :---: | --- |
|  lastShareId   | | number | 必填 | -1代表最后一页（无需再翻页查询）<br>其他代表下一页开始的分享文件id，携带到请求参数中 |
| shareList  | | array | 必填 | 文件列表 |
|  | shareId  | number | 必填 | 分享ID  |
|  | shareKey   | string | 必填 | 分享码,请将分享码拼接至 https://www.123pan.com/s/ 后面访问,即是分享页面   |
|  | shareName   | string | 必填 | 分享链接名称 |
|  | expiration   | string | 必填 | 过期时间 |
|  | expired   | integer | 必填 | 是否失效<br>0  未失效<br>1  失效 |
|  | sharePwd   | string | 必填 |  分享链接提取码   |
|  | <br>trafficSwitch   | <br>integer | <br>必填 | 分享提取流量包   1 全部关闭<br>2 打开游客免登录提取<br>3 打开超流量用户提取<br>4 全部开启 |
|  | <br>trafficLimitSwitch | <br>integer | <br>必填 | 分享提取流量包流量限制开关<br>1 关闭限制<br>2 打开限制 |
|  | <br>trafficLimit   | <br>number | <br>必填 | 分享提取流量包限制流量<br>单位：字节 |
|  | bytesCharge | number | 必填 | 分享使用流量<br>单位：字节 |
|  | previewCount | number | 必填 | 预览次数 |
|  | downloadCount | number | 必填 | 下载次数 |
|  | saveCount | number | 必填 | 转存次数 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "shareList": [
      {
        "shareId": 87184746,
        "shareKey": "PvitVv-OScLH",
        "shareName": "123pan-service-worker.exe",
        "expiration": "2025-02-25 10:12:45",
        "expired": 0,
        "sharePwd": "3YqT",
        "trafficSwitch": 2,
        "trafficLimitSwitch": 1,
        "trafficLimit": 0,
        "previewCount": 1,
        "downloadCount": 2,
        "saveCount": 1,
        "bytesCharge": 0
      },
      {
        "shareId": 87184747,
        "shareKey": "PvitVv-RScLH",
        "shareName": "123pan-service-worker.exe",
        "expiration": "2025-02-25 10:13:07",
        "expired": 0,
        "sharePwd": "",
        "trafficSwitch": 2,
        "trafficLimitSwitch": 1,
        "trafficLimit": 0,
        "previewCount": 2,
        "downloadCount": 3,
        "saveCount": 1,
        "bytesCharge": 0
      }
    ],
    "lastShareId": -1
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 修改分享链接

- 用途：修改分享链接。
- HTTP：PUT 域名 + /api/v1/share/list/info
- 官方来源：[修改分享链接](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/tanghycrgh9istqr)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| shareIdList | []uint64 | 必填 | 分享链接ID列表，数组长度最大为100 |
| trafficSwitch   | int | 选填 | 分享提取流量包   1 全部关闭<br>2 打开游客免登录提取<br>3 打开超流量用户提取<br>4 全部开启 |
| trafficLimitSwitch | int | 选填 | 分享提取流量包流量限制开关<br>1 关闭限制<br>2 打开限制 |
| trafficLimit   | int64 | 选填 | 分享提取流量包限制流量<br>单位：字节 |

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

### 创建付费分享链接

- 用途：创建付费分享链接。
- HTTP：POST 域名 + /api/v1/share/content-payment/create
- 官方来源：[创建付费分享链接](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/wmlt71asp6ymbrfq)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| shareName | string | 必填 | 分享链接名称,链接名要小于35个字符且不能包含特殊字符 |
| fileIDList | string | 必填 | 分享文件ID列表,以逗号分割,最大只支持拼接100个文件ID,示例:1,2,3 |
| payAmount   | integer | 必填 | 请输入整数|最小金额1元|最大金额 1000 元 |
| isReward | integer | 选填 | 是否开启打赏<br>0 否<br>1 是 |
| resourceDesc   | string | 选填 | 资源描述 |
| trafficSwitch   | int | 选填 | 分享提取流量包   1 全部关闭<br>2 打开游客免登录提取<br>3 打开超流量用户提取<br>4 全部开启 |
| trafficLimitSwitch | int | 选填 | 分享提取流量包流量限制开关<br>1 关闭限制<br>2 打开限制 |
| trafficLimit   | int64 | 选填 | 分享提取流量包限制流量<br>单位：字节 |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| shareID | number | 必填 | 分享ID |
| shareKey | string | 必填 | 分享码,请将分享码拼接至 https://www.123pan.com/ps/ 后面访问,即是分享页面 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "shareID": 87187530,
    "shareKey": "PvitVv-nPeLH"
  },
  "x-traceID": "..."
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取付费分享链接列表

- 用途：获取付费分享链接列表。
- HTTP：GET 域名 + /api/v1/share/payment/list
- 官方来源：[获取付费分享链接列表](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/mxc7eq2x3la72mwg)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| limit | number | 必填 | 每页文件数量，最大不超过100 |
|  lastShareId | number | 选填 | 翻页查询时需要填写，默认为 0 |

**响应字段**
| **名称** | | **类型** | **是否必填** | **说明** |
| --- | --- | :---: | :---: | --- |
|  lastShareId   | | number | 必填 | -1代表最后一页（无需再翻页查询）<br>其他代表下一页开始的分享文件id，携带到请求参数中 |
| shareList  | | array | 必填 | 文件列表 |
|  | shareId  | number | 必填 | 分享ID  |
|  | shareKey   | string | 必填 | 分享码,请将分享码拼接至 <br>https://www.123pan.com/ps/ 后面访问,即是分享页面   |
|  | shareName   | string | 必填 | 分享链接名称 |
|  | payAmount | number | 必填 | 付费金额 |
|  | amount | number | 必填 | 分享收益 |
|  | expiration   | string | 必填 | 过期时间 |
|  | expired   | integer | 必填 | 是否失效<br>0  未失效<br>1  失效 |
|  | <br>trafficSwitch   | <br>integer | <br>必填 | 分享提取流量包   1 全部关闭<br>2 打开游客免登录提取<br>3 打开超流量用户提取<br>4 全部开启 |
|  | <br>trafficLimitSwitch | <br>integer | <br>必填 | 分享提取流量包流量限制开关<br>1 关闭限制<br>2 打开限制 |
|  | <br>trafficLimit   | <br>number | <br>必填 | 分享提取流量包限制流量<br>单位：字节 |
|  | bytesCharge | number | 必填 | 分享使用流量<br>单位：字节 |
|  | previewCount | number | 必填 | 预览次数 |
|  | downloadCount | number | 必填 | 下载次数 |
|  | saveCount | number | 必填 | 转存次数 |

**成功示例结构**
```json
{
    "code": 0,
    "message": "ok",
    "data": {
        "shareList": [
            {
                "shareId": 81252309,
                "shareKey": "JY1ZVv-9GqnH",
                "shareName": "张三",
                "expiration": "2099-12-31 08:00:00",
                "expired": 0,
                "trafficSwitch": 4,
                "trafficLimitSwitch": 2,
                "trafficLimit": 1231783478,
                "previewCount": 0,
                "downloadCount": 0,
                "saveCount": 0,
                "payAmount": 11.1,
                "auditStatus": 0,
                "amount": 0,
                "bytesCharge": 0,
                "createAt": "2025-07-07 18:36:24",
                "updateAt": "2025-07-07 18:56:12"
            }
        ],
        "lastShareId": -1
    },
    "x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 修改付费分享链接

- 用途：修改付费分享链接。
- HTTP：PUT 域名 + /api/v1/share/list/payment/info
- 官方来源：[修改付费分享链接](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/euz8kc7fcyye496g)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| shareIdList | []uint64 | 必填 | 分享链接ID列表，数组长度最大为100 |
| trafficSwitch   | int | 选填 | 分享提取流量包   1 全部关闭<br>2 打开游客免登录提取<br>3 打开超流量用户提取<br>4 全部开启 |
| trafficLimitSwitch | int | 选填 | 分享提取流量包流量限制开关<br>1 关闭限制<br>2 打开限制 |
| trafficLimit   | int64 | 选填 | 分享提取流量包限制流量<br>单位：字节 |

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

### 创建离线下载任务

- 用途：创建离线下载任务。
- HTTP：POST 域名 + /api/v1/offline/download
- 官方来源：[创建离线下载任务](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/he47hsq2o1xvgado)

**鉴权**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| Authorization | string | 必填 | 鉴权access_token |
| Platform | string | 必填 | 固定为:open_platform |

**请求参数**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | --- |
| url | string | 必填 | 下载资源地址(http/https) |
| fileName | string | 非必填 | 自定义文件名称 （需携带图片格式，支持格式：png, gif, jpeg, tiff, webp,jpg,tif,svg,bmp）   |
| dirID | number | 非必填 | 选择下载到指定目录ID。 示例:10023<br>注:不支持下载到根目录,默认会下载到名为"来自:离线下载"的目录中 |
| callBackUrl | string | 非必填 | 回调地址,当文件下载成功或者失败,均会通过回调地址通知。回调内容如下      url: 下载资源地址<br>status: 0 成功，1 失败<br>fileReason：失败原因<br>fileID:成功后,该文件在云盘上的ID      请求类型：POST   {<br>	"url": "[http://dc.com/resource.jpg",](http://dc.com/resource.jpg",)<br>	"status": 0, <br>	"failReason": "",<br>        "fileID":100<br>} |

**响应字段**
| **名称** | **类型** | **是否必填** | **说明** |
| :---: | :---: | :---: | :---: |
| taskID | number | 必填 | 离线下载任务ID,可通过该ID,调用查询任务进度接口获取下载进度 |

**成功示例结构**
```json
{
  "code": 0,
  "message": "ok",
  "data": {
    "taskID": 394756
  },
  "x-traceID": "..."
}
```

**注意事项**
- 离线下载任务仅支持 http/https 任务创建

### 获取离线下载进度

- 用途：获取离线下载进度。
- HTTP：GET 域名 + /api/v1/offline/download/process
- 官方来源：[获取离线下载进度](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/sclficr3t655pii5)

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
| :---: | :---: | :---: | --- |
| process | float | 必填 | 下载进度百分比,当文件下载失败,该进度将会归零 |
| status | int | 必填 | 下载状态:<br>0进行中、1下载失败、2下载成功、3重试中 |

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
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。

### 获取用户信息

- 用途：获取用户信息。
- HTTP：GET 域名 + /api/v1/user/info
- 官方来源：[获取用户信息](https://123yunpan.yuque.com/org-wiki-123yunpan-muaork/cr6ced/zgf9gyh7gvmdl4a3)

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
| uid | number | 必填 | 用户账号id |
| nickname | string | 必填 | 昵称 |
| headImage | string | 必填 | 头像 |
| passport | string | 必填 | 手机号码 |
| mail | string | 必填 | 邮箱 |
| spaceUsed | number | 必填 | 已用空间 |
| spacePermanent | number | 必填 | 永久空间 |
| spaceTemp | number | 必填 | 临时空间 |
| spaceTempExpr | string | 必填 | 临时空间到期日 |
| vip | bool | 必填 | 是否会员 |
| directTraffic | number | 必填 | 剩余直链流量 |
| isHideUID | bool | 必填 | 直链链接是否隐藏UID |
| httpsCount | number | 必填 | https数量 |
| vipInfo | array | 必填 | vip信息（非VIP该字段为null） |
| vipInfo.vipLevel | number | 必填 | 1，2，3 VIP SVIP 长期VIP |
| vipInfo.vipLabel | string | 必填 | VIP级别名称 |
| vipInfo.startTime | string | 必填 | 开始时间 |
| vipInfo.endTime | string | 必填 | 结束时间 |
| developerInfo.startTime | string | 必填 | 开发者权益开始时间 |
| developerInfo.endTime | string | 必填 | 开发者权益结束时间 |

**成功示例结构**
```json
{
	"code": 0,
	"message": "ok",
	"data": {
		"uid": 1814442530,
		"nickname": "志中测试专属号003",
		"headImage": "https://...",
		"passport": "15184637593",
		"mail": "",
		"spaceUsed": 459894954684,
		"spacePermanent": 6894212784062464,
		"spaceTemp": 0,
		"spaceTempExpr": 0,
		"vip": true,
		"directTraffic": 276046404229020,
		"isHideUID": false,
		"httpsCount": 9999998,
		"vipInfo": [
			{
				"vipLevel": 1,
				"vipLabel": "VIP",
				"startTime": "2025-07-06 11:12:53",
				"endTime": "2026-05-14 14:13:05"
			},
			{
				"vipLevel": 2,
				"vipLabel": "SVIP",
				"startTime": "2025-06-06 11:12:53",
				"endTime": "2025-07-06 11:12:52"
			},
			{
				"vipLevel": 3,
				"vipLabel": "长期VIP",
				"startTime": "2025-04-27 11:18:49",
				"endTime": "9999-12-31 23:59:59"
			}
		],
    "developerInfo": {
        "startTime": "2025-07-16 17:27:53",
        "endTime": "2026-07-16 17:27:52"
    }
	},
	"x-traceID": ""
}
```

**注意事项**
- 按官方页面参数和返回字段处理；业务错误以 `code` 与 `message` 为准。
