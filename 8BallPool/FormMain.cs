using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using static EightBallPool.NativeMethods;

namespace EightBallPool;

/// <summary>
/// Borderless, always-on-top overlay rendered as a per-pixel-alpha layered window.
/// <list type="bullet">
/// <item><b>Setup mode</b>: the overlay is tinted and grabbable — drag to move, drag edges to resize, drag the ball to aim.</item>
/// <item><b>Play mode</b>: the whole window is click-through (WS_EX_TRANSPARENT) so the game receives every click.</item>
/// </list>
/// Global hotkeys work in both modes: F8 toggles the mode, F9 moves the reference ball to the mouse cursor.
/// </summary>
public partial class FormMain : Form
{
    private enum OverlayMode { Setup, Play }

    // Sizes in logical pixels (96 DPI); scaled with Scale().
    private const int TableMargin = 10;
    private const int BallRadius = 12;
    private const int BallHitRadius = 15;
    private const int BallCenterDotSize = 2;
    private const int CornerLineLength = 40;
    private const int CornerLineThickness = 4;
    private const int PocketIndicatorSize = 4;
    private const int GuideLineThickness = 3;
    private const int ResizeGripSize = 8;
    private const int HintFontSize = 12;

    private const int HotkeyToggleModeId = 1;
    private const int HotkeySnapBallId = 2;
    private const Keys HotkeyToggleMode = Keys.F8;
    private const Keys HotkeySnapBall = Keys.F9;

    private readonly NotifyIcon trayIcon;
    private readonly ToolStripMenuItem setupModeMenuItem;
    private readonly List<Keys> failedHotkeys = [];

    private OverlayMode mode = OverlayMode.Setup;
    private Point ballPosition;
    private bool isDragging;

    public FormMain()
    {
        InitializeComponent();

        components ??= new Container();

        setupModeMenuItem = new ToolStripMenuItem("Setup mode", null, (_, _) => ToggleMode())
        {
            ShortcutKeyDisplayString = HotkeyToggleMode.ToString(),
            Checked = true,
        };

        var trayMenu = new ContextMenuStrip(components);
        trayMenu.Items.Add(setupModeMenuItem);
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("Exit", null, (_, _) => Close());

        trayIcon = new NotifyIcon(components)
        {
            Icon = Icon,
            Text = Text,
            ContextMenuStrip = trayMenu,
            Visible = true,
        };
        trayIcon.DoubleClick += (_, _) => ToggleMode();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= WS_EX_LAYERED | WS_EX_TOOLWINDOW;
            if (mode == OverlayMode.Play)
                cp.ExStyle |= WS_EX_TRANSPARENT;
            return cp;
        }
    }

    private int Scale(int logicalPixels) => (int)Math.Round(logicalPixels * DeviceDpi / 96.0);

    private Rectangle TableBounds
    {
        get
        {
            int margin = Scale(TableMargin);
            return new Rectangle(margin, margin, ClientSize.Width - 2 * margin, ClientSize.Height - 2 * margin);
        }
    }

    #region Lifecycle

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        MinimumSize = new Size(Scale(200), Scale(120));
        ballPosition = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        RenderOverlay();

        string message = failedHotkeys.Count == 0
            ? $"Setup mode: position the overlay over the table. Press {HotkeyToggleMode} to play (click-through)."
            : $"Could not register hotkey(s) {string.Join(", ", failedHotkeys)} — another app is using them. Use this tray icon to switch modes.";
        trayIcon.ShowBalloonTip(5000, Text, message, failedHotkeys.Count == 0 ? ToolTipIcon.Info : ToolTipIcon.Warning);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        failedHotkeys.Clear();
        if (!RegisterHotKey(Handle, HotkeyToggleModeId, MOD_NOREPEAT, (uint)HotkeyToggleMode))
            failedHotkeys.Add(HotkeyToggleMode);
        if (!RegisterHotKey(Handle, HotkeySnapBallId, MOD_NOREPEAT, (uint)HotkeySnapBall))
            failedHotkeys.Add(HotkeySnapBall);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        UnregisterHotKey(Handle, HotkeyToggleModeId);
        UnregisterHotKey(Handle, HotkeySnapBallId);
        base.OnHandleDestroyed(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ballPosition = ClampToClient(ballPosition);
        RenderOverlay();
    }

    protected override void OnMove(EventArgs e)
    {
        base.OnMove(e);
        RenderOverlay();
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        MinimumSize = new Size(Scale(200), Scale(120));
        RenderOverlay();
    }

    #endregion

    #region Modes and hotkeys

    private void ToggleMode() => SetMode(mode == OverlayMode.Setup ? OverlayMode.Play : OverlayMode.Setup);

    private void SetMode(OverlayMode newMode)
    {
        if (mode == newMode)
            return;

        mode = newMode;
        isDragging = false;
        setupModeMenuItem.Checked = mode == OverlayMode.Setup;

        UpdateStyles(); // re-applies CreateParams, adding/removing WS_EX_TRANSPARENT
        RenderOverlay();

        if (mode == OverlayMode.Setup)
            Activate(); // so Esc reaches the window
    }

    private void SnapBallToCursor()
    {
        Point cursor = PointToClient(Cursor.Position);
        if (!ClientRectangle.Contains(cursor))
            return;

        ballPosition = cursor;
        RenderOverlay();
    }

    protected override void WndProc(ref Message m)
    {
        switch (m.Msg)
        {
            case WM_HOTKEY:
                switch ((int)m.WParam)
                {
                    case HotkeyToggleModeId: ToggleMode(); break;
                    case HotkeySnapBallId: SnapBallToCursor(); break;
                }
                return;

            case WM_NCHITTEST when mode == OverlayMode.Setup:
                // The window has no visible frame (a layered window draws only our bitmap),
                // so emulate one: edges resize, the ball is the client area, the rest moves the window.
                int lParam = unchecked((int)(long)m.LParam);
                var screenPoint = new Point((short)(lParam & 0xFFFF), (short)((lParam >> 16) & 0xFFFF));
                m.Result = HitTest(PointToClient(screenPoint));
                return;
        }

        base.WndProc(ref m);
    }

    private IntPtr HitTest(Point p)
    {
        if (IsOverBall(p))
            return HTCLIENT;

        int grip = Scale(ResizeGripSize);
        bool left = p.X < grip, right = p.X >= ClientSize.Width - grip;
        bool top = p.Y < grip, bottom = p.Y >= ClientSize.Height - grip;

        int result = (top, bottom, left, right) switch
        {
            (true, _, true, _) => HTTOPLEFT,
            (true, _, _, true) => HTTOPRIGHT,
            (_, true, true, _) => HTBOTTOMLEFT,
            (_, true, _, true) => HTBOTTOMRIGHT,
            (true, _, _, _) => HTTOP,
            (_, true, _, _) => HTBOTTOM,
            (_, _, true, _) => HTLEFT,
            (_, _, _, true) => HTRIGHT,
            _ => HTCAPTION,
        };
        return result;
    }

    #endregion

    #region Mouse and keyboard (setup mode only — in play mode no input reaches the window)

    private bool IsOverBall(Point p)
    {
        int dx = p.X - ballPosition.X, dy = p.Y - ballPosition.Y;
        int r = Scale(BallHitRadius);
        return dx * dx + dy * dy <= r * r;
    }

    private Point ClampToClient(Point p) => new(
        Math.Clamp(p.X, 0, Math.Max(0, ClientSize.Width - 1)),
        Math.Clamp(p.Y, 0, Math.Max(0, ClientSize.Height - 1)));

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && IsOverBall(e.Location))
        {
            isDragging = true;
            ballPosition = e.Location;
            RenderOverlay();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (isDragging)
        {
            ballPosition = ClampToClient(e.Location);
            RenderOverlay();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        isDragging = false;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode == Keys.Escape)
            Close();
    }

    #endregion

    #region Rendering

    private void RenderOverlay()
    {
        if (!IsHandleCreated || Width <= 0 || Height <= 0)
            return;

        using var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit; // ClearType breaks on transparent surfaces
            g.Clear(Color.Transparent);

            Rectangle table = TableBounds;

            if (mode == OverlayMode.Setup)
                DrawSetupChrome(g);

            DrawGuideLines(g, table);
            DrawCorners(g, table);
            DrawPockets(g, table);
            DrawBall(g);

            if (mode == OverlayMode.Setup)
                DrawHint(g, table);
        }

        UpdateLayeredWindow(Handle, bitmap, Location);
    }

    /// <summary>A light tint makes the whole window visible and hit-testable while positioning it.</summary>
    private void DrawSetupChrome(Graphics g)
    {
        using var tint = new SolidBrush(Color.FromArgb(50, 0, 0, 0));
        g.FillRectangle(tint, ClientRectangle);

        float thickness = Scale(2);
        using var border = new Pen(Color.FromArgb(180, 255, 255, 255), thickness) { DashStyle = DashStyle.Dash };
        g.DrawRectangle(border, thickness / 2, thickness / 2, ClientSize.Width - thickness, ClientSize.Height - thickness);
    }

    private void DrawGuideLines(Graphics g, Rectangle table)
    {
        using var pen = new Pen(Color.FromArgb(150, 144, 144, 144), Scale(GuideLineThickness));
        foreach (PocketPosition position in Pocket.All)
            g.DrawLine(pen, ballPosition, Pocket.GetPoint(position, table));
    }

    private void DrawCorners(Graphics g, Rectangle table)
    {
        int length = Scale(CornerLineLength);
        using var pen = new Pen(Color.FromArgb(200, 0, 0, 255), Scale(CornerLineThickness));

        foreach (PocketPosition position in Pocket.All)
        {
            var (dirX, dirY) = Pocket.InwardDirection(position);
            if (dirX == 0)
                continue;

            Point corner = Pocket.GetPoint(position, table);
            g.DrawLines(pen,
            [
                new Point(corner.X + dirX * length, corner.Y),
                corner,
                new Point(corner.X, corner.Y + dirY * length),
            ]);
        }
    }

    private void DrawPockets(Graphics g, Rectangle table)
    {
        int size = Scale(PocketIndicatorSize);
        using var pen = new Pen(Color.FromArgb(200, 255, 0, 0), Scale(2));

        foreach (PocketPosition position in Pocket.All)
        {
            Point p = Pocket.GetPoint(position, table);
            g.DrawEllipse(pen, p.X - size / 2, p.Y - size / 2, size, size);
        }
    }

    private void DrawBall(Graphics g)
    {
        int radius = Scale(BallRadius);
        int dot = Scale(BallCenterDotSize);
        var outer = new Rectangle(ballPosition.X - radius, ballPosition.Y - radius, radius * 2, radius * 2);
        var inner = new Rectangle(ballPosition.X - dot / 2, ballPosition.Y - dot / 2, dot, dot);

        using var outline = new Pen(Color.FromArgb(200, 144, 144, 144), Scale(2));
        using var center = new Pen(Color.Black, Scale(2));
        g.DrawEllipse(outline, outer);
        g.DrawEllipse(center, inner);
    }

    private void DrawHint(Graphics g, Rectangle table)
    {
        string hint =
            "SETUP — drag to move · drag edges to resize · drag the ball to aim\n" +
            $"{HotkeyToggleMode}: play mode (click-through)  ·  {HotkeySnapBall}: ball to cursor  ·  Esc: exit";

        using var font = new Font("Segoe UI", Scale(HintFontSize), GraphicsUnit.Pixel);
        using var format = new StringFormat { Alignment = StringAlignment.Center };

        SizeF textSize = g.MeasureString(hint, font, table.Width, format);
        int padding = Scale(6);
        var box = new Rectangle(
            table.Left + (table.Width - (int)textSize.Width) / 2 - padding,
            table.Top + Scale(CornerLineLength) / 2,
            (int)Math.Ceiling(textSize.Width) + padding * 2,
            (int)Math.Ceiling(textSize.Height) + padding * 2);

        using var background = new SolidBrush(Color.FromArgb(170, 0, 0, 0));
        g.FillRoundedRectangle(background, box, new Size(padding * 2, padding * 2));
        g.DrawString(hint, font, Brushes.White, Rectangle.Inflate(box, -padding, -padding), format);
    }

    #endregion
}
