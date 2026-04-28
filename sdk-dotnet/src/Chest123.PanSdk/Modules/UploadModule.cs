using System.Text.Json;
using Chest123.PanSdk.Internal;
using Chest123.PanSdk.Models;

namespace Chest123.PanSdk.Modules;

public sealed class UploadModule : ApiModule
{
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

    private async Task<UploadCompleteData?> PollUploadCompleteAsync(string preuploadID, UploadFileRequest request, CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < request.UploadCompleteMaxAttempts; attempt++)
        {
            var completed = await CompleteAsync(preuploadID, cancellationToken).ConfigureAwait(false);
            if (completed is not null && completed.Completed && completed.FileID > 0)
            {
                return completed;
            }
            await Task.Delay(request.UploadCompletePollInterval, cancellationToken).ConfigureAwait(false);
        }
        var approxSeconds = (int)(request.UploadCompleteMaxAttempts * request.UploadCompletePollInterval.TotalSeconds);
        throw new Pan123ApiException(
            $"Upload completion did not finish after {request.UploadCompleteMaxAttempts} polling attempts (~{approxSeconds}s). Increase UploadCompleteMaxAttempts or UploadCompletePollInterval if the server is slow.");
    }

    public async Task<UploadFileResult> UploadFileAsync(UploadFileRequest request, CancellationToken cancellationToken = default)
    {
        var fileInfo = new System.IO.FileInfo(request.FilePath);
        if (!fileInfo.Exists) throw new FileNotFoundException("Upload file does not exist.", request.FilePath);
        var filename = request.Filename ?? fileInfo.Name;
        var etag = await HashHelper.ComputeFileMd5Async(request.FilePath, cancellationToken).ConfigureAwait(false);
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
            var result = await SingleAsync(domains[0], request.FilePath, uploadRequest, cancellationToken).ConfigureAwait(false);
            if (result is null) throw new Pan123ApiException("Single upload returned no data.");
            if (result.Completed && result.FileID > 0)
            {
                return new UploadFileResult { FileID = result.FileID, Completed = true };
            }
            if (!string.IsNullOrWhiteSpace(result.PreuploadID))
            {
                var done = await PollUploadCompleteAsync(result.PreuploadID!, request, cancellationToken).ConfigureAwait(false);
                if (done is null) throw new Pan123ApiException("Upload completion polling returned no data.");
                return new UploadFileResult { FileID = done.FileID, Completed = true };
            }
            throw new Pan123ApiException(
                "Single upload did not return fileID and completed=true, and no preuploadID was returned for polling upload_complete. Check API response or reduce SingleUploadMaxBytes to use multipart upload.");
        }

        var created = await CreateAsync(uploadRequest, cancellationToken).ConfigureAwait(false);
        if (created is null) throw new Pan123ApiException("Upload create returned no data.");
        if (created.Reuse)
        {
            return new UploadFileResult { FileID = created.FileID ?? 0, Completed = true, Reuse = true };
        }
        if (string.IsNullOrWhiteSpace(created.PreuploadID) || !created.SliceSize.HasValue || created.Servers is null || created.Servers.Count == 0)
        {
            throw new Pan123ApiException("Upload create did not return preuploadID, sliceSize, or servers.");
        }

        var preuploadID = created.PreuploadID!;
        var buffer = new byte[created.SliceSize.Value];
        using var stream = File.OpenRead(request.FilePath);
        var sliceNo = 1;
        int read;
        while ((read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
        {
            var sliceBytes = read == buffer.Length ? buffer : buffer.Take(read).ToArray();
            var md5 = HashHelper.ComputeBufferMd5(sliceBytes);
            using var sliceStream = new MemoryStream(sliceBytes, writable: false);
            await SliceAsync(created.Servers[0], preuploadID, sliceNo, md5, sliceStream, $"{filename}.part{sliceNo}", cancellationToken).ConfigureAwait(false);
            sliceNo++;
        }

        var multipartDone = await PollUploadCompleteAsync(preuploadID, request, cancellationToken).ConfigureAwait(false);
        if (multipartDone is null) throw new Pan123ApiException("Upload completion polling returned no data.");
        return new UploadFileResult { FileID = multipartDone.FileID, Completed = true };
    }
}

