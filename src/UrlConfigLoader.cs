using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class DownloadUrls
{
    public string PatchedBinariesUrl { get; set; } = string.Empty;
    public string UbaHostUrl { get; set; } = string.Empty;
    public string NeoniteUrl { get; set; } = string.Empty;
}

public static class UrlConfigLoader
{
    private static readonly HttpClient _httpClient = new();

    public static async Task<DownloadUrls> LoadUrlsAsync(string configUrl)
    {
        try
        {
            string content = await _httpClient.GetStringAsync(configUrl);
            return ParseUrlConfig(content);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load URL configuration: {ex.Message}", ex);
        }
    }

    private static DownloadUrls ParseUrlConfig(string content)
    {
        var urls = new DownloadUrls();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"));

        foreach (string line in lines)
        {
            if (!line.Contains('='))
                continue;

            string[] parts = line.Split('=', 2);
            if (parts.Length != 2)
                continue;

            string key = parts[0].Trim().ToLowerInvariant();
            string value = parts[1].Trim();

            switch (key)
            {
                case "patched_binaries":
                case "binaries":
                    urls.PatchedBinariesUrl = value;
                    break;
                case "ubahost":
                case "uba_host":
                    urls.UbaHostUrl = value;
                    break;
                case "neonite":
                    urls.NeoniteUrl = value;
                    break;
            }
        }

        ValidateUrls(urls);
        return urls;
    }

    private static void ValidateUrls(DownloadUrls urls)
    {
        List<string> missing = new();

        if (string.IsNullOrWhiteSpace(urls.PatchedBinariesUrl))
            missing.Add("patched_binaries");
        if (string.IsNullOrWhiteSpace(urls.UbaHostUrl))
            missing.Add("ubahost");
        if (string.IsNullOrWhiteSpace(urls.NeoniteUrl))
            missing.Add("neonite");

        if (missing.Any())
        {
            throw new Exception($"Missing required URLs in config: {string.Join(", ", missing)}");
        }
    }
}