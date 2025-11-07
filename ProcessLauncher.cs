using System.Diagnostics;
using System.IO;

public static class ProcessLauncher
{
    public static void LaunchNeonite(string batFilePath)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"{batFilePath}\"",
            UseShellExecute = true,
            CreateNoWindow = false,
            WorkingDirectory = Path.GetDirectoryName(batFilePath),
            WindowStyle = ProcessWindowStyle.Normal
        };

        Process.Start(startInfo);
    }

    public static void LaunchUEFN(string fortnitePath)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = fortnitePath,
            Arguments = "-disableplugins=\"AtomVK,ValkyrieFortnite\"",
            UseShellExecute = true,
            CreateNoWindow = false
        };

        Process.Start(startInfo);
    }
}