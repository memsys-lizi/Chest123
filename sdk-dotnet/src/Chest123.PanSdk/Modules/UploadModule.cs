using System.Text.Json;
using Chest123.PanSdk.Internal;
using Chest123.PanSdk.Models;

namespace Chest123.PanSdk.Modules;

public sealed class UploadModule : ApiModule
{
    private const int FileCheckingCode = 20103;

    internal UploadModule(Pan123HttpClient http) : base(http) { }

    public Task<UploadCreateData?> CreateAsync(UploadCreateRequest request, CancellationToken cancellationToken = default)
        => Http.SendAsync<UploadCreateData>(HttpMethod.Post, "/upload/v2/file/create", new Pan123RequestOptions { Body = request }, cancellationToken);

    public Task<JsonElement> SliceAsync(string uploadUrl, string preuploadID, int sliceNo, string sliceMD5, Stream slice, string fileName, CancellationToken cancellationToken = default)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(preuploadID), "preuploadID" },
            { new StringContent(sliceNo.ToString(System.Globalization.CultureInfo.InvariantCulture)), "sliceNo" },
            { new StringContent(sliceMD5), "sliceMD5" }
        };
        content.Add(new StreamContent(slice), "slice", fileName);
        return Http.SendAsync<JsonElement>(HttpMethod.Post, "/upload/v2/file/slice", new Pan123RequestOptions
        {
            BaseUrl = uploadUrl,
            Multipart = content
        }, cancellationToken);
    }

    public Task<UploadCompleteData?> CompleteAsync(string preuploadID, CancellationToken cancellationToken = default)
        => Http.SendAsync<UploadCompleteData>(HttpMethod.Post, "/upload/v2/file/upload_complete", new Pan123RequestOptions { Body = new { preuploadID } }, cancellationToken);

    public Task<List<string>?> DomainAsync(CancellationToken cancellationToken = default)
        => Http.SendAsync<List<string>>(HttpMethod.Get, "/upload/v2/file/domain", new Pan123RequestOptions(), cancellationToken);

    public async Task<UploadCompleteData?> SingleAsync(string uploadUrl, string filePath, UploadCreateRequest request, CancellationToken cancellationToken = default)
    {
        var content = new MultipartFormDataContent
        {
            { new StreamContent(File.OpenRead(filePath)), "file", request.Filename },
            { new StringContent(request.ParentFileID.ToString(System.Globalization.CultureInfo.InvariantCulture)), "parentFileID" },
            { new StringContent(request.Filename), "filename" },
            { new StringContent(request.Etag), "etag" },
            { new StringContent(request.Size.ToString(System.Globalization.CultureInfo.InvariantCulture)), "size" }
        };
        if (request.Duplicate.HasValue) content.Add(new StringContent(request.Duplicate.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)), "duplicate");
        if (request.ContainDir.HasValue) content.Add(new StringContent(request.ContainDir.Value ? "true" : "false"), "containDir");

        return await Http.SendAsync<UploadCompleteData>(HttpMethod.Post, "/upload/v2/file/single/create", new Pan123RequestOptions
        {
            BaseUrl = uploadUrl,
            Multipart = content
        }, cancellationToken).ConfigureAwait(false);
    }

    public Task<UploadCreateData?> Sha1ReuseAsync(object request, CancellationToken cancellationToken = default)
        => Http.SendAsync<UploadCreateData>(HttpMethod.Post, "/upload/v2/file/sha1_reuse", new Pan123RequestOptions { Body = request }, cancellationToken);

    public async Task<UploadFileResult> UploadFileAsync(UploadFileRequest request, CancellationToken cancellationToken = default)
    {
        var fileInfo = new System.IO.FileInfo(request.FilePath);
        if (!fileInfo.Exists) throw new FileNotFoundException("Upload file does not exist.", request.FilePath);
        var filename = request.Filename ?? fileInfo.Name;
        await EmitProgressAsync(request, new UploadProgressEvent
        {
            Stage = "hashing",
            LoadedBytes = 0,
            TotalBytes = fileInfo.Length,
            Percent = 0
        }, cancellationToken).ConfigureAwait(false);
        var etag = await HashHelper.ComputeFileMd5Async(request.FilePath, cancellationToken).ConfigureAwait(false);
        await EmitProgressAsync(request, new UploadProgressEvent
        {
            Stage = "hashing",
            LoadedBytes = fileInfo.Length,
            TotalBytes = fileInfo.Length,
            Percent = 100
        }, cancellationToken).ConfigureAwait(false);
        var uploadRequest = new UploadCreateRequest
        {
            ParentFileID = request.ParentFileID,
            Filename = filename,
            Etag = etag,
            Size = fileInfo.Length,
            Duplicate = request.Duplicate,
            ContainDir = request.ContainDir
        };

        if (fileInfo.Length <= request.SingleUploadMaxBytes)
        {
            var domains = await DomainAsync(cancellationToken).ConfigureAwait(false);
            if (domains is null || domains.Count == 0) throw new Pan123ApiException("No upload domain returned.");
            var result = await UploadSingleUntilCompleteAsync(domains[0], request.FilePath, uploadRequest, request, cancellationToken).ConfigureAwait(false);
            await EmitProgressAsync(request, new UploadProgressEvent
            {
                Stage = "single",
                LoadedBytes = fileInfo.Length,
                TotalBytes = fileInfo.Length,
                Percent = 100
            }, cancellationToken).ConfigureAwait(false);
            return new UploadFileResult { FileID = result.FileID, Completed = true };
        }

        await EmitProgressAsync(request, new UploadProgressEvent
        {
            Stage = "create",
            LoadedBytes = 0,
            TotalBytes = fileInfo.Length,
            Percent = 0
        }, cancellationToken).ConfigureAwait(false);
        var created = await RetryTransientUploadAsync(() => CreateAsync(uploadRequest, cancellationToken), request, cancellationToken).ConfigureAwait(false);
        if (created is null) throw new Pan123ApiException("Upload create returned no data.");
        if (created.Reuse)
        {
            if (!created.FileID.HasValue || created.FileID.Value <= 0)
            {
                throw new Pan123ApiException("Upload create reported reuse but did not return a valid fileID.");
            }
            await EmitProgressAsync(request, new UploadProgressEvent
            {
                Stage = "reuse",
                LoadedBytes = fileInfo.Length,
                TotalBytes = fileInfo.Length,
                Percent = 100
            }, cancellationToken).ConfigureAwait(false);
            return new UploadFileResult { FileID = created.FileID.Value, Completed = true, Reuse = true };
        }
        if (string.IsNullOrWhiteSpace(created.PreuploadID) || !created.SliceSize.HasValue || created.Servers is null || created.Servers.Count == 0)
        {
            throw new Pan123ApiException("Upload create did not return preuploadID, sliceSize, or servers.");
        }

        var preuploadID = created.PreuploadID!;
        var buffer = new byte[created.SliceSize.Value];
        using var stream = File.OpenRead(request.FilePath);
        var sliceNo = 1;
        var totalSlices = (int)Math.Ceiling(fileInfo.Length / (double)created.SliceSize.Value);
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            var sliceBytes = read == buffer.Length ? buffer : buffer.Take(read).ToArray();
            var md5 = HashHelper.ComputeBufferMd5(sliceBytes);
            using var sliceStream = new MemoryStream(sliceBytes, writable: false);
            var currentSliceNo = sliceNo;
            await RetryTransientUploadAsync(
                () => SliceAsync(created.Servers[0], preuploadID, currentSliceNo, md5, sliceStream, $"{filename}.part{currentSliceNo}", cancellationToken),
                request,
                cancellationToken).ConfigureAwait(false);
            var loadedBytes = Math.Min(fileInfo.Length, (long)currentSliceNo * created.SliceSize.Value);
            await EmitProgressAsync(request, new UploadProgressEvent
            {
                Stage = "slice",
                LoadedBytes = loadedBytes,
                TotalBytes = fileInfo.Length,
                Percent = fileInfo.Length == 0 ? 100 : loadedBytes * 100d / fileInfo.Length,
                SliceNo = currentSliceNo,
                TotalSlices = totalSlices,
                CompletedSlices = currentSliceNo
            }, cancellationToken).ConfigureAwait(false);
            sliceNo++;
        }

        var completed = await WaitForUploadCompleteAsync(preuploadID, request, fileInfo.Length, cancellationToken).ConfigureAwait(false);
        return new UploadFileResult { FileID = completed.FileID, Completed = true };
    }

    public Task<UploadCompleteData> WaitCompleteAsync(string preuploadID, int attempts = 60, TimeSpan? delay = null, CancellationToken cancellationToken = default)
    {
        var request = new UploadFileRequest
        {
            CompletePollingAttempts = attempts,
            CompletePollingDelay = delay ?? TimeSpan.FromSeconds(1)
        };
        return WaitForUploadCompleteAsync(preuploadID, request, totalBytes: 0, cancellationToken);
    }

    private async Task<UploadCompleteData> UploadSingleUntilCompleteAsync(string uploadUrl, string filePath, UploadCreateRequest uploadRequest, UploadFileRequest request, CancellationToken cancellationToken)
    {
        var attempts = Positive(request.SingleUploadRetryAttempts, 5);
        UploadCompleteData? lastResult = null;
        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            var result = await RetryTransientUploadAsync(() => SingleAsync(uploadUrl, filePath, uploadRequest, cancellationToken), request, cancellationToken).ConfigureAwait(false);
            if (IsCompletedUpload(result))
            {
                return result!;
            }
            lastResult = result;
            if (attempt < attempts)
            {
                await Task.Delay(NonNegative(request.SingleUploadRetryDelay, TimeSpan.FromSeconds(1)), cancellationToken).ConfigureAwait(false);
            }
        }

        throw new Pan123ApiException("Single upload did not return a completed upload with a valid fileID.", responseBody: JsonSerializer.Serialize(lastResult, JsonHelper.Options));
    }

    private async Task<UploadCompleteData> WaitForUploadCompleteAsync(string preuploadID, UploadFileRequest request, long totalBytes, CancellationToken cancellationToken)
    {
        var attempts = Positive(request.CompletePollingAttempts, 60);
        var delay = NonNegative(request.CompletePollingDelay, TimeSpan.FromSeconds(1));
        UploadCompleteData? lastResult = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                var completed = await RetryTransientUploadAsync(() => CompleteAsync(preuploadID, cancellationToken), request, cancellationToken).ConfigureAwait(false);
                if (IsCompletedUpload(completed))
                {
                    await EmitProgressAsync(request, new UploadProgressEvent
                    {
                        Stage = "complete",
                        LoadedBytes = totalBytes,
                        TotalBytes = totalBytes,
                        Percent = 100,
                        Attempt = attempt
                    }, cancellationToken).ConfigureAwait(false);
                    return completed!;
                }
                lastResult = completed;
            }
            catch (Pan123ApiException ex) when (ex.Code == FileCheckingCode && attempt < attempts)
            {
                // The API documents this as a polling state: wait and call upload_complete again.
            }

            await EmitProgressAsync(request, new UploadProgressEvent
            {
                Stage = "complete",
                LoadedBytes = totalBytes,
                TotalBytes = totalBytes,
                Percent = 100,
                Attempt = attempt
            }, cancellationToken).ConfigureAwait(false);
            if (attempt < attempts)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        throw new Pan123ApiException($"Upload completion did not return completed=true with a valid fileID after {attempts} polling attempts.", responseBody: JsonSerializer.Serialize(lastResult, JsonHelper.Options));
    }

    private static bool IsCompletedUpload(UploadCompleteData? data)
        => data is not null && data.Completed && data.FileID > 0;

    private static int Positive(int value, int fallback)
        => Math.Max(1, value <= 0 ? fallback : value);

    private static TimeSpan NonNegative(TimeSpan value, TimeSpan fallback)
        => value < TimeSpan.Zero ? fallback : value;

    private static bool IsTransientUploadError(Pan123ApiException exception)
    {
        if ((int?)exception.StatusCode == 429 || exception.Code == 429) return true;
        return exception.Code == 1 &&
            (exception.Message.IndexOf("秒传队列", StringComparison.Ordinal) >= 0 ||
             exception.Message.IndexOf("削峰", StringComparison.Ordinal) >= 0 ||
             exception.Message.IndexOf("请慢一点", StringComparison.Ordinal) >= 0);
    }

    private static async Task<T?> RetryTransientUploadAsync<T>(Func<Task<T?>> task, UploadFileRequest request, CancellationToken cancellationToken)
    {
        var attempts = Positive(request.TransientRetryAttempts, 5);
        var delay = NonNegative(request.TransientRetryDelay, TimeSpan.FromSeconds(1));

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                return await task().ConfigureAwait(false);
            }
            catch (Pan123ApiException ex) when (IsTransientUploadError(ex) && attempt < attempts)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        return await task().ConfigureAwait(false);
    }

    private static async Task EmitProgressAsync(UploadFileRequest request, UploadProgressEvent progress, CancellationToken cancellationToken)
    {
        if (request.OnProgressAsync is not null)
        {
            await request.OnProgressAsync(progress, cancellationToken).ConfigureAwait(false);
        }
    }
}

