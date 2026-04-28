using System.Text.Json.Serialization;

namespace Chest123.PanSdk.Models;

public sealed class AccessTokenData
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expiredAt")]
    public DateTimeOffset ExpiredAt { get; set; }
}

public sealed class OAuthTokenRequest
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string ClientSecret { get; set; } = string.Empty;

    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("redirect_uri")]
    public string? RedirectUri { get; set; }
}

public sealed class OAuthTokenData
{
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}

public sealed class FileListRequest
{
    [JsonPropertyName("parentFileId")]
    public long ParentFileId { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("searchData")]
    public string? SearchData { get; set; }

    [JsonPropertyName("searchMode")]
    public int? SearchMode { get; set; }

    [JsonPropertyName("lastFileId")]
    public long? LastFileId { get; set; }
}

public sealed class FileListData
{
    [JsonPropertyName("lastFileId")]
    public long LastFileId { get; set; }

    [JsonPropertyName("fileList")]
    public List<FileInfo> FileList { get; set; } = new();
}

public sealed class FileInfo
{
    [JsonPropertyName("fileID")]
    public long? FileID { get; set; }

    [JsonPropertyName("fileId")]
    public long? FileId { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("etag")]
    public string? Etag { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("parentFileID")]
    public long? ParentFileID { get; set; }

    [JsonPropertyName("parentFileId")]
    public long? ParentFileId { get; set; }

    [JsonPropertyName("trashed")]
    public int? Trashed { get; set; }
}

public sealed class DownloadInfoRequest
{
    [JsonPropertyName("fileId")]
    public long FileId { get; set; }
}

public sealed class DownloadInfoData
{
    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = string.Empty;
}

public sealed class UploadCreateRequest
{
    [JsonPropertyName("parentFileID")]
    public long ParentFileID { get; set; }

    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("etag")]
    public string Etag { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("duplicate")]
    public int? Duplicate { get; set; }

    [JsonPropertyName("containDir")]
    public bool? ContainDir { get; set; }
}

public sealed class UploadCreateData
{
    [JsonPropertyName("fileID")]
    public long? FileID { get; set; }

    [JsonPropertyName("reuse")]
    public bool Reuse { get; set; }

    [JsonPropertyName("preuploadID")]
    public string? PreuploadID { get; set; }

    [JsonPropertyName("sliceSize")]
    public int? SliceSize { get; set; }

    [JsonPropertyName("servers")]
    public List<string>? Servers { get; set; }
}

public sealed class UploadCompleteData
{
    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    [JsonPropertyName("fileID")]
    public long FileID { get; set; }

    /// <summary>
    /// When single-upload is still processing server-side, poll <c>/upload/v2/file/upload_complete</c> with this id.
    /// </summary>
    [JsonPropertyName("preuploadID")]
    public string? PreuploadID { get; set; }
}

public sealed class UploadFileRequest
{
    public string FilePath { get; set; } = string.Empty;
    public long ParentFileID { get; set; }
    public string? Filename { get; set; }
    public int? Duplicate { get; set; }
    public bool? ContainDir { get; set; }
    public long SingleUploadMaxBytes { get; set; } = 1024L * 1024 * 1024;

    /// <summary>
    /// Max polling attempts when <c>/upload/v2/file/upload_complete</c> returns <c>completed: false</c>. Default 300 (~5 minutes at 1s interval).
    /// </summary>
    public int UploadCompleteMaxAttempts { get; set; } = 300;

    /// <summary>
    /// Delay between polls. Default 1 second.
    /// </summary>
    public TimeSpan UploadCompletePollInterval { get; set; } = TimeSpan.FromSeconds(1);
}

public sealed class UploadFileResult
{
    public long FileID { get; set; }
    public bool Completed { get; set; }
    public bool Reuse { get; set; }
}

public sealed class DirectLinkUrlRequest
{
    [JsonPropertyName("fileID")]
    public long FileID { get; set; }
}

public sealed class DirectLinkUrlData
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
