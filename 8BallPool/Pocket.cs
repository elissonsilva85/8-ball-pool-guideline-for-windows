namespace EightBallPool;

/// <summary>Computes where the six pockets are, given the table area the overlay covers.</summary>
internal static class Pocket
{
    public static PocketPosition[] All { get; } = Enum.GetValues<PocketPosition>();

    public static Point GetPoint(PocketPosition position, Rectangle table)
    {
        int centerX = table.Left + table.Width / 2;

        return position switch
        {
            PocketPosition.TopLeft => new Point(table.Left, table.Top),
            PocketPosition.TopMiddle => new Point(centerX, table.Top),
            PocketPosition.TopRight => new Point(table.Right, table.Top),
            PocketPosition.BottomLeft => new Point(table.Left, table.Bottom),
            PocketPosition.BottomMiddle => new Point(centerX, table.Bottom),
            PocketPosition.BottomRight => new Point(table.Right, table.Bottom),
            _ => throw new ArgumentOutOfRangeException(nameof(position), position, null),
        };
    }

    /// <summary>Direction the corner bracket arms point to (towards the table interior); (0, 0) for middle pockets.</summary>
    public static (int X, int Y) InwardDirection(PocketPosition position) => position switch
    {
        PocketPosition.TopLeft => (+1, +1),
        PocketPosition.TopRight => (-1, +1),
        PocketPosition.BottomLeft => (+1, -1),
        PocketPosition.BottomRight => (-1, -1),
        _ => (0, 0),
    };
}
