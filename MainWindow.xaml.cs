using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Win32;
using Win_Plus_V.Class;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Notifications;
using static System.Net.Mime.MediaTypeNames;


namespace Win_Plus_V;

public sealed partial class MainWindow : Window
{
    public ObservableCollection<ClipboardItem> Items = [];
    public static new MainWindow Current { get; private set; } = new();

    public MainWindow()
    {
        Current = this;
        InitializeComponent();
        SetTitleBar(TitleBar);
        RegisterHotkeyBaseOnWindowsEdition();

        // Listen for clipboard changes
        Clipboard.ContentChanged += Clipboard_ContentChanged;
        Activated += MainWindow_Activated;

        AppSettings.LoadSettings();
        ApplySettings();
        
    }

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
        if (args.WindowActivationState == WindowActivationState.Deactivated && !HotkeyManager.BlockHiding)
        {
            // Window lost focus
            Debug.WriteLine("Window lost focus");

            // Example: Hide window
            AppWindow.Hide();
        }
    }

    private async void Clipboard_ContentChanged(object? sender, object e)
    {

        DataPackageView dataPackageView = Clipboard.GetContent();
        if (dataPackageView != null)
        {
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    var Text = await dataPackageView.GetTextAsync();
                    if (Items.FirstOrDefault()?.Text != Text)
                    {
                        Items.Insert(0, new ClipboardItem(Text, DateTime.Now));
                        if (AppSettings.CurrentSettings.CacheItem)
                        {
                            AppSettings.CurrentSettings.Items = Items;
                            AppSettings.SaveSettings();
                        }
                    }
                });
            }
            else if (dataPackageView.Contains(StandardDataFormats.Bitmap))
            {
                var bitmapRef = await dataPackageView.GetBitmapAsync();
                var stream = await bitmapRef.OpenReadAsync();

                using var tempStream = new InMemoryRandomAccessStream();
                await RandomAccessStream.CopyAsync(stream, tempStream);

                tempStream.Seek(0);

                byte[] imageBytes = new byte[tempStream.Size];
                await tempStream.ReadAsync(imageBytes.AsBuffer(), (uint)tempStream.Size, InputStreamOptions.None);

                DispatcherQueue.TryEnqueue(async () =>
                {
                    var uiStream = new InMemoryRandomAccessStream();
                    await uiStream.WriteAsync(imageBytes.AsBuffer());
                    uiStream.Seek(0);

                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(uiStream);
                    if (Items.FirstOrDefault()?.ImageBytes != imageBytes)
                    {
                        Items.Insert(0, new ClipboardItem(bitmapImage, imageBytes, DateTime.Now));
                        if (AppSettings.CurrentSettings.CacheItem)
                        {
                            AppSettings.CurrentSettings.Items = Items;
                            AppSettings.SaveSettings();
                        }
                    }
                });
            }


        }
    }

    #region Helpers
    public static void RegisterHotkeyBaseOnWindowsEdition()
    {
        var registryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
        var productName = (string)Registry.GetValue(registryKey, "ProductName", "")!;

        if (string.IsNullOrEmpty(productName))
        {
            Console.WriteLine("Could not determine OS name from registry.");
            return;
        }

        // If true, the hotkey will be set to to Win + V otherwise it will be set Ctrl + Alt + V
        HotkeyManager.RegisterHotKey(productName.Contains("Server"));

    }
    public static async void ApplySettings()
    {
        if (AppSettings.CurrentSettings.CacheItem)
        {
            // Don't replace the collection instance (bindings depend on it) — copy items instead
            Current.Items.Clear();
            foreach (var it in AppSettings.CurrentSettings.Items)
            {
                if (it == null)
                    continue;

                Current.Items.Add(it);

                if (it.Type == ClipboardItemType.Image && it.ImageBytes != null)
                {
                    await it.LoadImageAsync();
                }
            }
        }

        switch (AppSettings.CurrentSettings.AppTheme)
        {
            case 0:
                Current.MainGrid.Background = new SolidColorBrush(Color.FromArgb(255, 39, 39, 39));
                break;
            case 1:
                Current.MainGrid.Background = new SolidColorBrush(Color.FromArgb(80, 20, 20, 20));
                Current.SystemBackdrop = new MicaBackdrop();
                break;
            case 2:
                Current.MainGrid.Background = new SolidColorBrush(Color.FromArgb(80, 20, 20, 20));
                Current.SystemBackdrop = new DesktopAcrylicBackdrop();
                break;
        }

    }

    public async void CopyItem(ClipboardItem item)
    {
        switch (item.Type)
        {
            case ClipboardItemType.Text:
                var dataPackage = new DataPackage();
                dataPackage.SetText(item.Text);
                Clipboard.SetContent(dataPackage);
                break;
            case ClipboardItemType.Image:
                var dataPackage2 = new DataPackage();
                // Prefer raw bytes if available
                if (item.ImageBytes != null)
                {
                    var stream = new InMemoryRandomAccessStream();
                    await stream.WriteAsync(item.ImageBytes.AsBuffer());
                    stream.Seek(0);

                    var ras = RandomAccessStreamReference.CreateFromStream(stream);
                    dataPackage2.SetBitmap(ras);
                    Clipboard.SetContent(dataPackage2);
                }
                else if (item.Image != null)
                {
                    // If only BitmapImage is present we cannot directly get its bytes here easily.
                    // As a fallback, set an empty package (or extend to render the Image control to a stream).
                    Clipboard.SetContent(dataPackage2);
                }
                break;
        }
        AppWindow.Hide();
    }
    #endregion

    #region App buttons
    private void MinimizeButtton_Click(object sender, RoutedEventArgs e) => AppWindow.Hide();
    private async void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var Dialog = new Dialog.Settings { XamlRoot = Content.XamlRoot };
        if (await Dialog.ShowAsync() == ContentDialogResult.Primary)
        {
            ApplySettings();
        }
    }
    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        Items.Clear();
        AppSettings.CurrentSettings.Items.Clear();
        AppSettings.SaveSettings();
    }

    #endregion

    #region Items buttons
    private void CopyButton_Click(object sender, RoutedEventArgs args)
    {
        if (sender is Button bt && bt.DataContext is ClipboardItem item)
            CopyItem(item);
    }
    private void MoreButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Flyout is Flyout flyout)
        {
            flyout.ShowAt(button);
        }
    }
    private void CopyItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button bt && bt.DataContext is ClipboardItem item)
            CopyItem(item);

    }

    private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem bt && bt.DataContext is ClipboardItem item)
        {
            Items.Remove(item);
            if (AppSettings.CurrentSettings.CacheItem)
            {
                AppSettings.CurrentSettings.Items = Items;
                AppSettings.SaveSettings();
            }
        }
    }

    #endregion
}