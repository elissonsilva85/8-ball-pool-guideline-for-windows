using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;

namespace _8BallPool
{
    static class Role
    {
        private static Point[] rolesPoints;
        private static int drawMargin = 10;

        public static void Initialize()
        {
            List<int> positions = Enum.GetValues(typeof(RolePosition)).Cast<RolePosition>().Select(v => (int)v).ToList();
            rolesPoints = new Point[positions.Count];
            foreach (int position in positions)
                rolesPoints[position] = new Point();
        }

        public static void UpdatePoints(Form frm)
        {
            int adjustX = 15;
            int adjustY = 40;
            rolesPoints[(int)RolePosition.TopLeft] = new Point(drawMargin, drawMargin);
            rolesPoints[(int)RolePosition.TopMiddle] = new Point(frm.Width / 2 - 5, drawMargin);
            rolesPoints[(int)RolePosition.TopRight] = new Point(frm.Width - drawMargin - adjustX, drawMargin);
            rolesPoints[(int)RolePosition.BottomLeft] = new Point(drawMargin, frm.Height - drawMargin - adjustY);
            rolesPoints[(int)RolePosition.BottomMiddle] = new Point(frm.Width / 2 - 5, frm.Height - drawMargin - adjustY);
            rolesPoints[(int)RolePosition.BottomRight] = new Point(frm.Width - drawMargin - adjustX, frm.Height - drawMargin - adjustY);
        }

        public static void SetPoint(RolePosition rp, Point pt)
        {
            rolesPoints[(int)rp] = pt;
        }

        public static Point GetPoint(RolePosition rp)
        {
            return rolesPoints[(int)rp];
        }
    }
}
