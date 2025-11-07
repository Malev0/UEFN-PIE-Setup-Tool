using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public static class NeoniteVerifier
{
    private const string SUCCESS_MESSAGE = "Neonite is up and listening";
    private const int TIMEOUT_SECONDS = 120;

    public static async Task StartAndVerifyAsync(string batFilePath)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = batFilePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = false,
            WorkingDirectory = Path.GetDirectoryName(batFilePath)
        };

        using Process process = new() { StartInfo = startInfo };
        
        bool successDetected = false;

        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine($"  [Neonite] {e.Data}");
                
                if (e.Data.Contains(SUCCESS_MESSAGE, StringComparison.OrdinalIgnoreCase))
                {
                    successDetected = true;
                }
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine($"  [Neonite Error] {e.Data}");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Wait for success message or timeout
        DateTime startTime = DateTime.Now;
        while (!successDetected && (DateTime.Now - startTime).TotalSeconds < TIMEOUT_SECONDS)
        {
            if (process.HasExited)
            {
                throw new Exception("Neonite process exited unexpectedly");
            }

            await Task.Delay(500);
        }

        if (!successDetected)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException($"Neonite did not start within {TIMEOUT_SECONDS} seconds");
        }

        // Success! Close the process
        Console.WriteLine("\n✓ Neonite verified running. Closing verification process...");
        await Task.Delay(1000);
        
        try
        {
            process.Kill(entireProcessTree: true);
            process.WaitForExit(5000);
        }
        catch
        {
            // Process might have already closed
        }
    }
}