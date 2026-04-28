using System.Net;

namespace Chest123.PanSdk;

/// <summary>Exception thrown for HTTP failures or 123Pan responses where code is not 0.</summary>
public sealed class Pan123ApiException : Exception
{
    public int? Code { get; }
    public string? TraceId { get; }
    public HttpStatusCode? StatusCode { get; }
    public string? ResponseBody { get; }

    public Pan123ApiException(string message, int? code = null, string? traceId = null, HttpStatusCode? statusCode = null, string? responseBody = null, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        TraceId = traceId;
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
