using System.Text.Json.Serialization;

namespace Chest123.PanSdk;

public sealed class Pan123Response<T>
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("x-traceID")]
    public string? TraceId { get; set; }
}
