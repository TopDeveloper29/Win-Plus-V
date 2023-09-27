using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinV
{
    public class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;

        public event EventHandler<HotkeyEventArgs> HotkeyPressed;

        private int currentId;

        public HotkeyManager()
        {
            Application.AddMessageFilter(new MessageFilter(this));
        }

        public void RegisterHotkey(Keys key, KeyModifiers modifiers)
        {
            currentId++;
            if (!RegisterHotKey(IntPtr.Zero, currentId, (uint)modifiers, (uint)key))
            {
                throw new InvalidOperationException("Couldn't register the hotkey.");
            }
        }

        public void UnregisterHotkeys()
        {
            for (int i = currentId; i > 0; i--)
            {
                UnregisterHotKey(IntPtr.Zero, i);
            }
        }

        private void OnHotkeyPressed(HotkeyEventArgs e)
        {
            HotkeyPressed?.Invoke(this, e);
        }

        private class MessageFilter : IMessageFilter
        {
            private readonly HotkeyManager hotkeyManager;

            public MessageFilter(HotkeyManager hotkeyManager)
            {
                this.hotkeyManager = hotkeyManager;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    var e = new HotkeyEventArgs(m.LParam.ToInt32());
                    hotkeyManager.OnHotkeyPressed(e);
                    return true;
                }
                return false;
            }
        }
    }

    public class HotkeyEventArgs : EventArgs
    {
        public int Id { get; }

        public HotkeyEventArgs(int id)
        {
            Id = id;
        }
    }

    public enum KeyModifiers
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Win = 8
    }

}
