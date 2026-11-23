using System.Runtime.InteropServices;

namespace EightBallPool;

/// <summary>Win32 interop, using source-generated P/Invoke stubs.</summary>
internal static partial class NativeMethods
{
    public const int WS_EX_LAYERED = 0x0008_0000;
    public const int WS_EX_TRANSPARENT = 0x0000_0020;
    public const int WS_EX_TOOLWINDOW = 0x0000_0080;

    public const int WM_NCHITTEST = 0x0084;
    public const int WM_HOTKEY = 0x0312;

    public const int HTCLIENT = 1;
    public const int HTCAPTION = 2;
    public const int HTLEFT = 10;
    public const int HTRIGHT = 11;
    public const int HTTOP = 12;
    public const int HTTOPLEFT = 13;
    public const int HTTOPRIGHT = 14;
    public const int HTBOTTOM = 15;
    public const int HTBOTTOMLEFT = 16;
    public const int HTBOTTOMRIGHT = 17;

    public const uint MOD_NOREPEAT = 0x4000;

    private const uint ULW_ALPHA = 0x02;
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
    private struct POINT(int x, int y)
    {
        public int X = x;
        public int Y = y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SIZE(int cx, int cy)
    {
        public int CX = cx;
        public int CY = cy;
    }

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool UpdateLayeredWindow(
        IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize,
        IntPtr hdcSrc, ref POINT pptSrc, uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);

    [LibraryImport("gdi32.dll", SetLastError = true)]
    private static partial IntPtr CreateCompatibleDC(IntPtr hdc);

    [LibraryImport("gdi32.dll")]
    private static partial IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteObject(IntPtr hObject);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool DeleteDC(IntPtr hdc);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool UnregisterHotKey(IntPtr hWnd, int id);

    /// <summary>
    /// Pushes a 32bpp ARGB bitmap as the full content of a layered window (per-pixel alpha).
    /// Pixels with alpha 0 are not hit-tested, so clicks pass through them.
    /// </summary>
    public static void UpdateLayeredWindow(IntPtr hwnd, Bitmap bitmap, Point screenLocation)
    {
        IntPtr memDc = CreateCompatibleDC(IntPtr.Zero);
        IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
        IntPtr oldBitmap = SelectObject(memDc, hBitmap);

        try
        {
            var dst = new POINT(screenLocation.X, screenLocation.Y);
            var size = new SIZE(bitmap.Width, bitmap.Height);
            var src = new POINT(0, 0);
            var blend = new BLENDFUNCTION
            {
                BlendOp = AC_SRC_OVER,
                SourceConstantAlpha = 255,
                AlphaFormat = AC_SRC_ALPHA,
            };

            UpdateLayeredWindow(hwnd, IntPtr.Zero, ref dst, ref size, memDc, ref src, 0, ref blend, ULW_ALPHA);
        }
        finally
        {
            SelectObject(memDc, oldBitmap);
            DeleteObject(hBitmap);
            DeleteDC(memDc);
        }
    }
}
