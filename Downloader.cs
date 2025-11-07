using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public static class Downloader
{
    private static readonly HttpClient _httpClient = new();

    public static async Task<string> DownloadFileAsync(string url, string fileName)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "pie_setup");
        Directory.CreateDirectory(tempDir);

        string destination = Path.Combine(tempDir, fileName);

        string? fileId = ExtractGoogleDriveFileId(url);
        
        if (fileId != null)
        {
            await DownloadFromGoogleDriveAsync(fileId, destination);
        }
        else
        {
            await DownloadDirectAsync(url, destination);
        }

        return destination;
    }

    private static async Task DownloadDirectAsync(string url, string destination)
    {
        using HttpResponseMessage response = await _httpClient.GetAsync(
            url, 
            HttpCompletionOption.ResponseHeadersRead
        );
        response.EnsureSuccessStatusCode();

        await using FileStream fileStream = new(
            destination, 
            FileMode.Create, 
            FileAccess.Write, 
            FileShare.None
        );
        await response.Content.CopyToAsync(fileStream);
    }

    private static string? ExtractGoogleDriveFileId(string url)
    {
        try
        {
            Uri uri = new(url);
            
            // Check query string for id parameter
            string query = uri.Query.TrimStart('?');
            var queryParams = query.Split('&')
                .Select(p => p.Split('='))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0], p => p[1]);

            if (queryParams.TryGetValue("id", out string? fileId))
                return fileId;

            // Check path segments for /d/{fileId}/ pattern
            string[] segments = uri.Segments;
            int dIndex = Array.IndexOf(segments, "d/");
            if (dIndex >= 0 && dIndex + 1 < segments.Length)
                return segments[dIndex + 1].TrimEnd('/');
        }
        catch
        {
            // Not a Google Drive URL
        }

        return null;
    }

    private static async Task DownloadFromGoogleDriveAsync(string fileId, string destination)
    {
        using HttpClient client = new();
        string url = $"https://drive.usercontent.google.com/download?id={fileId}&export=download&confirm=t";

        using HttpResponseMessage response = await client.GetAsync(
            url, 
            HttpCompletionOption.ResponseHeadersRead
        );
        response.EnsureSuccessStatusCode();

        await using FileStream fileStream = new(
            destination, 
            FileMode.Create, 
            FileAccess.Write
        );
        await response.Content.CopyToAsync(fileStream);
    }
}