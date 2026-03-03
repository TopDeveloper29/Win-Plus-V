using System;
using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Windows.UI;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using System.Diagnostics;

namespace Win_Plus_V.Class;

internal static class HotkeyManager
{
    public static bool BlockHiding = false;
    private const uint V_KEY = 0x56;
    private const uint WM_HOTKEY = 0x0312;
    public const int GWL_WNDPROC = -4;
    private static WNDPROC? origPrc;
    private static WNDPROC? hotKeyPrc;
    public static void RegisterHotKey(bool UseWinV = false)
    {
        // Register a global hotkey to show/hide the window
        HWND hwnd = new HWND(WinRT.Interop.WindowNative.GetWindowHandle(MainWindow.Current).ToInt32());

        if (UseWinV == false)
        {
            PInvoke.RegisterHotKey(hwnd, 0, HOT_KEY_MODIFIERS.MOD_CONTROL | HOT_KEY_MODIFIERS.MOD_ALT, V_KEY);
        }
        else
        {
            PInvoke.RegisterHotKey(hwnd, 0, HOT_KEY_MODIFIERS.MOD_WIN, V_KEY);
        }

        hotKeyPrc = HotKeyPrc;
        var hotKeyPrcPointer = Marshal.GetFunctionPointerForDelegate(hotKeyPrc);
        origPrc = Marshal.GetDelegateForFunctionPointer<Windows.Win32.UI.WindowsAndMessaging.WNDPROC>(SetWindowLongPtr(hwnd, GWL_WNDPROC, hotKeyPrcPointer));
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    private static LRESULT HotKeyPrc(HWND hwnd, uint uMsg, WPARAM wParam, LPARAM lParam)
    {
        if (uMsg == WM_HOTKEY)
        {
            if (!MainWindow.Current.Visible)
            {
                BlockHiding = true;
                var window = MainWindow.Current;

                window.AppWindow.Show();

                // Bring to foreground
                PInvoke.SetForegroundWindow(hwnd);
                PInvoke.BringWindowToTop(hwnd);
                MainWindow.Current.MainGrid.Focus(FocusState.Programmatic);

                // Move windows to cursor
                POINT point;
                GetCursorPos(out point);

                MainWindow.Current.AppWindow.Move(new Windows.Graphics.PointInt32(point.X, point.Y));

                BlockHiding = false;
            }
            else if (!BlockHiding)
            {
                MainWindow.Current.AppWindow.Hide();
            }
            return (LRESULT)IntPtr.Zero;
        }

        return PInvoke.CallWindowProc(origPrc, hwnd, uMsg, wParam, lParam);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);
}
