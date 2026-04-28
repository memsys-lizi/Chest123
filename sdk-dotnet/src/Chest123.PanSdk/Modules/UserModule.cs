using System.Text.Json;
using Chest123.PanSdk.Internal;

namespace Chest123.PanSdk.Modules;

public sealed class UserModule : ApiModule
{
    internal UserModule(Pan123HttpClient http) : base(http) { }
    public Task<JsonElement> GetInfoAsync(CancellationToken cancellationToken = default) => Http.SendAsync<JsonElement>(HttpMethod.Get, "/api/v1/user/info", new Pan123RequestOptions(), cancellationToken);
}

