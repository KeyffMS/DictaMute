using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using DictaMute.Services;

namespace DictaMute;

// Presentation-only companion: no changes to audio rules, persistence or hotkeys.
public partial class MainWindow
{
    private SuiteTrayTheme? _suiteTrayTheme;
    private bool _presentationAttached;

    private void Presentation_SourceInitialized(object? sender, EventArgs e)
    {
        // MainWindow is a derived Window type; apply the shared base style explicitly.
        SetResourceReference(StyleProperty, typeof(Window));
        var enabled = 1;
        var handle = new WindowInteropHelper(this).Handle;
        // Keep native resize, snap and accessibility; only request a dark title bar.
        if (DwmSetWindowAttribute(handle, 20, ref enabled, sizeof(int)) != 0)
            DwmSetWindowAttribute(handle, 19, ref enabled, sizeof(int));
        var work = SystemParameters.WorkArea;
        MinWidth = Math.Min(MinWidth, work.Width);
        MinHeight = Math.Min(MinHeight, work.Height);
        Width = Math.Min(Width, work.Width);
        Height = Math.Min(Height, work.Height);
    }

    private void Presentation_Loaded(object sender, RoutedEventArgs e)
    {
        if (_presentationAttached) return;
        _presentationAttached = true;
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        VersionText.Text = version is null ? string.Empty : $"v{version.Major}.{version.Minor}.{version.Build}";
        if (_tray.ContextMenuStrip is { } menu)
        {
            _suiteTrayTheme = new SuiteTrayTheme();
            _suiteTrayTheme.Apply(menu);
        }
        _engine.Updated += UpdateSuiteStatus;
    }

    private void UpdateSuiteStatus(EngineSnapshot snapshot)
    {
        if (_quitting || Dispatcher.HasShutdownStarted) return;
        Dispatcher.InvokeAsync(() =>
        {
            if (_quitting) return;
            var faulted = _faulted || snapshot.FatalError is not null;
            var background = faulted ? "DangerSoftBrush" : !snapshot.Enabled ? "SurfaceRaisedBrush"
                : snapshot.Active ? "AccentSoftBrush" : "SuccessSoftBrush";
            var foreground = faulted ? "DangerBrush" : !snapshot.Enabled ? "TextSecondaryBrush"
                : snapshot.Active ? "AccentHoverBrush" : "SuccessBrush";
            AutomationStateBadge.Background = (Brush)FindResource(background);
            AutomationStateText.Foreground = (Brush)FindResource(foreground);
            AutomationStateText.Text = faulted ? "BŁĄD" : !snapshot.Enabled ? "WYŁĄCZONA"
                : snapshot.Active ? "WYCISZANIE" : "GOTOWA";
        });
    }

    private void Presentation_Closed(object? sender, EventArgs e)
    {
        if (_presentationAttached) _engine.Updated -= UpdateSuiteStatus;
        _suiteTrayTheme?.Dispose();
    }

    private void ProjectLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        if (e.Uri.Scheme != Uri.UriSchemeHttps || e.Uri.Host != "github.com") return;
        try { Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true }); }
        catch (Exception ex) { Report(ex); }
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);
}
