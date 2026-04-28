# Chest123.PanSdk

`Chest123.PanSdk` 是 123 云盘开放平台的 C#/.NET SDK。它把官方开放接口封装成适合 .NET 项目使用的异步 API，开发者通常只需要创建 `Pan123Client`，然后调用 `client.Files`、`client.Upload`、`client.DirectLink` 等模块即可。

这个 SDK 适合在 ASP.NET Core、Worker Service、控制台程序、Windows 桌面程序、后台任务服务中使用。目标框架为 `netstandard2.0;net8.0`。

## 功能范围

- 自动获取、缓存和刷新开发者 `access_token`
- 自动添加公共请求头：`Platform: open_platform`
- 自动添加鉴权请求头：`Authorization: Bearer <access_token>`
- 封装文件管理、上传、下载链接、分享、离线下载、用户信息、直链、图床、视频转码等模块
- 提供 V2 上传高层 helper：`Upload.UploadFileAsync(...)`
- 支持底层兜底请求：`client.SendAsync<T>(...)`
- 官方响应 `code != 0` 时抛出 `Pan123ApiException`

## 安装

发布到 NuGet 后安装：

```bash
dotnet add package Chest123.PanSdk
```

在本仓库内开发或调试时，可以直接引用项目：

```bash
dotnet add reference ./sdk-dotnet/src/Chest123.PanSdk/Chest123.PanSdk.csproj
```

## 快速开始

```csharp
using Chest123.PanSdk;
using Chest123.PanSdk.Models;

var client = new Pan123Client(new Pan123ClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("PAN123_CLIENT_ID"),
    ClientSecret = Environment.GetEnvironmentVariable("PAN123_CLIENT_SECRET")
});

var files = await client.Files.ListAsync(new FileListRequest
{
    ParentFileId = 0,
    Limit = 100
});

foreach (var file in files?.FileList ?? [])
{
    Console.WriteLine($"{file.Filename} {file.FileID ?? file.FileId}");
}
```

## 客户端配置

```csharp
var client = new Pan123Client(new Pan123ClientOptions
{
    ClientId = "your-client-id",
    ClientSecret = "your-client-secret",
    BaseUrl = "https://open-api.123pan.com",
    Platform = "open_platform",
    Timeout = TimeSpan.FromSeconds(30)
});
```

| 选项 | 说明 | 默认值 |
| --- | --- | --- |
| `ClientId` | 123 云盘开放平台应用的 client_id | 无 |
| `ClientSecret` | 123 云盘开放平台应用的 client_secret | 无 |
| `AccessToken` | 已有 access token，可跳过首次自动获取 | 无 |
| `TokenExpiresAt` | access token 过期时间 | 无 |
| `BaseUrl` | API 基础地址 | `https://open-api.123pan.com` |
| `Platform` | 官方要求的公共请求头 | `open_platform` |
| `Timeout` | HTTP 请求超时时间 | 30 秒 |
| `HttpClient` | 自定义 `HttpClient`，方便接入代理、日志或测试 | 无 |

## 鉴权

### 自动鉴权

大多数业务接口不需要手动获取 token。SDK 会在第一次请求前自动调用 `/api/v1/access_token`，并缓存返回的 `accessToken`。

```csharp
await client.User.GetInfoAsync();
```

### 手动获取 access_token

```csharp
var token = await client.Auth.GetAccessTokenAsync();
Console.WriteLine(token.AccessToken);
Console.WriteLine(token.ExpiredAt);
```

### 使用已有 access_token

```csharp
var client = new Pan123Client(new Pan123ClientOptions
{
    AccessToken = "existing-token",
    TokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
});
```

也可以在运行中设置：

```csharp
client.Auth.SetAccessToken("existing-token", DateTimeOffset.UtcNow.AddHours(1));
```

### OAuth token

OAuth 授权换 token 使用 `GetOAuthTokenAsync`：

```csharp
var oauth = await client.Auth.GetOAuthTokenAsync(new OAuthTokenRequest
{
    ClientId = "your-client-id",
    ClientSecret = "your-client-secret",
    GrantType = "authorization_code",
    Code = "oauth-code",
    RedirectUri = "https://example.com/callback"
});
```

## 文件管理

文件管理接口位于 `client.Files`。

### 获取文件列表

```csharp
var files = await client.Files.ListAsync(new FileListRequest
{
    ParentFileId = 0,
    Limit = 100
});
```

`ParentFileId = 0` 表示根目录。分页时把上一次响应里的 `LastFileId` 传给下一次请求。

```csharp
var nextPage = await client.Files.ListAsync(new FileListRequest
{
    ParentFileId = 0,
    Limit = 100,
    LastFileId = files?.LastFileId
});
```

### 创建目录

```csharp
await client.Files.MkdirAsync(new
{
    parentID = 0,
    name = "SDK-Test"
});
```

### 获取文件详情

```csharp
var detail = await client.Files.DetailAsync(new
{
    fileID = 123456
});
```

### 获取多个文件详情

```csharp
var infos = await client.Files.InfosAsync(new
{
    fileIDs = new[] { 123456, 123457 }
});
```

### 重命名、移动、复制、删除

```csharp
await client.Files.RenameAsync(new
{
    fileID = 123456,
    filename = "new-name.txt"
});

await client.Files.MoveAsync(new
{
    fileIDs = new[] { 123456 },
    toParentFileID = 0
});

await client.Files.CopyAsync(new
{
    fileID = 123456,
    toParentFileID = 0
});

await client.Files.TrashAsync(new
{
    fileIDs = new[] { 123456 }
});
```

### 获取下载链接

```csharp
var info = await client.Files.GetDownloadInfoAsync(new DownloadInfoRequest
{
    FileId = 123456
});

Console.WriteLine(info?.DownloadUrl);
```

真实下载文件：

```csharp
using var http = new HttpClient();
var bytes = await http.GetByteArrayAsync(info!.DownloadUrl);
await File.WriteAllBytesAsync("downloaded-file.bin", bytes);
```

## 上传文件

推荐使用 V2 上传模块：`client.Upload`。

### 一行上传

`UploadFileAsync` 会自动计算文件 MD5：

- 文件大小小于等于 1GB：使用 V2 单步上传
- 文件大小大于 1GB：使用 V2 分片上传
- 文件名默认使用本地文件名，也可以通过 `Filename` 指定

```csharp
var upload = await client.Upload.UploadFileAsync(new UploadFileRequest
{
    FilePath = @"E:\Documents\nodejs\Chest123\Test.txt",
    ParentFileID = 0,
    Filename = "Test-from-dotnet-sdk.txt",
    Duplicate = 1
});

Console.WriteLine(upload.FileID);
```

`Duplicate` 使用官方字段，含义以 123 云盘官方文档为准。常见测试场景可以传 `1`，避免同名文件导致上传失败。

### V2 手动上传流程

如果你要完全控制上传流程，可以直接调用底层 V2 方法。

获取上传域名：

```csharp
var domains = await client.Upload.DomainAsync();
var uploadUrl = domains![0];
```

创建文件：

```csharp
var created = await client.Upload.CreateAsync(new UploadCreateRequest
{
    ParentFileID = 0,
    Filename = "large.zip",
    Etag = "file-md5",
    Size = 1024,
    Duplicate = 1
});
```

上传分片：

```csharp
await using var slice = File.OpenRead("part.bin");
await client.Upload.SliceAsync(
    uploadUrl: created!.Servers![0],
    preuploadID: created.PreuploadID!,
    sliceNo: 1,
    sliceMD5: "slice-md5",
    slice: slice,
    fileName: "part.bin");
```

完成上传：

```csharp
var completed = await client.Upload.CompleteAsync(created!.PreuploadID!);
Console.WriteLine(completed?.FileID);
```

### sha1 秒传

```csharp
var result = await client.Upload.Sha1ReuseAsync(new
{
    parentFileID = 0,
    filename = "existing-file.bin",
    etag = "md5",
    sha1 = "sha1",
    size = 1024,
    duplicate = 1
});
```

## 直链

直链接口位于 `client.DirectLink`。启用和禁用直链空间会改变账号配置，请在业务侧明确确认后再调用。

获取文件直链：

```csharp
var direct = await client.DirectLink.GetUrlAsync(new DirectLinkUrlRequest
{
    FileID = 123456
});

Console.WriteLine(direct?.Url);
```

启用或禁用直链空间：

```csharp
await client.DirectLink.EnableAsync(new { fileID = 123456 });
await client.DirectLink.DisableAsync(new { fileID = 123456 });
```

刷新直链缓存：

```csharp
await client.DirectLink.RefreshCacheAsync();
```

获取日志与 IP 黑名单：

```csharp
await client.DirectLink.GetTrafficLogsAsync(new { page = 1, limit = 100 });
await client.DirectLink.GetOfflineLogsAsync(new { page = 1, limit = 100 });
await client.DirectLink.ListIpBlacklistAsync(new { page = 1, limit = 100 });
```

## 分享、离线下载、用户信息

```csharp
var user = await client.User.GetInfoAsync();
```

创建分享：

```csharp
var share = await client.Share.CreateAsync(new
{
    fileIDList = new[] { 123456 },
    shareName = "shared-by-sdk"
});
```

获取分享列表：

```csharp
var shares = await client.Share.ListAsync(new { page = 1, limit = 100 });
```

创建离线下载任务：

```csharp
var task = await client.Offline.CreateDownloadTaskAsync(new
{
    url = "https://example.com/file.zip",
    parentFileID = 0
});
```

查询离线下载进度：

```csharp
var progress = await client.Offline.GetDownloadProcessAsync(new
{
    taskID = "task-id"
});
```

## 图床 OSS

图床相关接口位于 `client.Oss`。这些接口保留官方请求字段，适合用匿名对象传参。

```csharp
await client.Oss.MkdirAsync(new
{
    parentID = 0,
    name = "images"
});

await client.Oss.CreateAsync(new
{
    parentID = 0,
    filename = "image.png",
    etag = "md5",
    size = 1024
});

await client.Oss.ListAsync(new
{
    parentID = 0,
    page = 1,
    limit = 100
});
```

## 视频转码

视频转码相关接口位于 `client.Transcode`。

```csharp
var videos = await client.Transcode.ListCloudDiskVideosAsync(new FileListRequest
{
    ParentFileId = 0,
    Limit = 100
});

await client.Transcode.UploadFromCloudDiskAsync(new
{
    fileID = 123456
});

await client.Transcode.VideoResolutionsAsync(new
{
    fileID = 123456
});
```

## 所有模块方法

### Auth

| 方法 | 官方接口 |
| --- | --- |
| `GetAccessTokenAsync()` | `POST /api/v1/access_token` |
| `GetOAuthTokenAsync(request)` | `POST /api/v1/oauth2/access_token` |
| `SetAccessToken(token, expiresAt)` | 本地设置 token |
| `EnsureAccessTokenAsync()` | 本地确保 token 可用 |

### Files

| 方法 | 官方接口 |
| --- | --- |
| `MkdirAsync(request)` | `POST /upload/v1/file/mkdir` |
| `RenameAsync(request)` | `PUT /api/v1/file/name` |
| `BatchRenameAsync(request)` | `POST /api/v1/file/rename` |
| `TrashAsync(request)` | `POST /api/v1/file/trash` |
| `CopyAsync(request)` | `POST /api/v1/file/copy` |
| `AsyncCopyAsync(request)` | `POST /api/v1/file/async/copy` |
| `AsyncCopyProcessAsync(request)` | `GET /api/v1/file/async/copy/process` |
| `RecoverAsync(request)` | `POST /api/v1/file/recover` |
| `RecoverByPathAsync(request)` | `POST /api/v1/file/recover/by_path` |
| `DetailAsync(request)` | `GET /api/v1/file/detail` |
| `InfosAsync(request)` | `POST /api/v1/file/infos` |
| `ListAsync(request)` | `GET /api/v2/file/list` |
| `ListLegacyAsync(request)` | `GET /api/v1/file/list` |
| `MoveAsync(request)` | `POST /api/v1/file/move` |
| `GetDownloadInfoAsync(request)` | `GET /api/v1/file/download_info` |

### Upload V2

| 方法 | 官方接口 |
| --- | --- |
| `CreateAsync(request)` | `POST /upload/v2/file/create` |
| `SliceAsync(...)` | `POST /upload/v2/file/slice` |
| `CompleteAsync(preuploadID)` | `POST /upload/v2/file/upload_complete` |
| `DomainAsync()` | `GET /upload/v2/file/domain` |
| `SingleAsync(...)` | `POST /upload/v2/file/single/create` |
| `Sha1ReuseAsync(request)` | `POST /upload/v2/file/sha1_reuse` |
| `UploadFileAsync(request)` | 高层 helper |

### Upload V1

| 方法 | 官方接口 |
| --- | --- |
| `CreateAsync(request)` | `POST /upload/v1/file/create` |
| `GetUploadUrlAsync(request)` | `POST /upload/v1/file/get_upload_url` |
| `ListUploadPartsAsync(request)` | `POST /upload/v1/file/list_upload_parts` |
| `CompleteAsync(request)` | `POST /upload/v1/file/upload_complete` |
| `AsyncResultAsync(request)` | `POST /upload/v1/file/upload_async_result` |

### Share

| 方法 | 官方接口 |
| --- | --- |
| `CreateAsync(request)` | `POST /api/v1/share/create` |
| `ListAsync(request)` | `GET /api/v1/share/list` |
| `UpdateAsync(request)` | `PUT /api/v1/share/list/info` |
| `CreatePaidAsync(request)` | `POST /api/v1/share/content-payment/create` |
| `ListPaidAsync(request)` | `GET /api/v1/share/payment/list` |
| `UpdatePaidAsync(request)` | `PUT /api/v1/share/list/payment/info` |

### Offline

| 方法 | 官方接口 |
| --- | --- |
| `CreateDownloadTaskAsync(request)` | `POST /api/v1/offline/download` |
| `GetDownloadProcessAsync(request)` | `GET /api/v1/offline/download/process` |

### DirectLink

| 方法 | 官方接口 |
| --- | --- |
| `EnableAsync(request)` | `POST /api/v1/direct-link/enable` |
| `DisableAsync(request)` | `POST /api/v1/direct-link/disable` |
| `GetUrlAsync(request)` | `GET /api/v1/direct-link/url` |
| `RefreshCacheAsync()` | `POST /api/v1/direct-link/cache/refresh` |
| `GetTrafficLogsAsync(request)` | `GET /api/v1/direct-link/log` |
| `GetOfflineLogsAsync(request)` | `GET /api/v1/direct-link/offline/logs` |
| `SetIpBlacklistEnabledAsync(request)` | `POST /api/v1/developer/config/forbide-ip/switch` |
| `UpdateIpBlacklistAsync(request)` | `POST /api/v1/developer/config/forbide-ip/update` |
| `ListIpBlacklistAsync(request)` | `GET /api/v1/developer/config/forbide-ip/list` |

### Oss

| 方法 | 官方接口 |
| --- | --- |
| `MkdirAsync(request)` | `POST /upload/v1/oss/file/mkdir` |
| `CreateAsync(request)` | `POST /upload/v1/oss/file/create` |
| `GetUploadUrlAsync(request)` | `POST /upload/v1/oss/file/get_upload_url` |
| `CompleteAsync(request)` | `POST /upload/v1/oss/file/upload_complete` |
| `AsyncResultAsync(request)` | `POST /upload/v1/oss/file/upload_async_result` |
| `CreateCopyTaskAsync(request)` | `POST /api/v1/oss/source/copy` |
| `GetCopyProcessAsync(request)` | `GET /api/v1/oss/source/copy/process` |
| `GetCopyFailListAsync(request)` | `GET /api/v1/oss/source/copy/fail` |
| `MoveAsync(request)` | `POST /api/v1/oss/file/move` |
| `DeleteAsync(request)` | `POST /api/v1/oss/file/delete` |
| `DetailAsync(request)` | `GET /api/v1/oss/file/detail` |
| `ListAsync(request)` | `POST /api/v1/oss/file/list` |
| `CreateOfflineMigrationAsync(request)` | `POST /api/v1/oss/offline/download` |
| `GetOfflineMigrationAsync(request)` | `GET /api/v1/oss/offline/download/process` |

### Transcode

| 方法 | 官方接口 |
| --- | --- |
| `ListCloudDiskVideosAsync(request)` | `GET /api/v2/file/list` |
| `UploadFromCloudDiskAsync(request)` | `POST /api/v1/transcode/upload/from_cloud_disk` |
| `ListFilesAsync(request)` | `GET /api/v2/file/list` |
| `FolderInfoAsync(request)` | `POST /api/v1/transcode/folder/info` |
| `VideoResolutionsAsync(request)` | `POST /api/v1/transcode/video/resolutions` |
| `ListAsync(request)` | `GET /api/v1/video/transcode/list` |
| `TranscodeAsync(request)` | `POST /api/v1/transcode/video` |
| `RecordAsync(request)` | `POST /api/v1/transcode/video/record` |
| `ResultAsync(request)` | `POST /api/v1/transcode/video/result` |
| `DeleteAsync(request)` | `POST /api/v1/transcode/delete` |
| `DownloadOriginalAsync(request)` | `POST /api/v1/transcode/file/download` |
| `DownloadM3u8OrTsAsync(request)` | `POST /api/v1/transcode/m3u8_ts/download` |
| `DownloadAllAsync(request)` | `POST /api/v1/transcode/file/download/all` |

## 请求参数写法

SDK 对常用接口提供了强类型 DTO，例如：

- `FileListRequest`
- `DownloadInfoRequest`
- `UploadCreateRequest`
- `UploadFileRequest`
- `DirectLinkUrlRequest`

对字段复杂、官方变化较多或文档字段未完全固定的接口，SDK 使用 `object request`。推荐传匿名对象，并保持官方字段名：

```csharp
await client.Files.DetailAsync(new
{
    fileID = 123456
});
```

C# 属性大小写不会被 SDK 自动改写成其它含义。对于官方同时出现的字段，例如 `fileID`、`fileId`、`parentFileID`、`parentFileId`，SDK 保留差异，不擅自合并。

## 底层请求

如果官方新增了接口，而 SDK 还没提供对应方法，可以临时使用 `SendAsync<T>`：

```csharp
var data = await client.SendAsync<object>(
    HttpMethod.Get,
    "/api/v1/new/endpoint",
    new Pan123RequestOptions
    {
        Query = new { fileID = 123456 }
    });
```

POST JSON：

```csharp
var data = await client.SendAsync<object>(
    HttpMethod.Post,
    "/api/v1/new/endpoint",
    new Pan123RequestOptions
    {
        Body = new { fileID = 123456 }
    });
```

## 错误处理

当 HTTP 状态码不是成功状态，或官方响应里的 `code != 0` 时，SDK 会抛出 `Pan123ApiException`。

```csharp
try
{
    await client.User.GetInfoAsync();
}
catch (Pan123ApiException ex)
{
    Console.WriteLine($"Code: {ex.Code}");
    Console.WriteLine($"TraceId: {ex.TraceId}");
    Console.WriteLine($"HTTP Status: {ex.StatusCode}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine(ex.ResponseBody);
}
```

异常字段：

| 字段 | 说明 |
| --- | --- |
| `Code` | 官方响应 code |
| `TraceId` | 官方响应 trace id |
| `StatusCode` | HTTP 状态码 |
| `ResponseBody` | 原始响应内容，便于排查 |

## 本地开发

```bash
cd sdk-dotnet
dotnet restore
dotnet build Chest123.PanSdk.sln -c Release
dotnet test Chest123.PanSdk.sln -c Release
dotnet pack src/Chest123.PanSdk/Chest123.PanSdk.csproj -c Release
```

示例程序：

```bash
dotnet run --project examples/BasicUsage/BasicUsage.csproj
```

PowerShell 设置环境变量：

```powershell
$env:PAN123_CLIENT_ID="your-client-id"
$env:PAN123_CLIENT_SECRET="your-client-secret"
dotnet run --project .\examples\BasicUsage\BasicUsage.csproj
```

## Live Test

Live test 只有在设置环境变量后才会真实访问 123 云盘。它会执行：

- 获取 `access_token`
- 获取用户信息
- 列出指定目录文件
- 上传仓库根目录 `Test.txt`
- 获取上传文件详情
- 获取下载链接
- 真实下载文件并和本地 `Test.txt` 做字节比对

不会执行：

- 删除文件
- 回收文件
- 启用或禁用直链空间

运行：

```powershell
$env:PAN123_CLIENT_ID="your-client-id"
$env:PAN123_CLIENT_SECRET="your-client-secret"
$env:PAN123_PARENT_FILE_ID="0"
dotnet test .\tests\Chest123.PanSdk.Tests\Chest123.PanSdk.Tests.csproj -c Release --filter Live
```

## NuGet 发布

打包：

```bash
cd sdk-dotnet
dotnet test Chest123.PanSdk.sln -c Release
dotnet pack src/Chest123.PanSdk/Chest123.PanSdk.csproj -c Release
```

发布：

```bash
dotnet nuget push src/Chest123.PanSdk/bin/Release/Chest123.PanSdk.0.1.0.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

PowerShell 单行写法：

```powershell
dotnet nuget push .\src\Chest123.PanSdk\bin\Release\Chest123.PanSdk.0.1.0.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

## 维护约定

- `src/`：SDK 源码
- `tests/`：单元测试和 live test
- `examples/`：最小使用示例
- `bin/`、`obj/`：构建产物，不提交
- `.nupkg`：NuGet 包产物，不提交

运行时依赖说明：

- `net8.0`：使用 .NET 内置 `System.Text.Json`
- `netstandard2.0`：通过 Microsoft 官方 `System.Text.Json` 包补齐 JSON 能力
- 不引入第三方 HTTP、JSON、上传库

官方 API 事实来源在仓库根目录的 `123PanDoc`，新增或调整接口时应先对照 `123PanDoc/99-endpoint-index.md`。
