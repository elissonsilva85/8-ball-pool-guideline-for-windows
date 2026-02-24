using System;
using System.Drawing;

namespace _8BallPool
{
    static class Pocket
    {
        private const int TotalPockets = 6;
        private const int DrawMargin = 10;
        private const int AdjustX = 15;
        private const int AdjustY = 40;

        private static Point[] pocketPoints;

        public static void Initialize()
        {
            pocketPoints = new Point[TotalPockets];
        }

        public static void UpdatePoints(int width, int height)
        {
            pocketPoints[(int)PocketPosition.TopLeft] = new Point(DrawMargin, DrawMargin);
            pocketPoints[(int)PocketPosition.TopMiddle] = new Point(width / 2 - 5, DrawMargin);
            pocketPoints[(int)PocketPosition.TopRight] = new Point(width - DrawMargin - AdjustX, DrawMargin);
            pocketPoints[(int)PocketPosition.BottomLeft] = new Point(DrawMargin, height - DrawMargin - AdjustY);
            pocketPoints[(int)PocketPosition.BottomMiddle] = new Point(width / 2 - 5, height - DrawMargin - AdjustY);
            pocketPoints[(int)PocketPosition.BottomRight] = new Point(width - DrawMargin - AdjustX, height - DrawMargin - AdjustY);
        }

        public static Point GetPoint(PocketPosition position)
        {
            return pocketPoints[(int)position];
        }
    }
}
