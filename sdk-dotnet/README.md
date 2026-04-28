# Chest123.PanSdk

`Chest123.PanSdk` 是 123 云盘开放平台的 C#/.NET SDK。它把鉴权、文件管理、上传、下载链接、直链、分享、离线下载、图床和视频转码等接口封装成面向 .NET 开发者的异步 API。

SDK 面向服务端、桌面程序、Worker Service、ASP.NET Core、控制台工具等 .NET 项目使用。目标框架为 `netstandard2.0;net8.0`。

## 安装

发布到 NuGet 后：

```bash
dotnet add package Chest123.PanSdk
```

本仓库本地引用：

```bash
dotnet add reference ./sdk-dotnet/src/Chest123.PanSdk/Chest123.PanSdk.csproj
```

## 初始化客户端

```csharp
using Chest123.PanSdk;

var client = new Pan123Client(new Pan123ClientOptions
{
    ClientId = Environment.GetEnvironmentVariable("PAN123_CLIENT_ID"),
    ClientSecret = Environment.GetEnvironmentVariable("PAN123_CLIENT_SECRET")
});
```

SDK 会自动获取并缓存 `access_token`，请求时自动添加 `Platform: open_platform` 和 `Authorization: Bearer <token>`。

## 获取用户信息

```csharp
var user = await client.User.GetInfoAsync();
Console.WriteLine(user);
```

## 获取文件列表

```csharp
using Chest123.PanSdk.Models;

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

## 上传文件

`UploadFileAsync` 默认使用 V2 上传流程：自动计算 MD5，小于等于 1GB 使用单步上传，大于 1GB 使用分片上传。

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

## 获取下载链接并下载

```csharp
var info = await client.Files.GetDownloadInfoAsync(new DownloadInfoRequest
{
    FileId = upload.FileID
});

using var http = new HttpClient();
var bytes = await http.GetByteArrayAsync(info!.DownloadUrl);
await File.WriteAllBytesAsync("downloaded.txt", bytes);
```

## 获取直链

直链相关接口在 `client.DirectLink` 下。启用或禁用直链空间会改变账号配置，SDK 不会在测试中自动执行这类操作。

```csharp
var link = await client.DirectLink.GetUrlAsync(new DirectLinkUrlRequest
{
    FileID = upload.FileID
});

Console.WriteLine(link?.Url);
```

## 模块入口

| 模块 | 说明 |
| --- | --- |
| `client.Auth` | access token、OAuth token、手动设置 token |
| `client.Files` | 目录、列表、详情、重命名、删除、移动、复制、恢复、下载信息 |
| `client.Upload` | V2 上传、单步上传、分片上传、sha1 秒传、高层 `UploadFileAsync` |
| `client.UploadV1` | V1 旧上传流程 |
| `client.Share` | 普通分享、付费分享 |
| `client.Offline` | 离线下载任务与进度 |
| `client.User` | 用户信息 |
| `client.DirectLink` | 直链空间、直链 URL、缓存刷新、日志、IP 黑名单 |
| `client.Oss` | 图床上传、复制、移动、删除、详情、列表、离线迁移 |
| `client.Transcode` | 视频上传、导入、转码、查询、下载 |

如果官方新增接口但 SDK 还没封装，可以临时使用底层请求：

```csharp
var data = await client.SendAsync<object>(
    HttpMethod.Get,
    "/api/v1/some/new/path",
    new Pan123RequestOptions { Query = new { fileID = 123 } });
```

## 错误处理

当 HTTP 状态码失败，或官方响应 `code != 0` 时，SDK 会抛出 `Pan123ApiException`。

```csharp
try
{
    await client.User.GetInfoAsync();
}
catch (Pan123ApiException ex)
{
    Console.WriteLine($"Code: {ex.Code}");
    Console.WriteLine($"TraceId: {ex.TraceId}");
    Console.WriteLine($"HTTP: {ex.StatusCode}");
    Console.WriteLine(ex.Message);
}
```

## 本地开发与测试

```bash
cd sdk-dotnet
dotnet restore
dotnet build Chest123.PanSdk.sln -c Release
dotnet test Chest123.PanSdk.sln -c Release
dotnet pack src/Chest123.PanSdk/Chest123.PanSdk.csproj -c Release
```

Live test 默认在缺少环境变量时直接跳过真实网络流程。设置凭证后会获取 token、读取用户信息、列目录、上传根目录 `Test.txt`、获取下载链接并真实下载后做字节比对。

PowerShell：

```powershell
$env:PAN123_CLIENT_ID="your-client-id"
$env:PAN123_CLIENT_SECRET="your-client-secret"
$env:PAN123_PARENT_FILE_ID="0"
dotnet test .\tests\Chest123.PanSdk.Tests\Chest123.PanSdk.Tests.csproj -c Release --filter Live
```

## NuGet 发布

发布前先构建和打包：

```bash
dotnet test Chest123.PanSdk.sln -c Release
dotnet pack src/Chest123.PanSdk/Chest123.PanSdk.csproj -c Release
```

发布：

```bash
dotnet nuget push src/Chest123.PanSdk/bin/Release/Chest123.PanSdk.0.1.0.nupkg \
  --api-key YOUR_NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

`bin/`、`obj/` 和 `.nupkg` 是构建产物，默认不提交到 git。
