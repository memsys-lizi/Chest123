using Chest123.PanSdk;
using Chest123.PanSdk.Models;

var clientId = Environment.GetEnvironmentVariable("PAN123_CLIENT_ID");
var clientSecret = Environment.GetEnvironmentVariable("PAN123_CLIENT_SECRET");

if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
{
    Console.WriteLine("Please set PAN123_CLIENT_ID and PAN123_CLIENT_SECRET first.");
    return;
}

var client = new Pan123Client(new Pan123ClientOptions
{
    ClientId = clientId,
    ClientSecret = clientSecret
});

var user = await client.User.GetInfoAsync();
Console.WriteLine("User info:");
Console.WriteLine(user);

var files = await client.Files.ListAsync(new FileListRequest
{
    ParentFileId = 0,
    Limit = 100
});

Console.WriteLine($"Root file count: {files?.FileList.Count ?? 0}");
