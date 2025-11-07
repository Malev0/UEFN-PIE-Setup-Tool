using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class ConfigManager
{
    public static string? LoadConfig(string configFile)
    {
        try
        {
            if (File.Exists(configFile))
            {
                string path = File.ReadAllText(configFile).Trim();
                return string.IsNullOrWhiteSpace(path) ? null : path;
            }
        }
        catch
        {
            // Config file is corrupted or unreadable
        }
        
        return null;
    }

    public static void SaveConfig(string configFile, string fortnitePath)
    {
        File.WriteAllText(configFile, fortnitePath);
        UpdateUEFNPIESettings();
    }

    private static void UpdateUEFNPIESettings()
    {
        try
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string configPath = Path.Combine(
                userProfile,
                "AppData", "Local", "UnrealEditorFortnite", "Saved", "Config", "WindowsEditor"
            );
            string iniFilePath = Path.Combine(configPath, "EditorPerProjectUserSettings.ini");

            Directory.CreateDirectory(configPath);

            List<string> lines = File.Exists(iniFilePath) 
                ? File.ReadAllLines(iniFilePath).ToList() 
                : new List<string>();

            // Find or create the OnlinePIESettings section
            int sectionIndex = lines.FindIndex(line => 
                line.Trim().Equals("[/Script/OnlineSubsystemUtils.OnlinePIESettings]", 
                    StringComparison.OrdinalIgnoreCase));

            if (sectionIndex != -1)
            {
                // Remove existing settings in this section
                RemoveExistingSettings(lines, sectionIndex);
                
                // Insert new settings
                lines.Insert(sectionIndex + 1, "bOnlinePIEEnabled=True");
                lines.Insert(sectionIndex + 2, "Logins=(Id=\"pie@pie.com\",Type=\"exchangecode\",TokenBytes=(231,206,173,183,239,219,173,222,239,219,173,178,239,210,173,177))");
            }
            else
            {
                // Add new section at the end
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                {
                    lines.Add("");
                }
                
                lines.Add("[/Script/OnlineSubsystemUtils.OnlinePIESettings]");
                lines.Add("bOnlinePIEEnabled=True");
                lines.Add("Logins=(Id=\"pie@pie.com\",Type=\"exchangecode\",TokenBytes=(231,206,173,183,239,219,173,222,239,219,173,178,239,210,173,177))");
            }

            File.WriteAllLines(iniFilePath, lines);
            Console.WriteLine("✓ Updated UEFN PIE settings");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Warning: Could not update UEFN PIE settings: {ex.Message}");
        }
    }

    private static void RemoveExistingSettings(List<string> lines, int sectionIndex)
    {
        int linesToRemove = 0;
        for (int i = sectionIndex + 1; i < lines.Count && linesToRemove < 2; i++)
        {
            string trimmed = lines[i].Trim();
            
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }
            
            if (trimmed.StartsWith("["))
            {
                break; // Hit another section
            }
            
            lines.RemoveAt(i);
            linesToRemove++;
            i--;
        }
    }
}