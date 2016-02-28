using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _8BallPool
{
    public partial class FormMain : Form
    {
        private int refferBallSize = 18;
        private Point lastBallPosition;
        private System.Windows.Forms.MouseButtons lastMouseButton;

        public FormMain()
        {
            InitializeComponent();

            Role.Initialize();

            lastBallPosition = new Point(this.Width / 2, this.Height / 2);
            lastMouseButton = System.Windows.Forms.MouseButtons.None;
        }

        private void FormMain_Paint(object sender, PaintEventArgs e)
        {
            Role.UpdatePoints(this);

            DrawElements();
        }

        private void DrawElements()
        {
            Graphics g = this.CreateGraphics();

            g.Clear(Color.White);

            this.Text = "8 Ball Pool Hack (" + this.Width + "x" + this.Height + ")";

            DrawCorners(g);

            DrawRoles(g);

            DrawBall(g);

            DrawGuideLines(g);

            g.Dispose();
        }

        private void DrawCorners(Graphics g)
        {
            Point referencePoint, coordHor, coordVer;

            int lineSize = 20;
            int lineHeight = 4;

            Pen myPen = new Pen(Color.FromArgb(128, 0, 0, 255), lineHeight);

            // top left
            referencePoint = Role.GetPoint(RolePosition.TopLeft);
            coordHor = referencePoint;
            coordHor.X += lineSize;
            coordVer = referencePoint;
            coordVer.Y += lineSize;

            g.DrawLines(myPen, new Point[] { coordHor, referencePoint, coordVer });

            // bottom left
            referencePoint = Role.GetPoint(RolePosition.BottomLeft);
            coordHor = referencePoint;
            coordHor.X += lineSize;
            coordVer = referencePoint;
            coordVer.Y -= lineSize;

            g.DrawLines(myPen, new Point[] { coordHor, referencePoint, coordVer });

            // top right
            referencePoint = Role.GetPoint(RolePosition.TopRight);
            coordHor = referencePoint;
            coordHor.X -= lineSize;
            coordVer = referencePoint;
            coordVer.Y += lineSize;

            g.DrawLines(myPen, new Point[] { coordHor, referencePoint, coordVer });

            // bottom right
            referencePoint = Role.GetPoint(RolePosition.BottomRight);
            coordHor = referencePoint;
            coordHor.X -= lineSize;
            coordVer = referencePoint;
            coordVer.Y -= lineSize;

            g.DrawLines(myPen, new Point[] { coordHor, referencePoint, coordVer });

        }

        private void DrawRoles(Graphics g)
        {
            int size = 3;
            Pen myPen = new Pen(Color.FromArgb(128, 255, 0, 0), size);

            g.DrawEllipse(myPen, Role.GetPoint(RolePosition.TopLeft).X, Role.GetPoint(RolePosition.TopLeft).Y, size, size);
            g.DrawEllipse(myPen, Role.GetPoint(RolePosition.TopMiddle).X - 2, Role.GetPoint(RolePosition.TopMiddle).Y, size, size);
            g.DrawEllipse(myPen, Role.GetPoint(RolePosition.TopRight).X - 4, Role.GetPoint(RolePosition.TopRight).Y, size, size);

            g.DrawEllipse(myPen, Role.GetPoint(RolePosition.BottomLeft).X, Role.GetPoint(RolePosition.BottomLeft).Y - 4, size, size);
            g.DrawEllipse(myPen, Role.GetPoint(RolePosition.BottomMiddle).X - 2, Role.GetPoint(RolePosition.BottomMiddle).Y - 4, size, size);
            g.DrawEllipse(myPen, Role.GetPoint(RolePosition.BottomRight).X - 4, Role.GetPoint(RolePosition.BottomRight).Y - 4, size, size);

        }

        private void DrawBall(Graphics g)
        {
            int ballSizeInside = 2;
            Pen myPenOutside = new Pen(Color.FromArgb(60, 144, 144, 144), 1);
            Pen myPenInside = new Pen(Color.FromArgb(250, 0, 0, 0), 2);

            Point pt = lastBallPosition;
            Rectangle rectOutisde = new Rectangle(pt.X - refferBallSize / 2, pt.Y - refferBallSize / 2, refferBallSize, refferBallSize);
            Rectangle rectInside = new Rectangle(pt.X - ballSizeInside / 2, pt.Y - ballSizeInside / 2, ballSizeInside, ballSizeInside);

            g.DrawEllipse(myPenOutside, rectOutisde);
            g.DrawEllipse(myPenInside, rectInside);
        }

        private void DrawGuideLines(Graphics g)
        {
            Pen myPen = new Pen(Color.FromArgb(60, 144, 144, 144), 1);

            g.DrawLine(myPen, lastBallPosition, Role.GetPoint(RolePosition.TopRight));
            g.DrawLine(myPen, lastBallPosition, Role.GetPoint(RolePosition.TopMiddle));
            g.DrawLine(myPen, lastBallPosition, Role.GetPoint(RolePosition.TopLeft));

            g.DrawLine(myPen, lastBallPosition, Role.GetPoint(RolePosition.BottomRight));
            g.DrawLine(myPen, lastBallPosition, Role.GetPoint(RolePosition.BottomMiddle));
            g.DrawLine(myPen, lastBallPosition, Role.GetPoint(RolePosition.BottomLeft));
        }

        private void FormMain_MouseUp(object sender, MouseEventArgs e)
        {
            lastMouseButton = System.Windows.Forms.MouseButtons.None;
        }

        private void FormMain_MouseDown(object sender, MouseEventArgs e)
        {
            Rectangle rect = new Rectangle(lastBallPosition.X - 5, lastBallPosition.Y - 5, 10, 10);

            if (rect.Contains(e.X, e.Y))
            {
                lastMouseButton = e.Button;
                lastBallPosition = new Point(e.X, e.Y);

                Graphics g = this.CreateGraphics();

                DrawBall(g);
            }
        }

        private void FormMain_MouseMove(object sender, MouseEventArgs e)
        {
            Rectangle rect = new Rectangle(lastBallPosition.X - 5, lastBallPosition.Y - 5, 10, 10);

            if (rect.Contains(e.X, e.Y) || lastMouseButton == System.Windows.Forms.MouseButtons.Left)
            {
                Cursor.Current = Cursors.Hand;
                if (lastMouseButton == System.Windows.Forms.MouseButtons.Left)
                    lastBallPosition = new Point(e.X, e.Y);
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }

            DrawElements();
        }        

    }
}
