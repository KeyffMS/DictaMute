using System.Windows;
using DictaMute.Services;

namespace DictaMute;

public partial class App : System.Windows.Application
{
    private Mutex? _singleInstance;
    private bool _ownsMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _singleInstance = new Mutex(true, @"Local\DictaMute", out _ownsMutex);
        if (!_ownsMutex)
        {
            MessageBox.Show("DictaMute już działa. Otwórz jego okno przez ikonę obok zegara.", "DictaMute");
            Shutdown();
            return;
        }
        DispatcherUnhandledException += async (_, args) =>
        {
            args.Handled = true;
            Log.Write("Nieobsłużony błąd interfejsu.", args.Exception);
            MessageBox.Show("Wystąpił błąd. DictaMute przywróci dźwięk i zakończy działanie.\n" + args.Exception.Message,
                "DictaMute", MessageBoxButton.OK, MessageBoxImage.Error);
            if (MainWindow is DictaMute.MainWindow window) await window.ShutdownAsync();
            else Shutdown(1);
        };
        try
        {
            var store = new SettingsStore(Path.Combine(Log.DirectoryPath, "settings.json"));
            var settings = store.Load();
            var window = new MainWindow(settings, store);
            MainWindow = window;
            window.Show();
        }
        catch (Exception ex)
        {
            Log.Write("Uruchomienie aplikacji.", ex);
            MessageBox.Show("Nie można uruchomić DictaMute.\n" + ex.Message, "DictaMute", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
    {
        if (MainWindow is DictaMute.MainWindow window) window.StopForSessionEnding();
        base.OnSessionEnding(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_ownsMutex) _singleInstance?.ReleaseMutex();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
