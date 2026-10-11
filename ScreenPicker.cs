using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SleepWallpaper
{
    // Full-screen "eyedropper" overlay: shows a frozen screenshot, a magnifier and the hex value.
    public class ScreenPicker : Form
    {
        readonly Bitmap shot;
        Point mouse;
        public Color? Picked;

        public ScreenPicker(Bitmap shot, Rectangle area)
        {
            this.shot = shot;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = area;
            TopMost = true;
            ShowInTaskbar = false;
            Cursor = Cursors.Cross;
            DoubleBuffered = true;
            KeyPreview = true;
            mouse = new Point(area.Width / 2, area.Height / 2);
        }

        Color PixelAt(Point p)
        {
            int x = Math.Max(0, Math.Min(shot.Width - 1, p.X));
            int y = Math.Max(0, Math.Min(shot.Height - 1, p.Y));
            return shot.GetPixel(x, y);
        }

        static string Hex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.DrawImageUnscaled(shot, 0, 0);

            // hint bar
            using (var f = new Font("Segoe UI", 11f))
            {
                string hint = "Click to pick a color   |   Esc / right-click to cancel";
                var sz = g.MeasureString(hint, f);
                var r = new RectangleF((ClientSize.Width - sz.Width) / 2 - 12, 12, sz.Width + 24, sz.Height + 10);
                using (var b = new SolidBrush(Color.FromArgb(200, 20, 20, 20))) g.FillRectangle(b, r);
                g.DrawString(hint, f, Brushes.White, r.X + 12, r.Y + 5);
            }

            // magnifier
            const int n = 11, cell = 12, size = n * cell;
            int mx = mouse.X + 24, my = mouse.Y + 24;
            if (mx + size + 4 > ClientSize.Width) mx = mouse.X - 24 - size;
            if (my + size + 34 > ClientSize.Height) my = mouse.Y - 24 - size - 30;

            var dest = new Rectangle(mx, my, size, size);
            var src = new Rectangle(mouse.X - n / 2, mouse.Y - n / 2, n, n);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.FillRectangle(Brushes.Black, dest);
            g.DrawImage(shot, dest, src.X, src.Y, src.Width, src.Height, GraphicsUnit.Pixel);

            using (var p = new Pen(Color.White, 2)) g.DrawRectangle(p, dest);
            var center = new Rectangle(mx + (n / 2) * cell, my + (n / 2) * cell, cell, cell);
            using (var p = new Pen(Color.Black, 2)) g.DrawRectangle(p, center);
            using (var p = new Pen(Color.White, 1)) g.DrawRectangle(p, center.X + 2, center.Y + 2, cell - 4, cell - 4);

            var c = PixelAt(mouse);
            var label = new Rectangle(mx, my + size + 2, size, 28);
            using (var b = new SolidBrush(Color.FromArgb(230, 20, 20, 20))) g.FillRectangle(b, label);
            using (var b = new SolidBrush(c)) g.FillRectangle(b, label.X + 6, label.Y + 6, 16, 16);
            g.DrawRectangle(Pens.White, label.X + 6, label.Y + 6, 16, 16);
            using (var f = new Font("Consolas", 11f, FontStyle.Bold))
                g.DrawString(Hex(c), f, Brushes.White, label.X + 30, label.Y + 4);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            mouse = e.Location;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Picked = PixelAt(e.Location);
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
            Close();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}
