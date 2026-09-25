using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using DictaMute.Services;
using Microsoft.Win32;
using Forms = System.Windows.Forms;

namespace DictaMute;

internal sealed class AppRow : INotifyPropertyChanged
{
    private double _peak;
    private string _status = "Nieaktywna";
    public AppIdentity App { get; }
    public string Name => App.Name + ".exe";
    public ImageSource? Icon { get; }
    public Reaction Mode { get; set; }
    public string? MediaSessionId { get; set; }
    public double Peak { get => _peak; set { _peak = value; Changed(); } }
    public string Status { get => _status; set { _status = value; Changed(); } }
    public AppRow(AppIdentity app, Reaction mode = Reaction.Duck, string? mediaSessionId = null)
    { App = app; Mode = mode; MediaSessionId = mediaSessionId; Icon = ProcessService.Icon(app); }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Changed([CallerMemberName] string? property = null) => PropertyChanged?.Invoke(this, new(property));
}

internal sealed record AppChoice(AppIdentity App, string Label);

public partial class MainWindow : Window
{
    private AppSettings _settings;
    private readonly SettingsStore _store;
    private readonly AutomationEngine _engine;
    private readonly ObservableCollection<AppRow> _sources = [];
    private readonly ObservableCollection<AppRow> _targets = [];
    private readonly Forms.NotifyIcon _tray;
    private readonly Forms.ToolStripMenuItem _trayToggle;
    private readonly Forms.ToolStripMenuItem _trayProfiles;
    private readonly System.Drawing.Icon _trayReadyIcon;
    private readonly System.Drawing.Icon _trayActiveIcon;
    private readonly System.Drawing.Icon _trayDisabledIcon;
    private readonly System.Drawing.Icon _trayErrorIcon;
    private bool _trayIconsDisposed;
    private HotkeyService? _hotkeys;
    private bool _loading;
    private bool _quitting;
    private bool _suspended;
    private bool _faulted;
    private Task? _shutdown;
    private string? _notice;

    public MainWindow(AppSettings settings, SettingsStore store)
    {
        InitializeComponent();
        _settings = settings;
        _store = store;
        _engine = new(settings.Current, settings.Enabled && settings.Current.IsEnabled);
        _engine.Updated += OnSnapshot;
        SourcesGrid.ItemsSource = _sources;
        TargetsGrid.ItemsSource = _targets;
        TargetModeColumn.ItemsSource = Enum.GetValues<Reaction>();
        GlobalModeBox.ItemsSource = Enum.GetValues<Reaction>();
        _notice = store.RecoveryNotice;

        _trayReadyIcon = TrayIconFactory.Create(TrayIconState.Ready);
        _trayActiveIcon = TrayIconFactory.Create(TrayIconState.Active);
        _trayDisabledIcon = TrayIconFactory.Create(TrayIconState.Disabled);
        _trayErrorIcon = TrayIconFactory.Create(TrayIconState.Error);

        _trayToggle = new Forms.ToolStripMenuItem("Automatyka");
        _trayToggle.Click += (_, _) => Dispatcher.InvokeAsync(() => SetEnabled(!_settings.Enabled));
        _trayProfiles = new Forms.ToolStripMenuItem("Profil");
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Otwórz konfigurację", null, (_, _) => Dispatcher.InvokeAsync(ShowWindow));
        menu.Items.Add(_trayToggle);
        menu.Items.Add(_trayProfiles);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Zakończ", null, (_, _) => Dispatcher.InvokeAsync(async () => await ShutdownAsync()));
        _tray = new Forms.NotifyIcon
        {
            Text = "DictaMute",
            Icon = IsAutomationEffective(_settings) ? _trayReadyIcon : _trayDisabledIcon,
            ContextMenuStrip = menu, Visible = true
        };
        _tray.DoubleClick += (_, _) => Dispatcher.InvokeAsync(ShowWindow);
        LoadProfile();
        SourceInitialized += (_, _) =>
        {
            _hotkeys = new HotkeyService(new WindowInteropHelper(this).Handle);
            _hotkeys.Pressed += HandleHotkey;
            try { _hotkeys.Configure(_settings.Hotkeys); }
            catch (Exception ex) { Report(ex); }
        };
        Loaded += (_, _) => _engine.Start();
        SystemEvents.PowerModeChanged += PowerModeChanged;
    }

    private bool IsAutomationEffective(AppSettings settings) =>
        settings.Enabled && settings.Current.IsEnabled && !_suspended && !_faulted;

    private void LoadProfile()
    {
        _loading = true;
        try
        {
            ProfileBox.ItemsSource = _settings.Profiles;
            ProfileBox.SelectedIndex = _settings.ActiveProfile;
            var profile = _settings.Current;
            ProfileName.Text = profile.Name;
            ProfileEnabled.IsChecked = profile.IsEnabled;
            AutomationEnabled.IsChecked = _settings.Enabled;
            _sources.Clear();
            foreach (var app in profile.Sources) _sources.Add(new(app));
            _targets.Clear();
            foreach (var rule in profile.Targets) _targets.Add(new(rule.App, rule.Mode, rule.MediaSessionId));
            ThresholdSlider.Value = profile.Threshold * 100;
            HoldSlider.Value = profile.HoldMilliseconds / 1000d;
            DuckSlider.Value = profile.DuckLevel * 100;
            FadeSlider.Value = profile.FadeMilliseconds;
            AllExceptSources.IsChecked = profile.AllExceptSources;
            AnyMicrophone.IsChecked = profile.AnyMicrophoneApp;
            GlobalModeBox.SelectedItem = profile.GlobalMode;
            SourceHotkey.Text = _settings.Hotkeys.AddSource;
            TargetHotkey.Text = _settings.Hotkeys.AddTarget;
            ToggleHotkey.Text = _settings.Hotkeys.Toggle;
            RefreshTrayMenu();
        }
        finally { _loading = false; }
    }

    private AppSettings ReadEditor()
    {
        TargetsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
        TargetsGrid.CommitEdit(DataGridEditingUnit.Row, true);
        var profiles = _settings.Profiles.ToArray();
        profiles[_settings.ActiveProfile] = new Profile
        {
            Name = ProfileName.Text.Trim(), IsEnabled = ProfileEnabled.IsChecked == true,
            Sources = _sources.Select(r => r.App).ToArray(),
            Targets = _targets.Select(r => new TargetRule(r.App, r.Mode,
                string.IsNullOrWhiteSpace(r.MediaSessionId) ? null : r.MediaSessionId.Trim())).ToArray(),
            Threshold = (float)(ThresholdSlider.Value / 100),
            HoldMilliseconds = (int)Math.Round(HoldSlider.Value * 1000),
            DuckLevel = (float)(DuckSlider.Value / 100),
            FadeMilliseconds = (int)Math.Round(FadeSlider.Value),
            AllExceptSources = AllExceptSources.IsChecked == true,
            AnyMicrophoneApp = AnyMicrophone.IsChecked == true,
            GlobalMode = GlobalModeBox.SelectedItem is Reaction mode ? mode : Reaction.Duck
        };
        var next = _settings with
        {
            Profiles = profiles,
            Hotkeys = new() { AddSource = SourceHotkey.Text.Trim(), AddTarget = TargetHotkey.Text.Trim(), Toggle = ToggleHotkey.Text.Trim() }
        };
        next.Validate();
        return next;
    }

    private void ApplyEditor(bool notify = true)
    {
        var next = ReadEditor();
        _hotkeys?.Configure(next.Hotkeys);
        try { _store.Save(next); }
        catch
        {
            try { _hotkeys?.Configure(_settings.Hotkeys); } catch (Exception ex) { Log.Write("Przywracanie skrótów po błędzie zapisu.", ex); }
            throw;
        }
        _settings = next;
        _engine.Configure(next.Current, IsAutomationEffective(next));
        _loading = true;
        ProfileBox.ItemsSource = _settings.Profiles;
        ProfileBox.SelectedIndex = _settings.ActiveProfile;
        _loading = false;
        RefreshTrayMenu();
        if (notify) _notice = next.Current.IsEnabled
            ? "Zapisano i zastosowano ustawienia."
            : "Zapisano ustawienia. Wybrany profil jest wyłączony.";
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    { try { ApplyEditor(); } catch (Exception ex) { Report(ex); } }

    private void SetEnabled(bool enabled)
    {
        if (_quitting) return;
        if (enabled && _faulted)
        {
            _notice = "Silnik zakończył pracę po błędzie. Zamknij i uruchom DictaMute ponownie.";
            AutomationEnabled.IsChecked = false;
            return;
        }
        // Disabling must always restore sound, even when unrelated editor fields are invalid.
        _settings = _settings with { Enabled = enabled };
        AutomationEnabled.IsChecked = enabled;
        _engine.Configure(_settings.Current, IsAutomationEffective(_settings));
        try { _store.Save(_settings); } catch (Exception ex) { Report(ex); }
        RefreshTrayMenu();
    }
    private void ToggleAutomation_Click(object sender, RoutedEventArgs e) => SetEnabled(AutomationEnabled.IsChecked == true);

    private void ProfileEnabled_Click(object sender, RoutedEventArgs e)
    {
        if (_loading || _quitting) return;
        var enabled = ProfileEnabled.IsChecked == true;
        try
        {
            var profiles = _settings.Profiles.ToArray();
            profiles[_settings.ActiveProfile] = profiles[_settings.ActiveProfile] with { IsEnabled = enabled };
            var next = _settings with { Profiles = profiles };
            next.Validate();
            _store.Save(next);
            _settings = next;
            _engine.Configure(next.Current, IsAutomationEffective(next));
            _notice = enabled
                ? $"Profil „{next.Current.Name}” został włączony."
                : $"Profil „{next.Current.Name}” został wyłączony.";
            RefreshTrayMenu();
        }
        catch (Exception ex)
        {
            _loading = true;
            ProfileEnabled.IsChecked = _settings.Current.IsEnabled;
            _loading = false;
            Report(ex);
        }
    }

    private void ProfileBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || ProfileBox.SelectedIndex < 0) return;
        SelectProfile(ProfileBox.SelectedIndex);
    }
    private void SelectProfile(int index)
    {
        if (index < 0 || index >= _settings.Profiles.Length || _quitting) return;
        try
        {
            ApplyEditor(false);
            var next = _settings with { ActiveProfile = index };
            _store.Save(next);
            _settings = next;
            _engine.Configure(next.Current, IsAutomationEffective(next));
            LoadProfile();
        }
        catch (Exception ex)
        {
            _loading = true; ProfileBox.SelectedIndex = _settings.ActiveProfile; _loading = false;
            Report(ex);
        }
    }
    private void NewProfile_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ApplyEditor(false);
            if (_settings.Profiles.Length >= 32) throw new InvalidOperationException("Maksymalnie 32 profile.");
            var next = _settings with
            {
                Profiles = [.. _settings.Profiles, new Profile { Name = $"Profil {_settings.Profiles.Length + 1}" }],
                ActiveProfile = _settings.Profiles.Length
            };
            _store.Save(next); _settings = next;
            _engine.Configure(next.Current, IsAutomationEffective(next));
            LoadProfile();
        }
        catch (Exception ex) { Report(ex); }
    }
    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        if (_settings.Profiles.Length <= 1) { _notice = "Musi pozostać przynajmniej jeden profil."; return; }
        if (MessageBox.Show($"Usunąć profil „{_settings.Current.Name}”?", "DictaMute", MessageBoxButton.YesNo,
            MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            var next = _settings with
            { Profiles = _settings.Profiles.Where((_, i) => i != _settings.ActiveProfile).ToArray(), ActiveProfile = 0 };
            _store.Save(next); _settings = next;
            _engine.Configure(next.Current, IsAutomationEffective(next));
            LoadProfile();
        }
        catch (Exception ex) { Report(ex); }
    }

    private void AddApp(AppIdentity app, bool source)
    {
        try
        {
            if (string.Equals(app.Name, "DictaMute", StringComparison.OrdinalIgnoreCase)) return;
            if (!source && _sources.Any(r => r.App.Matches(app)))
                throw new InvalidOperationException("Ta aplikacja jest źródłem X. Nie można jednocześnie wyciszać tego samego procesu.");
            var rows = source ? _sources : _targets;
            if (rows.Any(r => r.App.Matches(app))) { _notice = "Aplikacja jest już na tej liście."; return; }
            var row = new AppRow(app);
            rows.Add(row);
            try { ApplyEditor(false); }
            catch { rows.Remove(row); throw; }
            _notice = $"Dodano {app.Name} do grupy {(source ? "X" : "Y")}.";
            if (!IsVisible) _tray.ShowBalloonTip(2000, "DictaMute", _notice, Forms.ToolTipIcon.Info);
        }
        catch (Exception ex) { Report(ex); }
    }
    private void HandleHotkey(int id)
    {
        if (id == 3) { SetEnabled(!_settings.Enabled); return; }
        var app = ProcessService.Foreground();
        if (app is null) { _notice = "Nie można odczytać aplikacji aktywnego okna."; return; }
        AddApp(app, id == 1);
    }
    private void AddCapture_Click(object sender, RoutedEventArgs e)
    { if (CaptureApps.SelectedItem is AppChoice choice) AddApp(choice.App, true); }
    private void AddPlayback_Click(object sender, RoutedEventArgs e)
    { if (PlaybackApps.SelectedItem is AppChoice choice) AddApp(choice.App, false); }
    private void BrowseSource_Click(object sender, RoutedEventArgs e) => Browse(true);
    private void BrowseTarget_Click(object sender, RoutedEventArgs e) => Browse(false);
    private void Browse(bool source)
    {
        var picker = new OpenFileDialog { Filter = "Aplikacje Windows (*.exe)|*.exe", CheckFileExists = true, Multiselect = false };
        if (picker.ShowDialog(this) == true) AddApp(new(Path.GetFileNameWithoutExtension(picker.FileName), picker.FileName), source);
    }
    private void RemoveSource_Click(object sender, RoutedEventArgs e) => RemoveRow(SourcesGrid, _sources);
    private void RemoveTarget_Click(object sender, RoutedEventArgs e) => RemoveRow(TargetsGrid, _targets);
    private void RemoveRow(DataGrid grid, ObservableCollection<AppRow> rows)
    {
        if (grid.SelectedItem is not AppRow row) return;
        rows.Remove(row);
        try { ApplyEditor(false); }
        catch (Exception ex) { rows.Add(row); Report(ex); }
    }
    private void AssignMedia_Click(object sender, RoutedEventArgs e)
    {
        if (TargetsGrid.SelectedItem is not AppRow row || MediaSessions.SelectedItem is not string id)
        { _notice = "Zaznacz aplikację docelową Y i wybierz ID sesji."; return; }
        row.MediaSessionId = id;
        TargetsGrid.Items.Refresh();
        try { ApplyEditor(); } catch (Exception ex) { Report(ex); }
    }

    private void OnSnapshot(EngineSnapshot snapshot)
    {
        if (_quitting || Dispatcher.HasShutdownStarted) return;
        Dispatcher.InvokeAsync(() =>
        {
            if (_quitting) return;
            if (snapshot.FatalError is not null)
            {
                _faulted = true;
                _settings = _settings with { Enabled = false };
                AutomationEnabled.IsChecked = false;
                _notice = "Silnik zatrzymany: " + snapshot.FatalError;
                RefreshTrayMenu();
            }
            foreach (var row in _sources)
            {
                var active = snapshot.Capture.Where(c => c.Active && row.App.Matches(c.App)).ToArray();
                row.Peak = active.Length == 0 ? 0 : active.Max(c => c.Peak) * 100;
                row.Status = active.Length == 0 ? "Nieaktywna" : string.Join(", ", active.Select(c => c.Device).Distinct());
            }
            UpdateChoices(CaptureApps, snapshot.Capture.Where(c => c.Active)
                .Select(c => new AppChoice(c.App, c.App.Name + " — " + c.Device))
                .DistinctBy(c => c.App.ExecutablePath ?? c.App.Name).ToArray());
            UpdateChoices(PlaybackApps, snapshot.Playback.Select(a => new AppChoice(a, a.Name)).ToArray());
            var oldIds = MediaSessions.ItemsSource as string[] ?? [];
            if (!oldIds.SequenceEqual(snapshot.MediaIds))
            {
                var selected = MediaSessions.SelectedItem as string;
                MediaSessions.ItemsSource = snapshot.MediaIds;
                MediaSessions.SelectedItem = selected is not null && snapshot.MediaIds.Contains(selected) ? selected : snapshot.MediaIds.FirstOrDefault();
            }
            PeakBar.Value = snapshot.Peak * 100;
            PeakText.Text = $"{snapshot.Peak * 100:F1}%";
            var profileDisabled = _settings.Enabled && !_settings.Current.IsEnabled;
            StatusText.Text = _faulted ? "Automatyka zatrzymana po błędzie"
                : profileDisabled ? "Wybrany profil jest wyłączony"
                : !snapshot.Enabled ? "Automatyka wyłączona"
                : snapshot.Active ? "Wyciszanie aktywne" : "Oczekiwanie na sygnał ze źródeł";
            NoticeText.Text = snapshot.Warning ?? _notice ?? "Zapisz ustawienia po zmianie suwaków lub trybu reakcji.";
            _tray.Text = "DictaMute — " + (_faulted ? "błąd" : profileDisabled ? "profil wyłączony"
                : !snapshot.Enabled ? "wyłączony" : snapshot.Active ? "wyciszanie aktywne" : "gotowy");
            _tray.Icon = _faulted ? _trayErrorIcon
                : profileDisabled || !snapshot.Enabled ? _trayDisabledIcon
                : snapshot.Active ? _trayActiveIcon
                : _trayReadyIcon;
        });
    }

    private static void UpdateChoices(ComboBox box, AppChoice[] choices)
    {
        var current = box.ItemsSource as AppChoice[] ?? [];
        if (current.SequenceEqual(choices)) return;
        var selected = box.SelectedItem as AppChoice;
        box.ItemsSource = choices;
        box.SelectedItem = choices.FirstOrDefault(c => selected is not null && c.App.Matches(selected.App)) ?? choices.FirstOrDefault();
    }
    private void RefreshTrayMenu()
    {
        _trayToggle.Checked = _settings.Enabled;
        _trayToggle.Text = _settings.Enabled ? "Wyłącz automatykę" : "Włącz automatykę";
        foreach (Forms.ToolStripItem item in _trayProfiles.DropDownItems.Cast<Forms.ToolStripItem>().ToArray()) item.Dispose();
        _trayProfiles.DropDownItems.Clear();
        for (var i = 0; i < _settings.Profiles.Length; i++)
        {
            var index = i;
            var profile = _settings.Profiles[i];
            var text = profile.Name + (profile.IsEnabled ? "" : " — wyłączony");
            var item = new Forms.ToolStripMenuItem(text) { Checked = i == _settings.ActiveProfile };
            item.Click += (_, _) => Dispatcher.InvokeAsync(() => SelectProfile(index));
            _trayProfiles.DropDownItems.Add(item);
        }
    }
    private void DisposeTrayIcons()
    {
        if (_trayIconsDisposed) return;
        _trayIconsDisposed = true;
        _trayReadyIcon.Dispose();
        _trayActiveIcon.Dispose();
        _trayDisabledIcon.Dispose();
        _trayErrorIcon.Dispose();
    }

    private void Report(Exception ex)
    {
        Log.Write("Operacja interfejsu nie powiodła się.", ex);
        _notice = ex.Message;
        MessageBox.Show(this, ex.Message, "DictaMute", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    private void ShowWindow() { Show(); WindowState = WindowState.Normal; Activate(); }
    private void Hide_Click(object sender, RoutedEventArgs e) => Hide();
    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_quitting) { e.Cancel = true; Hide(); }
        base.OnClosing(e);
    }
    protected override void OnStateChanged(EventArgs e)
    { if (WindowState == WindowState.Minimized && !_quitting) Hide(); base.OnStateChanged(e); }
    private void PowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (_quitting) return;
        Dispatcher.InvokeAsync(() =>
        {
            _suspended = e.Mode == PowerModes.Suspend || (e.Mode != PowerModes.Resume && _suspended);
            _engine.Configure(_settings.Current, IsAutomationEffective(_settings));
        });
    }
    private async void Exit_Click(object sender, RoutedEventArgs e) => await ShutdownAsync();
    public Task ShutdownAsync() => _shutdown ??= StopAndCloseAsync();
    private async Task StopAndCloseAsync()
    {
        _quitting = true;
        SystemEvents.PowerModeChanged -= PowerModeChanged;
        _hotkeys?.Dispose();
        _engine.Updated -= OnSnapshot;
        try { await _engine.StopAsync(); }
        finally
        {
            _tray.Visible = false;
            _tray.ContextMenuStrip?.Dispose();
            _tray.Dispose();
            DisposeTrayIcons();
            System.Windows.Application.Current.Shutdown();
        }
    }
    public void StopForSessionEnding()
    {
        _quitting = true;
        SystemEvents.PowerModeChanged -= PowerModeChanged;
        _engine.Updated -= OnSnapshot;
        try { _engine.StopAsync().GetAwaiter().GetResult(); }
        catch (Exception ex) { Log.Write("Kończenie sesji Windows.", ex); }
        _hotkeys?.Dispose();
        _tray.Dispose();
        DisposeTrayIcons();
    }
}
