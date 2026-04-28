namespace Chest123.PanSdk;

public sealed class Pan123RequestOptions
{
    public object? Query { get; set; }
    public object? Body { get; set; }
    public MultipartFormDataContent? Multipart { get; set; }
    public bool Auth { get; set; } = true;
    public string? BaseUrl { get; set; }
    public Dictionary<string, string> Headers { get; } = new();
}
