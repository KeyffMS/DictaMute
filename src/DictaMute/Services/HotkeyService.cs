using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Keys = System.Windows.Forms.Keys;

namespace DictaMute.Services;

internal sealed class HotkeyService : IDisposable
{
    private readonly IntPtr _window;
    private readonly HwndSource _source;
    private HotkeySettings? _settings;
    private readonly List<int> _registered = [];
    public event Action<int>? Pressed;

    public HotkeyService(IntPtr window)
    {
        _window = window;
        _source = HwndSource.FromHwnd(window) ?? throw new InvalidOperationException("Brak uchwytu okna.");
        _source.AddHook(WndProc);
    }

    public void Configure(HotkeySettings settings)
    {
        var values = new[] { settings.AddSource, settings.AddTarget, settings.Toggle }.Select(Parse).ToArray();
        if (values.Distinct().Count() != 3) throw new ArgumentException("Skróty muszą być różne.");
        var previous = _settings;
        UnregisterAll();
        try
        {
            Register(values);
            _settings = settings;
        }
        catch
        {
            UnregisterAll();
            if (previous is not null)
            {
                try { Register(new[] { previous.AddSource, previous.AddTarget, previous.Toggle }.Select(Parse).ToArray()); }
                catch (Exception ex) { UnregisterAll(); Log.Write("Nie udało się przywrócić skrótów.", ex); }
            }
            throw;
        }
    }

    private void Register((uint Modifiers, uint Key)[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            if (!RegisterHotKey(_window, i + 1, values[i].Modifiers | 0x4000, values[i].Key))
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Nie można zarejestrować skrótu nr {i + 1}. Może być używany przez inny program.");
            _registered.Add(i + 1);
        }
    }

    public static (uint Modifiers, uint Key) Parse(string text)
    {
        uint modifiers = 0;
        uint key = 0;
        foreach (var part in text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            switch (part.ToUpperInvariant())
            {
                case "CTRL": case "CONTROL": modifiers |= 2; break;
                case "ALT": modifiers |= 1; break;
                case "SHIFT": modifiers |= 4; break;
                case "WIN": modifiers |= 8; break;
                default:
                    if (key != 0 || !Enum.TryParse<Keys>(part, true, out var parsed) ||
                        (int)parsed is <= 0 or > 255 || parsed is Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LWin or Keys.RWin)
                        throw new ArgumentException($"Nieprawidłowy skrót: {text}. Przykład: Ctrl+Alt+X.");
                    key = (uint)parsed;
                    break;
            }
        }
        if (key == 0 || modifiers == 0) throw new ArgumentException($"Skrót wymaga modyfikatora i klawisza: {text}.");
        return (modifiers, key);
    }

    private IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == 0x0312 && _registered.Contains(wParam.ToInt32()))
        {
            handled = true;
            Pressed?.Invoke(wParam.ToInt32());
        }
        return IntPtr.Zero;
    }

    private void UnregisterAll()
    {
        foreach (var id in _registered) UnregisterHotKey(_window, id);
        _registered.Clear();
    }

    public void Dispose() { UnregisterAll(); _source.RemoveHook(WndProc); }
    [DllImport("user32.dll", SetLastError = true)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(IntPtr window, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(IntPtr window, int id);
}
