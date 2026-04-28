using Chest123.PanSdk.Models;

namespace Chest123.PanSdk.Tests;

public sealed class Pan123ClientTests
{
    [Fact]
    public async Task EnsureAccessTokenAsync_CachesTokenAndSendsCommonHeaders()
    {
        var tokenCalls = 0;
        var handler = new MockHttpMessageHandler(request =>
        {
            if (request.Uri.AbsolutePath == "/api/v1/access_token")
            {
                tokenCalls++;
                return MockHttpMessageHandler.Ok(new { accessToken = "token-1", expiredAt = DateTimeOffset.UtcNow.AddHours(1) });
            }

            Assert.Equal("/api/v1/user/info", request.Uri.AbsolutePath);
            Assert.Equal("open_platform", request.Headers["Platform"].Single());
            Assert.Equal("Bearer token-1", request.Headers["Authorization"].Single());
            return MockHttpMessageHandler.Ok(new { uid = 123 });
        });

        var client = CreateClient(handler);

        await client.User.GetInfoAsync();
        await client.User.GetInfoAsync();

        Assert.Equal(1, tokenCalls);
        Assert.Equal(3, handler.Requests.Count);
    }

    [Fact]
    public async Task SendAsync_RefreshesExpiredToken()
    {
        var tokenCalls = 0;
        var handler = new MockHttpMessageHandler(request =>
        {
            if (request.Uri.AbsolutePath == "/api/v1/access_token")
            {
                tokenCalls++;
                return MockHttpMessageHandler.Ok(new { accessToken = $"fresh-{tokenCalls}", expiredAt = DateTimeOffset.UtcNow.AddHours(1) });
            }

            Assert.Equal("Bearer fresh-1", request.Headers["Authorization"].Single());
            return MockHttpMessageHandler.Ok(new { uid = 123 });
        });

        var client = new Pan123Client(new Pan123ClientOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            AccessToken = "expired",
            TokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            HttpClient = new HttpClient(handler)
        });

        await client.User.GetInfoAsync();

        Assert.Equal(1, tokenCalls);
    }

    [Fact]
    public async Task FilesListAsync_SerializesGetQuery()
    {
        var handler = new MockHttpMessageHandler(request =>
        {
            if (request.Uri.AbsolutePath == "/api/v1/access_token")
            {
                return MockHttpMessageHandler.Ok(new { accessToken = "token", expiredAt = DateTimeOffset.UtcNow.AddHours(1) });
            }

            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("/api/v2/file/list", request.Uri.AbsolutePath);
            Assert.Contains("parentFileId=0", request.Uri.Query);
            Assert.Contains("limit=100", request.Uri.Query);
            Assert.Contains("lastFileId=42", request.Uri.Query);
            return MockHttpMessageHandler.Ok(new { lastFileId = 42, fileList = Array.Empty<object>() });
        });

        var client = CreateClient(handler);
        var result = await client.Files.ListAsync(new FileListRequest { ParentFileId = 0, Limit = 100, LastFileId = 42 });

        Assert.NotNull(result);
        Assert.Equal(42, result.LastFileId);
    }

    [Fact]
    public async Task SendAsync_WhenApiCodeIsNonZero_ThrowsPan123ApiException()
    {
        var handler = new MockHttpMessageHandler(_ => MockHttpMessageHandler.ApiError(401, "bad token"));
        var client = new Pan123Client(new Pan123ClientOptions
        {
            AccessToken = "token",
            HttpClient = new HttpClient(handler)
        });

        var exception = await Assert.ThrowsAsync<Pan123ApiException>(() => client.User.GetInfoAsync());

        Assert.Equal(401, exception.Code);
        Assert.Contains("bad token", exception.Message);
        Assert.Contains("\"code\":401", exception.ResponseBody);
    }

    [Fact]
    public async Task UploadFileAsync_UsesV2SingleUploadForSmallFiles()
    {
        var filePath = CreateTempFile("hello single upload");
        var handler = new MockHttpMessageHandler(request =>
        {
            return request.Uri.AbsolutePath switch
            {
                "/api/v1/access_token" => MockHttpMessageHandler.Ok(new { accessToken = "token", expiredAt = DateTimeOffset.UtcNow.AddHours(1) }),
                "/upload/v2/file/domain" => MockHttpMessageHandler.Ok(new[] { "https://upload.example.test" }),
                "/upload/v2/file/single/create" => AssertSingleUpload(request),
                _ => throw new InvalidOperationException("Unexpected request: " + request.Uri)
            };
        });

        var client = CreateClient(handler);
        var result = await client.Upload.UploadFileAsync(new UploadFileRequest
        {
            FilePath = filePath,
            ParentFileID = 0,
            Filename = "single.txt",
            Duplicate = 1
        });

        Assert.True(result.Completed);
        Assert.Equal(987, result.FileID);
    }

    [Fact]
    public async Task UploadFileAsync_UsesSlicesForLargeFiles()
    {
        var filePath = CreateTempFile("abcde");
        var sliceCalls = 0;
        var handler = new MockHttpMessageHandler(request =>
        {
            return request.Uri.AbsolutePath switch
            {
                "/api/v1/access_token" => MockHttpMessageHandler.Ok(new { accessToken = "token", expiredAt = DateTimeOffset.UtcNow.AddHours(1) }),
                "/upload/v2/file/create" => MockHttpMessageHandler.Ok(new
                {
                    reuse = false,
                    preuploadID = "pre-1",
                    sliceSize = 2,
                    servers = new[] { "https://upload.example.test" }
                }),
                "/upload/v2/file/slice" => AssertSliceUpload(request, ++sliceCalls),
                "/upload/v2/file/upload_complete" => MockHttpMessageHandler.Ok(new { completed = true, fileID = 456 }),
                _ => throw new InvalidOperationException("Unexpected request: " + request.Uri)
            };
        });

        var client = CreateClient(handler);
        var result = await client.Upload.UploadFileAsync(new UploadFileRequest
        {
            FilePath = filePath,
            ParentFileID = 0,
            Filename = "large.txt",
            SingleUploadMaxBytes = 1
        });

        Assert.Equal(3, sliceCalls);
        Assert.True(result.Completed);
        Assert.Equal(456, result.FileID);
    }

    private static Pan123Client CreateClient(MockHttpMessageHandler handler)
    {
        return new Pan123Client(new Pan123ClientOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            HttpClient = new HttpClient(handler)
        });
    }

    private static string CreateTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"pan123-sdk-test-{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, content);
        return path;
    }

    private static HttpResponseMessage AssertSingleUpload(CapturedRequest request)
    {
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Contains("multipart/form-data", request.ContentHeaders["Content-Type"].Single());
        Assert.Contains("filename", request.Body);
        Assert.Contains("single.txt", request.Body);
        Assert.Contains("parentFileID", request.Body);
        return MockHttpMessageHandler.Ok(new { completed = true, fileID = 987 });
    }

    private static HttpResponseMessage AssertSliceUpload(CapturedRequest request, int sliceNo)
    {
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Contains("preuploadID", request.Body);
        Assert.Contains("pre-1", request.Body);
        Assert.Contains("sliceNo", request.Body);
        Assert.Contains(sliceNo.ToString(), request.Body);
        Assert.Contains("sliceMD5", request.Body);
        return MockHttpMessageHandler.Ok(new { uploaded = true });
    }
}
