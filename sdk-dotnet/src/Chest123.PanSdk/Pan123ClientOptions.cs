namespace Chest123.PanSdk;

/// <summary>Options used to create a 123Pan SDK client.</summary>
public sealed class Pan123ClientOptions
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? AccessToken { get; set; }
    public DateTimeOffset? TokenExpiresAt { get; set; }
    public string BaseUrl { get; set; } = "https://open-api.123pan.com";
    public string Platform { get; set; } = "open_platform";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public HttpClient? HttpClient { get; set; }
}
