using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace Sailing
{
    public abstract class Content
    {
        public Content(ContentManager m) { m.Draw += Draw; }
        public abstract void Draw(Graphics g, float t);
    }

    public class ContentManager
    {
        Graphics graphics;
        public Color background;
        public int width, height;
        public bool showFPS;
        PointF FPStextPosition;
        Font font;
        Thread drawing;
        public bool rendering;
        Bitmap buffer;
        Graphics bufferGraphics;
        long lastDraw;
        public delegate void DrawEvent(Graphics g, float t);
        public event DrawEvent Draw;
        public delegate void KeyDownEvent(KeyEventArgs e);
        public event KeyDownEvent KeyDown;
        public ContentManager(Form form, Color background, bool showFPStext) {
            width = form.ClientSize.Width;
            height = form.ClientSize.Height;
            showFPS = showFPStext;
            graphics = Graphics.FromHwnd(form.Handle);
            this.background = background;
            form.BackColor = background;
            drawing = new Thread(() => { while (true) { if (rendering) onDraw(); else {
                Thread.Sleep(250); lastDraw = DateTime.Now.Ticks; } } });
            form.KeyDown += (object s, KeyEventArgs e) => {
                if (e.KeyCode == Keys.Escape) Application.Exit();
                else if (e.KeyCode == Keys.F1) showFPS = !showFPS;
                if (KeyDown != null) KeyDown(e);
            };
            form.FormClosing += (object s, FormClosingEventArgs e) => { drawing.Abort(); };
            onFormResize();
            font = new Font(FontFamily.GenericSansSerif, 14f, FontStyle.Bold);
        }
        public void StartRender() {
            lastDraw = DateTime.Now.Ticks;
            rendering = true;
            if (!drawing.IsAlive) drawing.Start();
        }
        void onDraw() {
            bufferGraphics.Clear(background);
            float elapsedS = (float)(DateTime.Now.Ticks-lastDraw)/TimeSpan.TicksPerSecond;
            if (Draw != null) Draw(bufferGraphics, elapsedS);
            if (showFPS) bufferGraphics.DrawString(
                ((int)(1/elapsedS + .5f)).ToString(), font, Brushes.White, FPStextPosition);
            lastDraw = DateTime.Now.Ticks;
            graphics.DrawImage(buffer, 0, 0);
            Thread.Sleep(0);
        }
        void onFormResize() {
            if (buffer != null) buffer.Dispose();
            if (bufferGraphics != null) bufferGraphics.Dispose();
            buffer = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            bufferGraphics = Graphics.FromImage(buffer);
            bufferGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            bufferGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            FPStextPosition = new PointF(width-45, 5);
        }
    }
}