using Chest123.PanSdk.Models;

namespace Chest123.PanSdk.Tests;

public sealed class LiveTests
{
    [Fact]
    public async Task Live_UploadDownloadAndCompareBytesAsync()
    {
        var clientId = Environment.GetEnvironmentVariable("PAN123_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("PAN123_CLIENT_SECRET");
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            return;
        }

        var root = FindRepositoryRoot();
        var sourceFile = Path.Combine(root, "Test.txt");
        Assert.True(File.Exists(sourceFile), "Test.txt must exist in the repository root for live upload tests.");

        var parentFileIdText = Environment.GetEnvironmentVariable("PAN123_PARENT_FILE_ID");
        var parentFileId = long.TryParse(parentFileIdText, out var parsed) ? parsed : 0;
        var client = new Pan123Client(new Pan123ClientOptions
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Timeout = TimeSpan.FromSeconds(120)
        });

        var token = await client.Auth.EnsureAccessTokenAsync();
        Assert.False(string.IsNullOrWhiteSpace(token));

        await client.User.GetInfoAsync();
        await client.Files.ListAsync(new FileListRequest { ParentFileId = parentFileId, Limit = 100 });

        var upload = await client.Upload.UploadFileAsync(new UploadFileRequest
        {
            FilePath = sourceFile,
            ParentFileID = parentFileId,
            Filename = $"Test-sdk-live-dotnet-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.txt",
            Duplicate = 1
        });

        Assert.True(upload.FileID > 0);
        await client.Files.DetailAsync(new { fileID = upload.FileID });

        var download = await client.Files.GetDownloadInfoAsync(new DownloadInfoRequest { FileId = upload.FileID });
        Assert.NotNull(download);
        Assert.False(string.IsNullOrWhiteSpace(download.DownloadUrl));

        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        var downloadedBytes = await httpClient.GetByteArrayAsync(download.DownloadUrl);
        var sourceBytes = await File.ReadAllBytesAsync(sourceFile);
        Assert.Equal(sourceBytes, downloadedBytes);
    }

    private static string FindRepositoryRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "Test.txt")))
            {
                return current;
            }

            var parent = Directory.GetParent(current);
            if (parent is null) break;
            current = parent.FullName;
        }

        throw new DirectoryNotFoundException("Could not locate repository root containing Test.txt.");
    }
}
