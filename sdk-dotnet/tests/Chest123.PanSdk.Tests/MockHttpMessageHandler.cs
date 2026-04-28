using System.Net;
using System.Text;
using System.Text.Json;

namespace Chest123.PanSdk.Tests;

internal sealed class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<CapturedRequest, HttpResponseMessage> _responder;

    public MockHttpMessageHandler(Func<CapturedRequest, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public List<CapturedRequest> Requests { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        var captured = new CapturedRequest(
            request.Method,
            request.RequestUri ?? throw new InvalidOperationException("RequestUri is null."),
            request.Headers.ToDictionary(x => x.Key, x => x.Value.ToArray()),
            request.Content?.Headers.ToDictionary(x => x.Key, x => x.Value.ToArray()) ?? new Dictionary<string, string[]>(),
            body);

        Requests.Add(captured);
        return _responder(captured);
    }

    public static HttpResponseMessage Ok(object? data)
    {
        var payload = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["code"] = 0,
            ["message"] = "ok",
            ["data"] = data,
            ["x-traceID"] = "trace-test"
        });
        return Json(payload);
    }

    public static HttpResponseMessage ApiError(int code, string message)
    {
        var payload = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["code"] = code,
            ["message"] = message,
            ["data"] = null,
            ["x-traceID"] = "trace-error"
        });
        return Json(payload);
    }

    private static HttpResponseMessage Json(string payload)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
    }
}

internal sealed record CapturedRequest(
    HttpMethod Method,
    Uri Uri,
    IReadOnlyDictionary<string, string[]> Headers,
    IReadOnlyDictionary<string, string[]> ContentHeaders,
    string? Body);
