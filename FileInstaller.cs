using System.IO;

public static class FileInstaller
{
    public static void MoveFiles(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (string sourceFile in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
            string destinationPath = Path.Combine(destinationDir, relativePath);
            
            string? destinationFolder = Path.GetDirectoryName(destinationPath);
            if (destinationFolder != null)
                Directory.CreateDirectory(destinationFolder);

            File.Move(sourceFile, destinationPath, overwrite: true);
        }
    }

    public static void MoveFile(string sourceFile, string destinationPath)
    {
        string? destinationDir = Path.GetDirectoryName(destinationPath);
        if (destinationDir != null)
            Directory.CreateDirectory(destinationDir);

        File.Move(sourceFile, destinationPath, overwrite: true);
    }
}