using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

namespace DictaMute.Services;

internal sealed record CaptureReading(AppIdentity App, string Device, bool Active, float Peak);

internal sealed class AudioSession : ISessionVolume, IDisposable
{
    private readonly AudioSessionControl _control;
    public string Key { get; }
    public string DeviceId { get; }
    public int ProcessId { get; }
    public AppIdentity App { get; }
    public bool Capture { get; }
    public bool Removed { get; private set; }
    public bool Active => !Removed && _control.State == AudioSessionState.AudioSessionStateActive;
    public float Volume { get => _control.SimpleAudioVolume.Volume; set => _control.SimpleAudioVolume.Volume = value; }
    public bool Muted { get => _control.SimpleAudioVolume.Mute; set => _control.SimpleAudioVolume.Mute = value; }

    public AudioSession(string key, string deviceId, int processId, AppIdentity app, bool capture, AudioSessionControl control)
    {
        Key = key; DeviceId = deviceId; ProcessId = processId; App = app; Capture = capture; _control = control;
    }

    public void Dispose()
    {
        if (Removed) return;
        Removed = true;
        _control.Dispose();
    }
}

/// <summary>All access happens on the engine's MTA worker, never on the WPF thread.</summary>
internal sealed class AudioCatalog : IDisposable
{
    private sealed record Endpoint(MMDevice Device, bool Capture);
    private readonly MMDeviceEnumerator _enumerator = new();
    private readonly Dictionary<string, Endpoint> _devices = [];
    private readonly Dictionary<string, AudioSession> _sessions = [];
    public string? Warning { get; private set; }
    public IEnumerable<AudioSession> Playback => _sessions.Values.Where(s => !s.Capture);

    public void Refresh()
    {
        Warning = null;
        var seenDevices = new HashSet<string>();
        foreach (var flow in new[] { DataFlow.Capture, DataFlow.Render })
        {
            try
            {
                var devices = _enumerator.EnumerateAudioEndPoints(flow, DeviceState.Active);
                foreach (var candidate in devices)
                {
                    var id = candidate.ID;
                    seenDevices.Add(id);
                    if (_devices.ContainsKey(id)) candidate.Dispose();
                    else _devices.Add(id, new(candidate, flow == DataFlow.Capture));
                }
            }
            catch (Exception ex)
            {
                Warning = "Nie można odczytać części urządzeń audio. Szczegóły w dzienniku.";
                Log.Write("Enumeracja urządzeń audio.", ex);
                foreach (var pair in _devices.Where(p => p.Value.Capture == (flow == DataFlow.Capture)))
                    seenDevices.Add(pair.Key);
            }
        }

        var seenSessions = new HashSet<string>();
        var processes = new Dictionary<int, AppIdentity?>();
        foreach (var pair in _devices.Where(p => seenDevices.Contains(p.Key)).ToArray())
        {
            try
            {
                var manager = pair.Value.Device.AudioSessionManager;
                manager.RefreshSessions();
                var collection = manager.Sessions;
                for (var i = 0; i < collection.Count; i++)
                {
                    AudioSessionControl? control = collection[i];
                    try
                    {
                        if (control.State == AudioSessionState.AudioSessionStateExpired) continue;
                        var pid = (int)control.GetProcessID;
                        if (pid <= 0 || pid == Environment.ProcessId) continue;
                        if (!processes.TryGetValue(pid, out var app))
                            processes[pid] = app = ProcessService.Read(pid);
                        if (app is null) continue;
                        var key = pair.Key + "|" + control.GetSessionInstanceIdentifier;
                        seenSessions.Add(key);
                        if (_sessions.ContainsKey(key)) continue;
                        _sessions.Add(key, new(key, pair.Key, pid, app, pair.Value.Capture, control));
                        control = null; // Ownership transferred to the catalog.
                    }
                    catch (Exception ex) { Log.Write("Sesja audio zniknęła podczas odczytu.", ex); }
                    finally { control?.Dispose(); }
                }
            }
            catch (Exception ex)
            {
                Warning = "Urządzenie audio jest chwilowo niedostępne.";
                Log.Write("Odświeżanie sesji audio.", ex);
                // Keep handles for restoration and retry after an endpoint transient error.
                foreach (var session in _sessions.Values.Where(s => s.DeviceId == pair.Key)) seenSessions.Add(session.Key);
            }
        }

        foreach (var key in _sessions.Keys.Where(k => !seenSessions.Contains(k)).ToArray())
        {
            try { _sessions[key].Dispose(); } catch (Exception ex) { Log.Write("Zamykanie sesji.", ex); }
            _sessions.Remove(key);
        }
        foreach (var key in _devices.Keys.Where(k => !seenDevices.Contains(k)).ToArray())
        {
            try { _devices[key].Device.Dispose(); } catch (Exception ex) { Log.Write("Odłączanie urządzenia.", ex); }
            _devices.Remove(key);
        }
    }

    public CaptureReading[] ReadCapture()
    {
        var readings = new List<CaptureReading>();
        foreach (var pair in _devices.Where(p => p.Value.Capture))
        {
            try
            {
                var device = pair.Value.Device;
                var peak = device.AudioEndpointVolume.Mute || device.AudioEndpointVolume.MasterVolumeLevelScalar <= 0
                    ? 0 : device.AudioMeterInformation.MasterPeakValue;
                if (!float.IsFinite(peak)) peak = 0;
                peak = Math.Clamp(peak, 0, 1);
                foreach (var session in _sessions.Values.Where(s => s.Capture && s.DeviceId == pair.Key))
                {
                    try
                    {
                        var active = session.Active;
                        readings.Add(new(session.App, device.FriendlyName, active, active ? peak : 0));
                    }
                    catch (Exception ex) { Log.Write("Odczyt aktywności wejścia.", ex); }
                }
            }
            catch (Exception ex)
            {
                Warning = "Nie można odczytać poziomu mikrofonu. Sprawdź urządzenie i sterownik.";
                Log.Write("Odczyt miernika mikrofonu.", ex);
            }
        }
        return readings.ToArray();
    }

    public void Dispose()
    {
        foreach (var session in _sessions.Values)
            try { session.Dispose(); } catch (Exception ex) { Log.Write("Zwalnianie sesji audio.", ex); }
        foreach (var endpoint in _devices.Values)
            try { endpoint.Device.Dispose(); } catch (Exception ex) { Log.Write("Zwalnianie urządzenia audio.", ex); }
        _sessions.Clear();
        _devices.Clear();
        _enumerator.Dispose();
    }
}
