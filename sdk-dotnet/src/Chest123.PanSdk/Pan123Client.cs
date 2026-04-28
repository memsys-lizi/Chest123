using Chest123.PanSdk.Internal;
using Chest123.PanSdk.Modules;

namespace Chest123.PanSdk;

/// <summary>Main entry point for the 123Pan SDK.</summary>
public sealed class Pan123Client
{
    private readonly Pan123HttpClient _http;

    public AuthModule Auth { get; }
    public FilesModule Files { get; }
    public UploadModule Upload { get; }
    public UploadV1Module UploadV1 { get; }
    public ShareModule Share { get; }
    public OfflineModule Offline { get; }
    public UserModule User { get; }
    public DirectLinkModule DirectLink { get; }
    public OssModule Oss { get; }
    public TranscodeModule Transcode { get; }

    public Pan123Client(Pan123ClientOptions options)
    {
        _http = new Pan123HttpClient(options);
        Auth = new AuthModule(_http);
        Files = new FilesModule(_http);
        Upload = new UploadModule(_http);
        UploadV1 = new UploadV1Module(_http);
        Share = new ShareModule(_http);
        Offline = new OfflineModule(_http);
        User = new UserModule(_http);
        DirectLink = new DirectLinkModule(_http);
        Oss = new OssModule(_http);
        Transcode = new TranscodeModule(_http);
    }

    public Task<T?> SendAsync<T>(HttpMethod method, string path, Pan123RequestOptions? options = null, CancellationToken cancellationToken = default)
        => _http.SendAsync<T>(method, path, options ?? new Pan123RequestOptions(), cancellationToken);
}
