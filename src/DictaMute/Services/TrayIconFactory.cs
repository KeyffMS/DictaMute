using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace DictaMute.Services;

internal enum TrayIconState
{
    Ready,
    Active,
    Disabled,
    Error,
}

internal static class TrayIconFactory
{
    private static readonly Color WindowBackground = Color.FromArgb(20, 23, 31);
    private static readonly Color Surface = Color.FromArgb(29, 34, 45);
    private static readonly Color Border = Color.FromArgb(54, 63, 81);
    private static readonly Color TextSecondary = Color.FromArgb(190, 200, 216);
    private static readonly Color TextMuted = Color.FromArgb(151, 164, 184);
    private static readonly Color Accent = Color.FromArgb(112, 139, 255);
    private static readonly Color AccentHover = Color.FromArgb(130, 154, 255);
    private static readonly Color Success = Color.FromArgb(77, 211, 169);
    private static readonly Color Danger = Color.FromArgb(255, 111, 128);

    public static Icon Create(TrayIconState state)
    {
        using var bitmap = new Bitmap(32, 32, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.Clear(Color.Transparent);

        using (var tile = RoundedRectangle(new RectangleF(2, 2, 28, 28), 7))
        using (var background = new SolidBrush(Surface))
        using (var border = new Pen(Border, 1.2f))
        {
            graphics.FillPath(background, tile);
            graphics.DrawPath(border, tile);
        }

        var dimmed = state == TrayIconState.Disabled;
        using (var microphone = RoundedRectangle(new RectangleF(12, 7, 8, 12), 4))
        using (var microphoneBrush = new SolidBrush(dimmed ? TextMuted : Accent))
        {
            graphics.FillPath(microphoneBrush, microphone);
        }

        using (var stemBrush = new SolidBrush(dimmed ? TextMuted : Accent))
        {
            graphics.FillRectangle(stemBrush, 15, 18, 2, 6);
        }

        using (var voicePen = new Pen(dimmed ? Border : Success, 1.8f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        })
        {
            graphics.DrawArc(voicePen, 9, 12, 14, 11, 0, 180);
        }

        using (var basePen = new Pen(dimmed ? TextMuted : TextSecondary, 1.8f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        })
        {
            graphics.DrawLine(basePen, 12, 25, 20, 25);
        }

        using (var mutePen = new Pen(dimmed ? TextMuted : Danger, 2.8f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
        })
        {
            graphics.DrawLine(mutePen, 23, 8, 8, 23);
        }

        var statusColor = state switch
        {
            TrayIconState.Ready => Success,
            TrayIconState.Active => AccentHover,
            TrayIconState.Error => Danger,
            _ => TextMuted,
        };
        using (var statusOutline = new SolidBrush(WindowBackground))
        using (var statusBrush = new SolidBrush(statusColor))
        {
            graphics.FillEllipse(statusOutline, 22, 22, 8, 8);
            graphics.FillEllipse(statusBrush, 23.5f, 23.5f, 5, 5);
        }

        var handle = bitmap.GetHicon();
        try
        {
            using var borrowed = Icon.FromHandle(handle);
            return (Icon)borrowed.Clone();
        }
        finally
        {
            DestroyIcon(handle);
        }
    }

    private static GraphicsPath RoundedRectangle(RectangleF rectangle, float radius)
    {
        var diameter = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rectangle.Left, rectangle.Top, diameter, diameter, 180, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Top, diameter, diameter, 270, 90);
        path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rectangle.Left, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DestroyIcon(nint handle);
}
