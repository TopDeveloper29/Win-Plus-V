using System;
using System.Collections.Generic;
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
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI;


namespace Win_Plus_V;
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        ResizeRelativeToScreen(_window.AppWindow, 0.2, 0.4);
        _window.ExtendsContentIntoTitleBar = true;
        _window.AppWindow.TitleBar.ButtonForegroundColor = Color.FromArgb(255, 200, 200, 200);

        if (_window.AppWindow.Presenter is OverlappedPresenter op)
        {
            op.SetBorderAndTitleBar(true, false);
            op.IsResizable = false;
        }

#if DEBUG
        _window.Activate();
#endif

    }

    public static void ResizeRelativeToScreen(AppWindow AppWindow, double widthRatio, double heightRatio)
    {
        if (Current == null)
            return;

        var displayArea = DisplayArea.GetFromWindowId(AppWindow.Id, DisplayAreaFallback.Nearest); ;
        var workArea = displayArea.WorkArea; // excludes taskbar

        var targetWidth = (int)(workArea.Width * widthRatio);
        var targetHeight = (int)(workArea.Height * heightRatio);

        var x = workArea.X + (workArea.Width - targetWidth) / 2;
        var y = workArea.Y + (workArea.Height - targetHeight) / 2;

        AppWindow.Resize(new SizeInt32(targetWidth, targetHeight));
    }
}
