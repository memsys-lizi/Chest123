using System.Text.Json;
using Chest123.PanSdk.Internal;
using Chest123.PanSdk.Models;

namespace Chest123.PanSdk.Modules;

public sealed class FilesModule : ApiModule
{
    internal FilesModule(Pan123HttpClient http) : base(http) { }

    public Task<JsonElement> MkdirAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/file/mkdir", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> RenameAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Put, "/api/v1/file/name", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> BatchRenameAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/file/rename", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> TrashAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/file/trash", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> CopyAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/file/copy", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> AsyncCopyAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/file/async/copy", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> AsyncCopyProcessAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/file/async/copy/process", new Pan123RequestOptions { Query = request }, cancellationToken);

    public Task<JsonElement> RecoverAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/file/recover", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> RecoverByPathAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/file/recover/by_path", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> DetailAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/file/detail", new Pan123RequestOptions { Query = request }, cancellationToken);

    public Task<JsonElement> InfosAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/file/infos", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<FileListData?> ListAsync(FileListRequest request, CancellationToken cancellationToken = default)
        => Http.SendAsync<FileListData>(HttpMethod.Get, "/api/v2/file/list", new Pan123RequestOptions { Query = request }, cancellationToken);

    public Task<JsonElement> ListLegacyAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/file/list", new Pan123RequestOptions { Query = request }, cancellationToken);

    public Task<JsonElement> MoveAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/file/move", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<DownloadInfoData?> GetDownloadInfoAsync(DownloadInfoRequest request, CancellationToken cancellationToken = default)
        => Http.SendAsync<DownloadInfoData>(HttpMethod.Get, "/api/v1/file/download_info", new Pan123RequestOptions { Query = request }, cancellationToken);
}

