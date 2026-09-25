using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32.SafeHandles;

namespace DictaMute.Services;

internal static class ProcessService
{
    public static AppIdentity? Foreground()
    {
        var window = GetForegroundWindow();
        if (window == IntPtr.Zero) return null;
        GetWindowThreadProcessId(window, out var id);
        return id == Environment.ProcessId ? null : Read((int)id);
    }

    public static AppIdentity? Read(int id)
    {
        if (id <= 0) return null;
        try
        {
            using var process = Process.GetProcessById(id);
            var name = process.ProcessName;
            string? path = null;
            string? appId = null;
            using var handle = OpenProcess(0x1000, false, id); // PROCESS_QUERY_LIMITED_INFORMATION
            if (!handle.IsInvalid)
            {
                var text = new StringBuilder(32768);
                var length = text.Capacity;
                if (QueryFullProcessImageName(handle, 0, text, ref length)) path = text.ToString();
                uint size = 0;
                if (GetApplicationUserModelId(handle, ref size, null) == 122 && size is > 0 and < 32768)
                {
                    var model = new StringBuilder((int)size);
                    if (GetApplicationUserModelId(handle, ref size, model) == 0) appId = model.ToString();
                }
            }
            return new(name, path, appId);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            return null; // Process exited or is not accessible to the current user.
        }
    }

    public static ImageSource? Icon(AppIdentity app)
    {
        if (string.IsNullOrWhiteSpace(app.ExecutablePath)) return null;
        try
        {
            using var icon = System.Drawing.Icon.ExtractAssociatedIcon(app.ExecutablePath);
            if (icon is null) return null;
            var image = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(20, 20));
            image.Freeze();
            return image;
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or System.ComponentModel.Win32Exception)
        {
            return null;
        }
    }

    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("kernel32.dll", SetLastError = true)] private static extern SafeProcessHandle OpenProcess(uint access,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool QueryFullProcessImageName(SafeProcessHandle process, int flags, StringBuilder name, ref int size);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetApplicationUserModelId(SafeProcessHandle process, ref uint length, StringBuilder? applicationId);
}
