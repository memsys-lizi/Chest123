using System.Text.Json;
using Chest123.PanSdk.Internal;

namespace Chest123.PanSdk.Modules;

public sealed class ShareModule : ApiModule
{
    internal ShareModule(Pan123HttpClient http) : base(http) { }
    public Task<JsonElement> CreateAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/share/create", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> ListAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/share/list", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> UpdateAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Put, "/api/v1/share/list/info", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> CreatePaidAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Post, "/api/v1/share/content-payment/create", new Pan123RequestOptions { Body = request }, cancellationToken);
    public Task<JsonElement> ListPaidAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/share/payment/list", new Pan123RequestOptions { Query = request }, cancellationToken);
    public Task<JsonElement> UpdatePaidAsync(object request, CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Put, "/api/v1/share/list/payment/info", new Pan123RequestOptions { Body = request }, cancellationToken);
}

