using Chest123.PanSdk.Internal;
using Chest123.PanSdk.Models;

namespace Chest123.PanSdk.Modules;

public sealed class AuthModule : ApiModule
{
    internal AuthModule(Pan123HttpClient http) : base(http) { }

    public Task<AccessTokenData> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        => Http.GetAccessTokenAsync(cancellationToken);

    public Task<OAuthTokenData?> GetOAuthTokenAsync(OAuthTokenRequest request, CancellationToken cancellationToken = default)
        => Http.GetOAuthTokenAsync(request, cancellationToken);

    public void SetAccessToken(string token, DateTimeOffset? expiresAt = null)
        => Http.SetAccessToken(token, expiresAt);

    public Task<string> EnsureAccessTokenAsync(CancellationToken cancellationToken = default)
        => Http.EnsureAccessTokenAsync(cancellationToken);
}

