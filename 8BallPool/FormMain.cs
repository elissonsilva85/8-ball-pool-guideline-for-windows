using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace _8BallPool
{
    public partial class FormMain : Form
    {
        private const int ReferenceBallSize = 25;
        private const int BallCenterDotSize = 2;
        private const int BallHitAreaRadius = 15;
        private const int CornerLineLength = 40;
        private const int CornerLineThickness = 4;
        private const int PocketIndicatorSize = 3;
        private const int GuideLineThickness = 3;

        private Point lastBallPosition;
        private bool isDragging;
        private bool isInitialized;

        #region Win32 API

        private const int WS_EX_LAYERED = 0x00080000;
        private const int ULW_ALPHA = 0x00000002;
        private const byte AC_SRC_OVER = 0x00;
        private const byte AC_SRC_ALPHA = 0x01;

        [StructLayout(LayoutKind.Sequential)]
        private struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct W32Point
        {
            public int X;
            public int Y;

            public W32Point(int x, int y) { X = x; Y = y; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct W32Size
        {
            public int Width;
            public int Height;

            public W32Size(int w, int h) { Width = w; Height = h; }
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UpdateLayeredWindow(
            IntPtr hwnd, IntPtr hdcDst,
            ref W32Point pptDst, ref W32Size psize,
            IntPtr hdcSrc, ref W32Point pptSrc,
            uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hDC);

        #endregion

        public FormMain()
        {
            InitializeComponent();

            Pocket.Initialize();
            lastBallPosition = new Point(this.Width / 2, this.Height / 2);
            isDragging = false;
            isInitialized = true;

            this.Shown += (s, e) => UpdateOverlay();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED;
                return cp;
            }
        }

        private void UpdateOverlay()
        {
            if (!isInitialized || this.Width <= 0 || this.Height <= 0)
                return;

            Pocket.UpdatePoints(this.Width, this.Height);

            using (Bitmap bitmap = new Bitmap(this.Width, this.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);

                    DrawCorners(g);
                    DrawPockets(g);
                    DrawBall(g);
                    DrawGuideLines(g);
                }

                ApplyLayeredWindow(bitmap);
            }
        }

        private void ApplyLayeredWindow(Bitmap bitmap)
        {
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
            IntPtr oldBitmap = SelectObject(memDc, hBitmap);

            try
            {
                W32Point topPos = new W32Point(this.Left, this.Top);
                W32Size size = new W32Size(bitmap.Width, bitmap.Height);
                W32Point srcPos = new W32Point(0, 0);

                BLENDFUNCTION blend = new BLENDFUNCTION
                {
                    BlendOp = AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = AC_SRC_ALPHA
                };

                UpdateLayeredWindow(this.Handle, screenDc,
                    ref topPos, ref size,
                    memDc, ref srcPos,
                    0, ref blend, ULW_ALPHA);
            }
            finally
            {
                SelectObject(memDc, oldBitmap);
                DeleteObject(hBitmap);
                DeleteDC(memDc);
                ReleaseDC(IntPtr.Zero, screenDc);
            }
        }

        private void DrawCorners(Graphics g)
        {
            using (Pen pen = new Pen(Color.FromArgb(200, 0, 0, 255), CornerLineThickness))
            {
                DrawCorner(g, pen, PocketPosition.TopLeft, +1, +1);
                DrawCorner(g, pen, PocketPosition.BottomLeft, +1, -1);
                DrawCorner(g, pen, PocketPosition.TopRight, -1, +1);
                DrawCorner(g, pen, PocketPosition.BottomRight, -1, -1);
            }
        }

        private void DrawCorner(Graphics g, Pen pen, PocketPosition position, int dirX, int dirY)
        {
            Point reference = Pocket.GetPoint(position);
            Point coordHor = new Point(reference.X + dirX * CornerLineLength, reference.Y);
            Point coordVer = new Point(reference.X, reference.Y + dirY * CornerLineLength);
            g.DrawLines(pen, new[] { coordHor, reference, coordVer });
        }

        private void DrawPockets(Graphics g)
        {
            using (Pen pen = new Pen(Color.FromArgb(200, 255, 0, 0), PocketIndicatorSize))
            {
                foreach (PocketPosition position in Enum.GetValues(typeof(PocketPosition)))
                {
                    Point pt = Pocket.GetPoint(position);
                    int offsetX = GetPocketOffsetX(position);
                    int offsetY = GetPocketOffsetY(position);
                    g.DrawEllipse(pen, pt.X + offsetX, pt.Y + offsetY, PocketIndicatorSize, PocketIndicatorSize);
                }
            }
        }

        private int GetPocketOffsetX(PocketPosition position)
        {
            switch (position)
            {
                case PocketPosition.TopMiddle:
                case PocketPosition.BottomMiddle:
                    return -2;
                case PocketPosition.TopRight:
                case PocketPosition.BottomRight:
                    return -4;
                default:
                    return 0;
            }
        }

        private int GetPocketOffsetY(PocketPosition position)
        {
            switch (position)
            {
                case PocketPosition.BottomLeft:
                case PocketPosition.BottomMiddle:
                case PocketPosition.BottomRight:
                    return -4;
                default:
                    return 0;
            }
        }

        private void DrawBall(Graphics g)
        {
            Point pt = lastBallPosition;
            int halfBall = ReferenceBallSize / 2;
            int halfDot = BallCenterDotSize / 2;

            Rectangle rectOutside = new Rectangle(pt.X - halfBall, pt.Y - halfBall, ReferenceBallSize, ReferenceBallSize);
            Rectangle rectInside = new Rectangle(pt.X - halfDot, pt.Y - halfDot, BallCenterDotSize, BallCenterDotSize);

            // Fill the ball area so the entire circle receives mouse events (per-pixel alpha hit test)
            using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(1, 144, 144, 144)))
            {
                g.FillEllipse(fillBrush, rectOutside);
            }

            using (Pen penOutside = new Pen(Color.FromArgb(180, 144, 144, 144), 2))
            using (Pen penInside = new Pen(Color.FromArgb(255, 0, 0, 0), 2))
            {
                g.DrawEllipse(penOutside, rectOutside);
                g.DrawEllipse(penInside, rectInside);
            }
        }

        private void DrawGuideLines(Graphics g)
        {
            using (Pen pen = new Pen(Color.FromArgb(150, 144, 144, 144), GuideLineThickness))
            {
                foreach (PocketPosition position in Enum.GetValues(typeof(PocketPosition)))
                {
                    g.DrawLine(pen, lastBallPosition, Pocket.GetPoint(position));
                }
            }
        }

        private void FormMain_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            Rectangle hitArea = new Rectangle(
                lastBallPosition.X - BallHitAreaRadius,
                lastBallPosition.Y - BallHitAreaRadius,
                BallHitAreaRadius * 2,
                BallHitAreaRadius * 2);

            if (hitArea.Contains(e.X, e.Y))
            {
                isDragging = true;
                lastBallPosition = new Point(e.X, e.Y);
                UpdateOverlay();
            }
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                lastBallPosition = new Point(e.X, e.Y);
                UpdateOverlay();
            }
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampBallPosition();
            UpdateOverlay();
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            UpdateOverlay();
        }

        private void ClampBallPosition()
        {
            int x = Math.Max(0, Math.Min(lastBallPosition.X, this.ClientSize.Width));
            int y = Math.Max(0, Math.Min(lastBallPosition.Y, this.ClientSize.Height));
            lastBallPosition = new Point(x, y);
        }
    }
}
