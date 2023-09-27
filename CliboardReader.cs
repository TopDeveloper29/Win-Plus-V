using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace WinV
{
    public class ClipboardReader
    {
        public ClipboardReader(WinClip control)
        {
            this.control = control;
        }
        private Thread thread;
        private WinClip control;
        private string Last = "";
        private byte[] LastImg;
        bool isAdded = false; // Initialize isAdded flag
        public void Start()
        {
            thread = new Thread(ReadClipboard);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        public void Stop()
        {
            thread.Abort();
        }

        private void ReadClipboard()
        {
            while (true)
            {
                if (Clipboard.ContainsText())
                {
                    string clipboard = Clipboard.GetText();
                    if (clipboard != Last)
                    {
                        Last = clipboard;
                        control.Invoke(new Action(() => control.AddItem(clipboard)));
                    }
                }
                if (Clipboard.ContainsImage())
                {
                    System.Drawing.Image clipimg = Clipboard.GetImage();
                    byte[] imageBytes;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        clipimg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        imageBytes = ms.ToArray();
                    }

                    if (LastImg != null)
                    {
                        if (imageBytes.LongLength != LastImg.LongLength)
                        {
                            LastImg = imageBytes;
                            control.Invoke(new Action(() => control.AddItem(clipimg)));
                        }
                    }
                    else
                    {
                        LastImg = imageBytes;
                        control.Invoke(new Action(() => control.AddItem(clipimg)));
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
