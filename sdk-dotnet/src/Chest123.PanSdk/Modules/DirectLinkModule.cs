using System.Text.Json;
using Chest123.PanSdk.Internal;
using Chest123.PanSdk.Models;

namespace Chest123.PanSdk.Modules;

public sealed class DirectLinkModule : ApiModule
{
    internal DirectLinkModule(Pan123HttpClient http) : base(http) { }
    public Task<JsonElement> EnableAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/direct-link/enable", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> DisableAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/direct-link/disable", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<DirectLinkUrlData?> GetUrlAsync(DirectLinkUrlRequest request, CancellationToken cancellationToken = default) => Http.SendAsync<DirectLinkUrlData>(HttpMethod.Get, "/api/v1/direct-link/url", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> RefreshCacheAsync(CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/direct-link/cache/refresh", new Pan123RequestOptions(), cancellationToken);
    public Task<JsonElement> GetTrafficLogsAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/direct-link/log", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> GetOfflineLogsAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/direct-link/offline/logs", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> SetIpBlacklistEnabledAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/developer/config/forbide-ip/switch", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> UpdateIpBlacklistAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/developer/config/forbide-ip/update", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> ListIpBlacklistAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/developer/config/forbide-ip/list", new Pan123RequestOptions { Query = request }, cancellationToken);
}

