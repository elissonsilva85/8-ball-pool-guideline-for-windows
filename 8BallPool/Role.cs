using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            // Lista de posicoes
            List<int> posicoes = Enum.GetValues(typeof(RolePosition)).Cast<RolePosition>().Select(v => (int)v).ToList();

            rolesPoints = new Point[posicoes.Count];

            // Inicalizando posicoes no vetor
            foreach (int posicao in posicoes)
                rolesPoints[posicao] = new Point();
        }

        public static void UpdatePoints(Form frm)
        {
            int ajusteX = 15;
            int ajusteY = 40;

            rolesPoints[(int)RolePosition.TopLeft] = new Point(drawMargin, drawMargin);
            rolesPoints[(int)RolePosition.TopMiddle] = new Point(frm.Width / 2 - 5, drawMargin);
            rolesPoints[(int)RolePosition.TopRight] = new Point(frm.Width - drawMargin - ajusteX, drawMargin);

            rolesPoints[(int)RolePosition.BottomLeft] = new Point(drawMargin, frm.Height - drawMargin - ajusteY);
            rolesPoints[(int)RolePosition.BottomMiddle] = new Point(frm.Width / 2 - 5, frm.Height - drawMargin - ajusteY);
            rolesPoints[(int)RolePosition.BottomRight] = new Point(frm.Width - drawMargin - ajusteX, frm.Height - drawMargin - ajusteY);
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
