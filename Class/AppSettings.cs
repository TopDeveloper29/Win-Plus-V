using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.UI.Xaml;
using Microsoft.Win32;

namespace Win_Plus_V.Class;

// Manages application settings stored in the Windows Registry
public static class AppSettings
{
    // Application data folder path
    public static DirectoryInfo AppdataFolder { get; } = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(Win_Plus_V)));

    // Current application settings instance
    public static Setting CurrentSettings { get; set; } = new();

    // Registry path and key constants
    public const string RegistryPath = $@"Software\{nameof(Win_Plus_V)}";

    // Key under which settings are stored
    private const string SettingsKey = "AppSettings";


    // Loads settings from the Windows Registry
    public static void LoadSettings()
    {
        try
        {
            // If the appdata folder do not exist create it
            if (!AppdataFolder.Exists)
                AppdataFolder.Create();


            // Attempt to read settings from the registry
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            if (key != null)
            {
                // Retrieve the raw JSON string
                var raw = key.GetValue(SettingsKey) as string;
                if (!string.IsNullOrEmpty(raw))
                {
                    // Deserialize JSON to Setting object
                    CurrentSettings = JsonSerializer.Deserialize<Setting>(raw) ?? new Setting();
                    // After deserialization, convert any stored image bytes into BitmapImage
                    if (CurrentSettings?.Items != null && MainWindow.Current != null)
                    {
                        foreach (var it in CurrentSettings.Items)
                        {
                            if (it?.Type == ClipboardItemType.Image && it.ImageBytes != null)
                            {
                                // Ensure image is created on the UI dispatcher
                                MainWindow.Current.DispatcherQueue.TryEnqueue(async () => await it.LoadImageAsync());
                            }
                        }
                    }

                    return;
                }
            }
        }
        catch
        {
            // Ignore and fallback to default
        }

        // If missing or corrupt, fallback to defaults
        CurrentSettings = new Setting();
    }

    // Saves current settings to the Windows Registry
    public static void SaveSettings()
    {
        try
        {
            // Create or open the registry key for writing
            using var key = Registry.CurrentUser.CreateSubKey(RegistryPath, writable: true);
            if (key != null)
            {
                // Serialize current settings to JSON
                var json = JsonSerializer.Serialize(CurrentSettings);
                key.SetValue(SettingsKey, json, RegistryValueKind.String);
            }
        }
        catch
        {
            // Optional: log or handle save errors
        }
    }
}

// Represents all application settings
public class Setting
{
    public bool CacheItem { get; set; } = true;
    public int AppTheme { get; set; } = 1;
    public ObservableCollection<ClipboardItem> Items { get; set; } = [];
}