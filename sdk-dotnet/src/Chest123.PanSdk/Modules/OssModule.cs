using System.Text.Json;
using Chest123.PanSdk.Internal;

namespace Chest123.PanSdk.Modules;

public sealed class OssModule : ApiModule
{
    internal OssModule(Pan123HttpClient http) : base(http) { }
    public Task<JsonElement> MkdirAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/oss/file/mkdir", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> CreateAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/oss/file/create", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> GetUploadUrlAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/oss/file/get_upload_url", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> CompleteAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/oss/file/upload_complete", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> AsyncResultAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/oss/file/upload_async_result", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> CreateCopyTaskAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/oss/source/copy", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> GetCopyProcessAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/oss/source/copy/process", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> GetCopyFailListAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/oss/source/copy/fail", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> MoveAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/oss/file/move", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> DeleteAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/oss/file/delete", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> DetailAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/oss/file/detail", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> ListAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/oss/file/list", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> CreateOfflineMigrationAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/oss/offline/download", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> GetOfflineMigrationAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/oss/offline/download/process", new Pan123RequestOptions { Query = request }, cancellationToken);
}

