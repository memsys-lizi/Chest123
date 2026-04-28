using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Chest123.PanSdk.Models;

namespace Chest123.PanSdk.Internal;

public sealed class Pan123HttpClient
{
    private readonly Pan123ClientOptions _options;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset? _tokenExpiresAt;

    public Pan123HttpClient(Pan123ClientOptions options)
    {
        _options = options;
        _accessToken = options.AccessToken;
        _tokenExpiresAt = options.TokenExpiresAt;
        _httpClient = options.HttpClient ?? new HttpClient();
        _httpClient.Timeout = options.Timeout;
    }

    public void SetAccessToken(string token, DateTimeOffset? expiresAt = null)
    {
        _accessToken = token;
        _tokenExpiresAt = expiresAt;
    }

    public async Task<string> EnsureAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && (!_tokenExpiresAt.HasValue || DateTimeOffset.UtcNow < _tokenExpiresAt.Value.AddMinutes(-1)))
        {
            return _accessToken!;
        }

        await _tokenLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) && (!_tokenExpiresAt.HasValue || DateTimeOffset.UtcNow < _tokenExpiresAt.Value.AddMinutes(-1)))
            {
                return _accessToken!;
            }

            var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
            return token.AccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public async Task<AccessTokenData> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            throw new Pan123ApiException("ClientId and ClientSecret are required to fetch access_token.");
        }

        var data = await SendAsync<AccessTokenData>(HttpMethod.Post, "/api/v1/access_token", new Pan123RequestOptions
        {
            Auth = false,
            Body = new
            {
                clientID = _options.ClientId,
                clientSecret = _options.ClientSecret
            }
        }, cancellationToken).ConfigureAwait(false);

        if (data is null || string.IsNullOrWhiteSpace(data.AccessToken))
        {
            throw new Pan123ApiException("The access_token response did not contain accessToken.");
        }

        _accessToken = data.AccessToken;
        _tokenExpiresAt = data.ExpiredAt;
        return data;
    }

    public Task<OAuthTokenData?> GetOAuthTokenAsync(OAuthTokenRequest request, CancellationToken cancellationToken = default)
    {
        return SendAsync<OAuthTokenData>(HttpMethod.Post, "/api/v1/oauth2/access_token", new Pan123RequestOptions
        {
            Auth = false,
            Query = request
        }, cancellationToken);
    }

    public async Task<T?> SendAsync<T>(HttpMethod method, string path, Pan123RequestOptions options, CancellationToken cancellationToken = default)
    {
        var uri = BuildUri(options.BaseUrl ?? _options.BaseUrl, path, options.Query);
        using var request = new HttpRequestMessage(method, uri);
        request.Headers.TryAddWithoutValidation("Platform", _options.Platform);

        foreach (var header in options.Headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (options.Auth)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await EnsureAccessTokenAsync(cancellationToken).ConfigureAwait(false));
        }

        if (options.Multipart is not null)
        {
            request.Content = options.Multipart;
        }
        else if (options.Body is not null)
        {
            var json = JsonSerializer.Serialize(options.Body, JsonHelper.Options);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new Pan123ApiException($"HTTP request failed with status {(int)response.StatusCode}.", statusCode: response.StatusCode, responseBody: responseBody);
        }

        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return default;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("code", out _) && root.TryGetProperty("message", out _))
            {
                var wrapped = JsonSerializer.Deserialize<Pan123Response<T>>(responseBody, JsonHelper.Options);
                if (wrapped is null) return default;
                if (wrapped.Code != 0)
                {
                    throw new Pan123ApiException(wrapped.Message, wrapped.Code, wrapped.TraceId, response.StatusCode, responseBody);
                }
                return wrapped.Data;
            }

            return JsonSerializer.Deserialize<T>(responseBody, JsonHelper.Options);
        }
        catch (JsonException ex)
        {
            throw new Pan123ApiException("Failed to parse API response JSON.", statusCode: response.StatusCode, responseBody: responseBody, innerException: ex);
        }
    }

    private static Uri BuildUri(string baseUrl, string path, object? query)
    {
        var root = baseUrl.TrimEnd('/');
        var relative = path.StartsWith("/", StringComparison.Ordinal) ? path : "/" + path;
        return new Uri(root + relative + JsonHelper.ToQueryString(query));
    }
}
