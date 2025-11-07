using System;
using System.IO;
using System.Linq;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Archives.Zip;

public static class ArchiveExtractor
{
    public static void Extract7z(string archivePath, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        using SevenZipArchive archive = SevenZipArchive.Open(archivePath);
        
        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
        {
            ExtractEntry(entry, outputDirectory);
        }
    }

    public static void ExtractZip(string archivePath, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        using ZipArchive archive = ZipArchive.Open(archivePath);
        
        foreach (var entry in archive.Entries.Where(e => !e.IsDirectory))
        {
            ExtractEntry(entry, outputDirectory);
        }
    }

    private static void ExtractEntry(SharpCompress.Archives.IArchiveEntry entry, string outputDirectory)
    {
        string destinationPath = Path.Combine(outputDirectory, entry.Key);
        string? destinationDir = Path.GetDirectoryName(destinationPath);
        
        if (destinationDir != null)
            Directory.CreateDirectory(destinationDir);

        Console.WriteLine($"  Extracting: {entry.Key}");
        
        using Stream entryStream = entry.OpenEntryStream();
        using FileStream fileStream = File.Create(destinationPath);
        entryStream.CopyTo(fileStream);
    }
}