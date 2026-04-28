using System.Text.Json;
using Chest123.PanSdk.Internal;
using Chest123.PanSdk.Models;

namespace Chest123.PanSdk.Modules;

public sealed class TranscodeModule : ApiModule
{
    internal TranscodeModule(Pan123HttpClient http) : base(http) { }
    public Task<FileListData?> ListCloudDiskVideosAsync(FileListRequest request, CancellationToken cancellationToken = default) => Http.SendAsync<FileListData>(HttpMethod.Get, "/api/v2/file/list", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> UploadFromCloudDiskAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/upload/from_cloud_disk", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<FileListData?> ListFilesAsync(FileListRequest request, CancellationToken cancellationToken = default) => Http.SendAsync<FileListData>(HttpMethod.Get, "/api/v2/file/list", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> FolderInfoAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/folder/info", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> VideoResolutionsAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/video/resolutions", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> ListAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/video/transcode/list", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> TranscodeAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/video", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> RecordAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/video/record", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> ResultAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/video/result", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> DeleteAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/delete", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> DownloadOriginalAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/file/download", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> DownloadM3u8OrTsAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/m3u8_ts/download", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> DownloadAllAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/transcode/file/download/all", new Pan123RequestOptions { Body = request }, cancellationToken);
}

