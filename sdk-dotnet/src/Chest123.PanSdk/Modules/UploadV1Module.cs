using System.Text.Json;
using Chest123.PanSdk.Internal;

namespace Chest123.PanSdk.Modules;

public sealed class UploadV1Module : ApiModule
{
    internal UploadV1Module(Pan123HttpClient http) : base(http) { }

    public Task<JsonElement> CreateAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/file/create", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> GetUploadUrlAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/file/get_upload_url", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> ListUploadPartsAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/file/list_upload_parts", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> CompleteAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/file/upload_complete", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> AsyncResultAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v1/file/upload_async_result", new Pan123RequestOptions { Body = request }, cancellationToken);
}

