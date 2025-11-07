using System;
using System.IO;
using System.Threading.Tasks;

public class Program
{
    private const string URLS_CONFIG_URL = "https://raw.githubusercontent.com/Malev0/UEFN-PIE-Setup-Tool/refs/heads/main/downloadUrls.txt";
    private const string CONFIG_FILE = "pie.config";

    static async Task Main()
    {
        PrintBanner();

        string? configPath = ConfigManager.LoadConfig(CONFIG_FILE);
        
        if (configPath != null && File.Exists(configPath))
        {
            Console.WriteLine($"\n✓ Found saved Fortnite path: {configPath}");
            await ShowMainMenuAsync(configPath);
        }
        else
        {
            await RunFirstTimeSetupAsync();
        }
    }

    private static async Task ShowMainMenuAsync(string fortnitePath)
    {
        while (true)
        {
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║           UEFN PIE SETUP TOO           ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine("\n1. Launch UEFN PIE");
            Console.WriteLine("2. Re-run Installation");
            Console.WriteLine("3. Change Fortnite Path");
            Console.WriteLine("4. Exit");
            Console.Write("\nSelect option: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await LaunchUEFNPIEAsync(fortnitePath);
                    break;
                case "2":
                    await RunFullInstallationAsync(fortnitePath);
                    break;
                case "3":
                    fortnitePath = await ChangeFortnitePathAsync();
                    break;
                case "4":
                    Console.WriteLine("\nExiting...");
                    return;
                default:
                    Console.WriteLine("\n✗ Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static async Task RunFirstTimeSetupAsync()
    {
        Console.WriteLine("\n=== First Time Setup ===");
        
        string fortnitePath = await GetFortnitePathAsync();
        
        ConfigManager.SaveConfig(CONFIG_FILE, fortnitePath);
        Console.WriteLine($"\n✓ Saved Fortnite path to {CONFIG_FILE}");

        Console.WriteLine("\nSetup complete! Showing main menu...");
        await Task.Delay(2000);
        Console.Clear();
        PrintBanner();
        await ShowMainMenuAsync(fortnitePath);
    }

    private static async Task RunFullInstallationAsync(string fortnitePath)
    {
        try
        {
            Console.WriteLine("\nFetching download URLs...");
            DownloadUrls urls = await UrlConfigLoader.LoadUrlsAsync(URLS_CONFIG_URL);
            Console.WriteLine("✓ URLs loaded successfully");

            string exeDirectory = Path.GetDirectoryName(fortnitePath)!;
            string ubaPath = GetUbaPath(exeDirectory);
            
            Console.WriteLine($"\nTarget UBA path: {ubaPath}");
            
            await InstallPatchedFilesAsync(exeDirectory, ubaPath, urls);
            await InstallNeoniteAsync(urls.NeoniteUrl);
            
            Console.WriteLine("\n✓ Installation completed successfully!");
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
            Console.Clear();
            PrintBanner();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Installation failed: {ex.Message}");
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
            Console.Clear();
            PrintBanner();
        }
    }

    private static async Task<string> ChangeFortnitePathAsync()
    {
        Console.WriteLine("\n=== Change Fortnite Path ===");
        string fortnitePath = await GetFortnitePathAsync();
        
        ConfigManager.SaveConfig(CONFIG_FILE, fortnitePath);
        Console.WriteLine($"\n✓ Updated Fortnite path");
        
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        PrintBanner();
        
        return fortnitePath;
    }

    private static async Task LaunchUEFNPIEAsync(string fortnitePath)
    {
        try
        {
            Console.WriteLine("\n=== Launching UEFN PIE ===");
            
            string neoniteBatPath = Path.Combine(
                Directory.GetCurrentDirectory(), 
                "Neonite-main", 
                "Start Neonite.bat"
            );
            
            if (!File.Exists(neoniteBatPath))
            {
                Console.WriteLine($"✗ Error: Neonite not found at {neoniteBatPath}");
                Console.WriteLine("Please run 'Re-run Installation' first.");
                PauseAndReturnToMenu();
                return;
            }

            if (!File.Exists(fortnitePath))
            {
                Console.WriteLine($"✗ Error: Fortnite executable not found at {fortnitePath}");
                Console.WriteLine("Please update your Fortnite path.");
                PauseAndReturnToMenu();
                return;
            }

            Console.WriteLine("Starting Neonite...");
            ProcessLauncher.LaunchNeonite(neoniteBatPath);
            
            Console.WriteLine("Waiting 3 seconds...");
            await Task.Delay(3000);
            
            Console.WriteLine("Launching UEFN...");
            ProcessLauncher.LaunchUEFN(fortnitePath);
            
            Console.WriteLine("\n✓ UEFN PIE launched successfully!");
            PauseAndReturnToMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error launching UEFN PIE: {ex.Message}");
            PauseAndReturnToMenu();
        }
    }

    private static void PrintBanner()
    {
        Console.WriteLine(@"
=========================================================================
|     ___ ___ ___   ___ ___ _____ _   _ ___   _____ ___   ___  _      |
|    | _ \_ _| __| / __| __|_   _| | | | _ \ |_   _/ _ \ / _ \| |     |
|    |  _/| || _|  \__ \ _|  | | | |_| |  _/   | || (_) | (_) | |__   |
|    |_| |___|___| |___/___| |_|  \___/|_|     |_| \___/ \___/|____|  |
|                                                                       |
=========================================================================");
    }

    private static async Task<string> GetFortnitePathAsync()
    {
        Console.WriteLine("\nPress Enter to auto-locate Fortnite, or enter installation path:");
        string? userInput = Console.ReadLine();

        string? exePath;
        if (string.IsNullOrWhiteSpace(userInput))
        {
            Console.WriteLine("Searching for Fortnite installation...");
            exePath = FortniteLocator.FindFortniteExecutable();
        }
        else
        {
            exePath = FortniteLocator.FindInDirectory(userInput);
        }

        if (exePath != null && File.Exists(exePath))
        {
            Console.WriteLine($"✓ Found Fortnite: {exePath}");
            return exePath;
        }

        Console.WriteLine("✗ Fortnite not found. Please enter the full path to UnrealEditorFortnite-Win64-Shipping.exe:");
        string? manualPath = Console.ReadLine();
        
        if (!string.IsNullOrWhiteSpace(manualPath) && File.Exists(manualPath))
        {
            return manualPath;
        }

        throw new FileNotFoundException("Could not locate Fortnite executable.");
    }

    private static string GetUbaPath(string exeDirectory)
    {
        return Path.GetFullPath(Path.Combine(
            exeDirectory, 
            @"..\..\..\Engine\Binaries\Win64\UnrealBuildAccelerator\x64"
        ));
    }

    private static async Task InstallPatchedFilesAsync(string exeDirectory, string ubaPath, DownloadUrls urls)
    {
        try
        {
            // Download and extract patched binaries
            Console.WriteLine("\n[1/2] Downloading patched binaries...");
            string archivePath = await Downloader.DownloadFileAsync(
                urls.PatchedBinariesUrl, 
                "patched_binaries.7z"
            );
            Console.WriteLine($"✓ Downloaded to temporary location");

            Console.WriteLine("\n[2/2] Extracting archive...");
            string extractPath = Path.Combine(Path.GetTempPath(), $"pie_extract_{Guid.NewGuid():N}");
            ArchiveExtractor.Extract7z(archivePath, extractPath);
            
            FileInstaller.MoveFiles(extractPath, exeDirectory);
            Console.WriteLine($"✓ Installed binaries to: {exeDirectory}");

            // Download and install UbaHost.dll
            Console.WriteLine("\nDownloading UbaHost.dll...");
            string dllPath = await Downloader.DownloadFileAsync(
                urls.UbaHostUrl, 
                "UbaHost.dll"
            );

            string ubaDestination = Path.Combine(ubaPath, "UbaHost.dll");
            FileInstaller.MoveFile(dllPath, ubaDestination);
            Console.WriteLine($"✓ Installed UbaHost.dll to: {ubaDestination}");

            // Cleanup
            CleanupTemp(archivePath, extractPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error: {ex.Message}");
            throw;
        }
    }

    private static void CleanupTemp(string archivePath, string extractPath)
    {
        try
        {
            if (File.Exists(archivePath))
                File.Delete(archivePath);
            
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, recursive: true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private static async Task InstallNeoniteAsync(string neoniteUrl)
    {
        try
        {
            Console.WriteLine("\n[3/3] Downloading Neonite...");
            string currentDir = Directory.GetCurrentDirectory();
            string zipPath = await Downloader.DownloadFileAsync(neoniteUrl, "neonite.zip");

            Console.WriteLine("Extracting Neonite...");
            ArchiveExtractor.ExtractZip(zipPath, currentDir);
            Console.WriteLine($"✓ Extracted to: {currentDir}");

            File.Delete(zipPath);
            Console.WriteLine("✓ Cleaned up zip file");

            string batPath = Path.Combine(currentDir, "Neonite-main", "Start Neonite.bat");
            
            if (!File.Exists(batPath))
            {
                Console.WriteLine($"⚠ Warning: Could not find Start Neonite.bat");
                return;
            }

            Console.WriteLine("\nStarting Neonite...");
            await NeoniteVerifier.StartAndVerifyAsync(batPath);
            Console.WriteLine("✓ Neonite started and verified");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n⚠ Warning: Neonite setup failed: {ex.Message}");
            Console.WriteLine("You can manually start Neonite later.");
        }
    }

    private static void PauseAndReturnToMenu()
    {
        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
        Console.Clear();
        PrintBanner();
    }
}