using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace WinV
{
    public partial class WinClip : MaterialForm
    {
        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        public static extern bool ShouldSystemUseDarkMode();
        HotkeyManager hotkeyManager = new HotkeyManager();

        ObservableCollection<ClipItem> obci = new ObservableCollection<ClipItem>();
        private int Y = 0;
        private ClipItem SelectedItem = null;
        private bool allowshowdisplay = false;
        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(allowshowdisplay ? value : allowshowdisplay);
        }
        public WinClip()
        {
            hotkeyManager.RegisterHotkey(Keys.V, KeyModifiers.Control | KeyModifiers.Alt);
            hotkeyManager.HotkeyPressed += HotkeyManager_HotkeyPressed;
            obci.CollectionChanged += Obci_CollectionChanged;
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            
            if (ShouldSystemUseDarkMode())
            {
                materialSkinManager.Theme = MaterialSkinManager.Themes.DARK;
            }
            else
            {
                this.BackColor = Color.White;
                materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            }

            materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
        }

        private void Obci_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
                body.Controls.Clear();
                foreach (ClipItem item in obci)
                {
                    body.Controls.Add(item);
                }     
        }

        private void WinClip_Load(object sender, EventArgs e)
        {
            this.FormClosed += WinClip_FormClosed; ;
            ClipboardReader clipboardReader = new ClipboardReader(this);
            clipboardReader.Start();
        }

        private void WinClip_FormClosed(object sender, FormClosedEventArgs e)
        {
            hotkeyManager.UnregisterHotkeys();
            Environment.Exit(0);
        }

        private void HotkeyManager_HotkeyPressed(object sender, HotkeyEventArgs e)
        {
            if (this.Visible)
            {
                this.Hide();
            }
            else
            {
                this.allowshowdisplay = true;
                this.Visible = !this.Visible;
                this.Location = new Point(Cursor.Position.X, Cursor.Position.Y);
                this.Show();
            }
            
        }

        internal void AddItem(object val)
        {
            ClipItem ti = new ClipItem();
            ti.Value = val;
            ti.Location = new Point(10, Y);
            ti.Width = body.Width-40;
            ti.ClipItemEvent += Ti_ClipItemEvent;
            obci.Add(ti);
            Y += 80;
        }

        private void Ti_ClipItemEvent(object sender, ClipItemEventArgs e)
        {
            ClipItem item = e._ClipItem as ClipItem;
            if (item.HoverDot)
            {
                SelectedItem = item;
               
                cms.Show(new Point(Cursor.Position.X-120,Cursor.Position.Y));
            }
            else
            {
                if (item.Value.GetType().ToString().Contains("Bitmap"))
                {
                    Clipboard.SetImage((Image)item.Value);
                }
                else
                {
                    Clipboard.SetText(item.Value.ToString());
                }
                this.Hide();
            }
        }

        private void deleteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Y = 0;
            obci.Clear();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
            {
                obci.Remove(SelectedItem);
                List<ClipItem> list = obci.ToList();
                ObservableCollection<ClipItem> obciCopy = new ObservableCollection<ClipItem>(list);
                obci.Clear();
                Y = 0;
                foreach (ClipItem item in obciCopy)
                {
                    AddItem(item.Value);
                }
                SelectedItem = null;
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedItem != null)
            {
                if (SelectedItem.Value.GetType().ToString().Contains("Image"))
                {
                    Clipboard.SetImage((Image)SelectedItem.Value);
                }
                else
                {
                    Clipboard.SetText(SelectedItem.Value.ToString());
                }
                SelectedItem = null;
            }
        }
    }
}
