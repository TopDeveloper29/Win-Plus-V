using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Win_Plus_V.Class;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Win_Plus_V.Dialog;

public sealed partial class Settings : ContentDialog
{
    public Settings()
    {
        this.InitializeComponent();
        CacheItem.IsOn = AppSettings.CurrentSettings.CacheItem;
        ThemeComboBox.SelectedIndex = AppSettings.CurrentSettings.AppTheme;
    }

    private void Cancel_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
    }

    private void Save_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Save the settings from the dialog fields to the application settings
        AppSettings.CurrentSettings.CacheItem = CacheItem.IsOn;
        AppSettings.CurrentSettings.AppTheme = ThemeComboBox.SelectedIndex;

        // Save settings to registry
        AppSettings.SaveSettings();
    }

}
