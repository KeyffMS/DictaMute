using System.Drawing;
using System.Drawing.Drawing2D;
using Forms = System.Windows.Forms;
using Wpf = System.Windows;
using Media = System.Windows.Media;

namespace DictaMute.Services;

// The tray reads the same semantic palette as WPF; no second set of hex values.
internal sealed class SuiteTrayTheme : Forms.ToolStripProfessionalRenderer, IDisposable
{
    private readonly Color _surface = ColorFor("SurfaceColor");
    private readonly Color _hover = ColorFor("SurfaceHoverColor");
    private readonly Color _border = ColorFor("BorderColor");
    private readonly Color _text = ColorFor("TextPrimaryColor");
    private readonly Color _muted = ColorFor("TextMutedColor");
    private readonly Color _accent = ColorFor("AccentColor");
    private readonly Color _danger = ColorFor("DangerColor");
    private readonly Font _font = new("Segoe UI", 10f, FontStyle.Regular, GraphicsUnit.Point);

    internal void Apply(Forms.ToolStripDropDown menu)
    {
        menu.Renderer = this;
        menu.BackColor = _surface;
        menu.ForeColor = _text;
        menu.Font = _font;
        menu.Padding = new Forms.Padding(8);
        menu.MinimumSize = new Size(320, 0);
        if (menu is Forms.ToolStripDropDownMenu dropdown)
        {
            dropdown.ShowImageMargin = false;
            dropdown.ShowCheckMargin = true;
        }
        StyleItems(menu);
        menu.Opening += (_, _) => StyleItems(menu);
    }

    private void StyleItems(Forms.ToolStripDropDown menu)
    {
        foreach (Forms.ToolStripItem item in menu.Items)
        {
            item.Font = _font;
            item.ForeColor = item.Text == "Zakończ" ? _danger : _text;
            if (item is Forms.ToolStripSeparator) continue;
            item.Padding = new Forms.Padding(10, 6, 10, 6);
            if (item is Forms.ToolStripMenuItem parent)
            {
                parent.DropDown.Renderer = this;
                parent.DropDown.BackColor = _surface;
                parent.DropDown.ForeColor = _text;
                parent.DropDown.Font = _font;
                parent.DropDown.Padding = new Forms.Padding(8);
                parent.DropDown.MinimumSize = new Size(260, 0);
                if (parent.HasDropDownItems) StyleItems(parent.DropDown);
            }
        }
    }

    protected override void OnRenderToolStripBackground(Forms.ToolStripRenderEventArgs e)
    { e.Graphics.Clear(_surface); }

    protected override void OnRenderImageMargin(Forms.ToolStripRenderEventArgs e)
    { using var brush = new SolidBrush(_surface); e.Graphics.FillRectangle(brush, e.AffectedBounds); }

    protected override void OnRenderToolStripBorder(Forms.ToolStripRenderEventArgs e)
    { using var pen = new Pen(_border); e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1); }

    protected override void OnRenderMenuItemBackground(Forms.ToolStripItemRenderEventArgs e)
    {
        if (!e.Item.Selected || !e.Item.Enabled) return;
        var rect = new Rectangle(2, 1, Math.Max(1, e.Item.Width - 4), Math.Max(1, e.Item.Height - 2));
        using var path = RoundedRectangle(rect, 7);
        using var brush = new SolidBrush(_hover);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.FillPath(brush, path);
    }

    protected override void OnRenderItemText(Forms.ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = !e.Item.Enabled ? _muted : e.Item.Text == "Zakończ" ? _danger : _text;
        base.OnRenderItemText(e);
    }

    protected override void OnRenderArrow(Forms.ToolStripArrowRenderEventArgs e)
    { e.ArrowColor = e.Item?.Enabled == true ? _text : _muted; base.OnRenderArrow(e); }

    protected override void OnRenderItemCheck(Forms.ToolStripItemImageRenderEventArgs e)
    {
        using var pen = new Pen(_accent, 2);
        var rect = e.ImageRectangle;
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawLines(pen, [new Point(rect.Left + 2, rect.Top + rect.Height / 2), new Point(rect.Left + 6, rect.Bottom - 3), new Point(rect.Right - 1, rect.Top + 3)]);
    }

    protected override void OnRenderSeparator(Forms.ToolStripSeparatorRenderEventArgs e)
    { using var pen = new Pen(_border); e.Graphics.DrawLine(pen, 10, e.Item.Height / 2, e.Item.Width - 10, e.Item.Height / 2); }

    private static Color ColorFor(string key)
    {
        var value = (Media.Color)Wpf.Application.Current.FindResource(key);
        return Color.FromArgb(value.A, value.R, value.G, value.B);
    }

    private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var size = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, size, size, 180, 90);
        path.AddArc(bounds.Right - size, bounds.Top, size, size, 270, 90);
        path.AddArc(bounds.Right - size, bounds.Bottom - size, size, size, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - size, size, size, 90, 90);
        path.CloseFigure();
        return path;
    }

    public void Dispose() => _font.Dispose();
}
