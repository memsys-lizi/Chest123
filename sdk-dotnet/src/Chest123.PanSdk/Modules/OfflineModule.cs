using System.Text.Json;
using Chest123.PanSdk.Internal;

namespace Chest123.PanSdk.Modules;

public sealed class OfflineModule : ApiModule
{
    internal OfflineModule(Pan123HttpClient http) : base(http) { }
    public Task<JsonElement> CreateDownloadTaskAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/offline/download", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> GetDownloadProcessAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/offline/download/process", new Pan123RequestOptions { Query = request }, cancellationToken);
}

