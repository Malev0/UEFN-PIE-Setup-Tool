using System;
using System.IO;
using System.Text.Json;

public static class FortniteLocator
{
    private const string EXE_NAME = "UnrealEditorFortnite-Win64-Shipping.exe";

    public static string? FindFortniteExecutable()
    {
        string manifestsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Epic", "EpicGamesLauncher", "Data", "Manifests"
        );

        if (!Directory.Exists(manifestsPath))
            return null;

        foreach (string manifestFile in Directory.GetFiles(manifestsPath, "*.item"))
        {
            string? installPath = TryParseManifest(manifestFile);
            if (installPath != null)
                return installPath;
        }

        return null;
    }

    private static string? TryParseManifest(string manifestFile)
    {
        try
        {
            string json = File.ReadAllText(manifestFile);
            using JsonDocument doc = JsonDocument.Parse(json);

            if (!IsFortniteManifest(doc))
                return null;

            if (doc.RootElement.TryGetProperty("InstallLocation", out JsonElement installLocation))
            {
                string installPath = installLocation.GetString()!;
                string exePath = Path.Combine(
                    installPath, 
                    "FortniteGame", 
                    "Binaries", 
                    "Win64", 
                    EXE_NAME
                );

                return File.Exists(exePath) ? exePath : null;
            }
        }
        catch
        {
            // Ignore malformed manifests
        }

        return null;
    }

    private static bool IsFortniteManifest(JsonDocument doc)
    {
        return doc.RootElement.TryGetProperty("DisplayName", out JsonElement displayName) 
            && displayName.GetString()?.Contains("Fortnite", StringComparison.OrdinalIgnoreCase) == true;
    }

    public static string? FindInDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            return null;

        // Try direct exe path
        string directExe = Path.Combine(path, EXE_NAME);
        if (File.Exists(directExe))
            return directExe;

        // Try navigating up from common subdirectories
        string? hierarchicalSearch = SearchUpwardsFromPath(path);
        if (hierarchicalSearch != null)
            return hierarchicalSearch;

        // Last resort: recursive search
        return RecursiveSearch(path);
    }

    private static string? SearchUpwardsFromPath(string path)
    {
        string? currentPath = path;

        while (!string.IsNullOrEmpty(currentPath))
        {
            string folderName = Path.GetFileName(currentPath);

            string? result = folderName.ToLowerInvariant() switch
            {
                "win64" => CheckPath(currentPath, EXE_NAME),
                "binaries" => CheckPath(currentPath, "Win64", EXE_NAME),
                "fortnitegame" => CheckPath(currentPath, "Binaries", "Win64", EXE_NAME),
                _ => null
            };

            if (result != null)
                return result;

            currentPath = Path.GetDirectoryName(currentPath);
        }

        return null;
    }

    private static string? RecursiveSearch(string path)
    {
        try
        {
            foreach (string file in Directory.EnumerateFiles(path, EXE_NAME, SearchOption.AllDirectories))
            {
                return file;
            }
        }
        catch
        {
            // Ignore permission errors
        }

        return null;
    }

    private static string? CheckPath(string basePath, params string[] pathSegments)
    {
        string fullPath = Path.Combine(basePath, Path.Combine(pathSegments));
        return File.Exists(fullPath) ? fullPath : null;
    }
}