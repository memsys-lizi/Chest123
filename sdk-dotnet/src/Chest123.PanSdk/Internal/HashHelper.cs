using System.Security.Cryptography;

namespace Chest123.PanSdk.Internal;

internal static class HashHelper
{
    internal static async Task<string> ComputeFileMd5Async(string filePath, CancellationToken cancellationToken)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
#if NETSTANDARD2_0
        var hash = md5.ComputeHash(stream);
        await Task.CompletedTask;
#else
        var hash = await md5.ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
#endif
        return ToHex(hash);
    }

    internal static string ComputeBufferMd5(byte[] buffer)
    {
        using var md5 = MD5.Create();
        return ToHex(md5.ComputeHash(buffer));
    }

    private static string ToHex(byte[] bytes)
    {
        var chars = new char[bytes.Length * 2];
        const string alphabet = "0123456789abcdef";
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i * 2] = alphabet[bytes[i] >> 4];
            chars[i * 2 + 1] = alphabet[bytes[i] & 0xF];
        }
        return new string(chars);
    }
}
