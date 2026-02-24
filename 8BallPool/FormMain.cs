using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private bool isTransparent;

        public FormMain()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);

            Pocket.Initialize();
            lastBallPosition = new Point(this.Width / 2, this.Height / 2);
            isDragging = false;
            isTransparent = true;
        }

        private void FormMain_Paint(object sender, PaintEventArgs e)
        {
            Pocket.UpdatePoints(this.Width, this.Height);
            this.Text = "8 Ball Pool Guidelines (" + this.Width + "x" + this.Height + ")";

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            DrawCorners(g);
            DrawPockets(g);
            DrawBall(g);
            DrawGuideLines(g);
        }

        private void DrawCorners(Graphics g)
        {
            using (Pen pen = new Pen(Color.FromArgb(128, 0, 0, 255), CornerLineThickness))
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
            using (Pen pen = new Pen(Color.FromArgb(128, 255, 0, 0), PocketIndicatorSize))
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

            using (Pen penOutside = new Pen(Color.FromArgb(60, 144, 144, 144), 1))
            using (Pen penInside = new Pen(Color.FromArgb(250, 0, 0, 0), 2))
            {
                g.DrawEllipse(penOutside, rectOutside);
                g.DrawEllipse(penInside, rectInside);
            }
        }

        private void DrawGuideLines(Graphics g)
        {
            using (Pen pen = new Pen(Color.FromArgb(60, 144, 144, 144), GuideLineThickness))
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
                this.Invalidate();
            }
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle hitArea = new Rectangle(
                lastBallPosition.X - BallHitAreaRadius,
                lastBallPosition.Y - BallHitAreaRadius,
                BallHitAreaRadius * 2,
                BallHitAreaRadius * 2);

            if (hitArea.Contains(e.X, e.Y) || isDragging)
            {
                Cursor.Current = Cursors.Hand;
                if (isDragging)
                    lastBallPosition = new Point(e.X, e.Y);
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }

            this.Invalidate();
        }

        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                isTransparent = !isTransparent;
                if (isTransparent)
                {
                    this.TransparencyKey = Color.Empty;
                    this.Opacity = 0.7D;
                }
                else
                {
                    this.TransparencyKey = this.BackColor;
                    this.Opacity = 1.0D;
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampBallPosition();
            this.Invalidate();
        }

        private void ClampBallPosition()
        {
            int x = Math.Max(0, Math.Min(lastBallPosition.X, this.ClientSize.Width));
            int y = Math.Max(0, Math.Min(lastBallPosition.Y, this.ClientSize.Height));
            lastBallPosition = new Point(x, y);
        }
    }
}
