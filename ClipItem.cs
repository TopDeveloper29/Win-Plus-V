using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WinV
{

    public class ClipItem : Panel
    {
        // Border color
        private Color borderColor = Color.White;

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            //Fill background with BackColor
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            // Draw border
            ControlPaint.DrawBorder(e.Graphics, ClientRectangle, borderColor, (ButtonBorderStyle)this.BorderStyle);
        }
        //Private Item
        private Panel InFrame = new Panel();
        private Panel OutFrame = new Panel();
        private bool ImageMode;
        private Image image;
        private string text;
        private PictureBox pictureBox;
        private Label label;
        private Label button;
        protected virtual void OnClipItemEvent()
        {
            // Create an instance of ClipItemEventArgs and pass in the current instance of ClipItem
            ClipItemEventArgs args = new ClipItemEventArgs(this);

            // Raise the event, passing the arguments
            ClipItemEventHandler handler = ClipItemEvent;
            handler?.Invoke(this, args);
        }

        //Public Item

        public delegate void ClipItemEventHandler(object sender, ClipItemEventArgs e);
        public event ClipItemEventHandler ClipItemEvent;
        public bool HoverDot = false;
        public new ButtonBorderStyle BorderStyle;
        public new Color BackColor = Color.FromArgb(69, 69, 69);
        [Browsable(true)]
        [Category("Behavior")]
        public object Value
        {
            get 
            {
                if (ImageMode == true)
                {
                    return image;
                }
                else
                {
                    return text;
                }
            }
            set
            {
                InFrame.Controls.Clear();
                InFrame.BackColor = this.BackColor;
                OutFrame.BackColor = this.BackColor;
                button.BackColor = this.BackColor;

                if (value?.GetType().Name.Contains("Bitmap") == true)
                {
                    ImageMode = true;
                    image = (Image)value;
                    pictureBox = new PictureBox();
                    pictureBox.Image = image;
                    pictureBox.Dock = DockStyle.Fill;
                    pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox.BackColor = this.BackColor;
                    InFrame.Dock = DockStyle.Left;
                    InFrame.Controls.Add(pictureBox);
                }
                else
                {
                    ImageMode = false;
                    text = (string)value;
                    label = new Label();
                    label.Top = 8;
                    label.Left = 8;
                    label.AutoSize = false;
                    label.Font = new Font(label.Font.FontFamily, 8);
                    label.AutoEllipsis = false;
                    label.Width = InFrame.Width+15;
                    label.Height = InFrame.Height-10;
                    label.Text = text;
                    label.BackColor = this.BackColor;
                    InFrame.Dock = DockStyle.Fill;
                    InFrame.Controls.Add(label);
                }

                button.MouseEnter += Button_MouseEnter1;
                button.MouseLeave += Button_MouseLeave1;
                label.MouseEnter += ClipItem_MouseEnter;
                label.MouseLeave += ClipItem_MouseLeave;

                label.MouseClick += InFrame_MouseClick;
                pictureBox.MouseClick += InFrame_MouseClick;
                button.MouseClick += InFrame_MouseClick;
            }
        }

        public ClipItem()
        {
            if (WinClip.ShouldSystemUseDarkMode())
            {
                BackColor = Color.FromArgb(69, 69, 69);
            }
            else
            {
                BackColor = Color.FromArgb(255, 255, 255);
            }
            label = new Label();
            pictureBox = new PictureBox();
            
            button = new Label();
            button.Text = "...";
            button.Dock = DockStyle.Top;
            button.Font = new Font("Arial", 11, FontStyle.Bold);
            button.MouseEnter += Button_MouseEnter;
            button.MouseLeave += Button_MouseLeave;

            InFrame.Dock = DockStyle.Fill;
            InFrame.BackColor = this.BackColor;
            
            OutFrame.Dock = DockStyle.Right;
            OutFrame.Controls.Add(button);
            OutFrame.Width = 25;
            OutFrame.BackColor = this.BackColor;

            this.Height = 70;
            this.Padding = new Padding(1);
            this.Controls.Add(InFrame);
            this.Controls.Add(OutFrame);
            this.BorderStyle = ButtonBorderStyle.None;
            if (WinClip.ShouldSystemUseDarkMode())
            {
                this.BorderColor = Color.White;
            }
            else
            {
                this.BorderColor = Color.DarkGray;
            }

            InFrame.MouseEnter += ClipItem_MouseEnter;
            InFrame.MouseLeave += ClipItem_MouseLeave;
            pictureBox.MouseEnter += ClipItem_MouseEnter;
            pictureBox.MouseLeave += ClipItem_MouseLeave;
            OutFrame.MouseEnter += ClipItem_MouseEnter;
            OutFrame.MouseLeave += ClipItem_MouseLeave;
            button.MouseEnter += Button_MouseEnter1;
            button.MouseLeave += Button_MouseLeave1;
            label.MouseEnter += ClipItem_MouseEnter;
            label.MouseLeave += ClipItem_MouseLeave;

            label.MouseClick += InFrame_MouseClick;
            pictureBox.MouseClick += InFrame_MouseClick;
            button.MouseClick += InFrame_MouseClick;
            InFrame.MouseClick += InFrame_MouseClick;
            OutFrame.MouseClick += InFrame_MouseClick;
            
        }

        private void InFrame_MouseClick(object sender, MouseEventArgs e)
        {
            OnClipItemEvent();
        }

        private void Button_MouseEnter1(object sender, EventArgs e)
        {
            this.BorderStyle = ButtonBorderStyle.Solid;
            if (WinClip.ShouldSystemUseDarkMode())
            {
                this.BorderColor = Color.White;
            }
            else
            {
                this.BorderColor = Color.DarkGray;
            }
            HoverDot = true;
        }

        private void Button_MouseLeave1(object sender, EventArgs e)
        {
            HoverDot = false;
            this.BorderStyle = ButtonBorderStyle.None;
            if (WinClip.ShouldSystemUseDarkMode())
            {
                this.BorderColor = Color.White;
            }
            else
            {
                this.BorderColor = Color.DarkGray;
            }
        }

        private void ClipItem_MouseLeave(object sender, EventArgs e)
        {
            this.BorderStyle = ButtonBorderStyle.None;
            if (WinClip.ShouldSystemUseDarkMode())
            {
                this.BorderColor = Color.White;
            }
            else
            {
                this.BorderColor = Color.DarkGray;
            }
        }

        private void ClipItem_MouseEnter(object sender, EventArgs e)
        {
            this.BorderStyle = ButtonBorderStyle.Solid;
            if (WinClip.ShouldSystemUseDarkMode())
            {
                this.BorderColor = Color.White;
            }
            else
            {
                this.BorderColor = Color.DarkGray;
            }
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            button.ForeColor = Parent.ForeColor;
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            Color temp = Parent.ForeColor;
            int amount;
            if (WinClip.ShouldSystemUseDarkMode())
            {
                amount = 100;
            }
            else
            {
                amount = -100;
            }
            

            Color lighterColor = Color.FromArgb(
                temp.R - amount,
                temp.G - amount,
                temp.B - amount
            );
            button.ForeColor = lighterColor;
        }

    }
    public class ClipItemEventArgs : EventArgs
    {
        public ClipItem _ClipItem { get; }

        public ClipItemEventArgs(ClipItem clipItem)
        {
            _ClipItem = clipItem;
        }
    }
}
