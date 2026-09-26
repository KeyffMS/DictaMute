using System.Diagnostics;

namespace DictaMute.Services;

internal sealed record EngineConfiguration(Profile Profile, bool Enabled);
internal sealed record EngineSnapshot(bool Enabled, bool Active, float Peak, CaptureReading[] Capture,
    AppIdentity[] Playback, string[] MediaIds, string? Warning, string? FatalError = null);

internal sealed class AutomationEngine
{
    private EngineConfiguration _configuration;
    private readonly CancellationTokenSource _stop = new();
    private Task? _worker;
    public event Action<EngineSnapshot>? Updated;

    public AutomationEngine(Profile profile, bool enabled) => _configuration = new(profile, enabled);
    public void Configure(Profile profile, bool enabled)
    {
        profile.Validate();
        // Caller supplies a new immutable snapshot, not WPF's editable collections.
        Volatile.Write(ref _configuration, new(profile, enabled));
    }
    public void Start() => _worker ??= Task.Run(RunAsync);
    public async Task StopAsync()
    {
        _stop.Cancel();
        if (_worker is not null) await _worker.ConfigureAwait(false);
    }

    private async Task RunAsync()
    {
        AudioCatalog? audio = null;
        var media = new MediaService();
        var leases = new Dictionary<string, (AudioSession Session, VolumeLease Volume)>();
        var pauseDecisions = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var gate = new NoiseGate();
        var clock = Stopwatch.StartNew();
        var previousTime = clock.Elapsed;
        var nextAudioRefresh = TimeSpan.Zero;
        var nextMediaRefresh = TimeSpan.Zero;
        var nextSnapshot = TimeSpan.Zero;
        var mediaIds = Array.Empty<string>();
        EngineConfiguration? applied = null;
        var wasOpen = false;

        async Task RestoreAllAsync()
        {
            foreach (var lease in leases.Values)
                try { lease.Volume.Restore(TimeSpan.Zero, TimeSpan.Zero, immediate: true); }
                catch (Exception ex) { Log.Write("Przywracanie głośności sesji.", ex); }
            leases.Clear();
            await media.RestoreAsync().ConfigureAwait(false);
            pauseDecisions.Clear();
            gate.Reset();
            wasOpen = false;
        }

        try
        {
            audio = new AudioCatalog();
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(75));
            do
            {
                var now = clock.Elapsed;
                var step = now - previousTime;
                previousTime = now;
                var configuration = Volatile.Read(ref _configuration);
                if (!ReferenceEquals(configuration, applied))
                {
                    await RestoreAllAsync().ConfigureAwait(false);
                    applied = configuration;
                }
                var profile = configuration.Profile;
                if (now >= nextAudioRefresh)
                {
                    audio.Refresh();
                    // Session churn while the gate is open must be discovered quickly:
                    // a replacement playback session otherwise has up to 500 ms to play
                    // at full volume before DictaMute acquires it.
                    nextAudioRefresh = clock.Elapsed + TimeSpan.FromMilliseconds(wasOpen ? 150 : 500);
                }
                if (now >= nextMediaRefresh)
                {
                    mediaIds = await media.GetSessionIdsAsync().ConfigureAwait(false);
                    nextMediaRefresh = clock.Elapsed + TimeSpan.FromMilliseconds(500);
                }
                var capture = audio.ReadCapture();
                var sources = capture.Where(c => c.Active &&
                    (profile.AnyMicrophoneApp || RuleResolver.IsSource(profile, c.App))).ToArray();
                var dynamicSources = profile.AnyMicrophoneApp ? sources.Select(c => c.App).ToArray() : [];
                if (dynamicSources.Length > 0)
                {
                    // An existing target can begin recording while the gate is already
                    // open. Its new source role takes precedence immediately.
                    await media.RestoreAsync(dynamicSources).ConfigureAwait(false);
                    foreach (var app in dynamicSources) pauseDecisions.Remove(app.ExecutablePath ?? app.Name);
                }
                var peak = sources.Length == 0 ? 0 : sources.Max(c => c.Peak);
                var open = gate.Update(configuration.Enabled, sources.Length > 0, peak, profile.Threshold,
                    TimeSpan.FromMilliseconds(profile.HoldMilliseconds), clock.Elapsed);
                if (open != wasOpen)
                {
                    Log.Write($"Bramka {(open ? "OPEN" : "CLOSE")}: sources={sources.Length}, " +
                        $"peak={peak:P1}, openThreshold={profile.Threshold:P1}, " +
                        $"releaseThreshold={profile.Threshold * NoiseGate.ReleaseThresholdRatio:P1}, " +
                        $"hold={profile.HoldMilliseconds} ms.");
                    if (open) nextAudioRefresh = TimeSpan.Zero;
                }
                var fade = TimeSpan.FromMilliseconds(profile.FadeMilliseconds);

                if (open)
                {
                    foreach (var session in audio.Playback.ToArray())
                    {
                        if (leases.ContainsKey(session.Key)) continue;
                        var dynamicSource = dynamicSources.Any(a => a.Matches(session.App));
                        var rule = RuleResolver.TargetFor(profile, session.App,
                            session.ProcessId == Environment.ProcessId || dynamicSource);
                        if (rule is null) continue;
                        var mode = rule.Mode;
                        if (mode == Reaction.Pause)
                        {
                            var key = session.App.ExecutablePath ?? session.App.Name;
                            if (!pauseDecisions.TryGetValue(key, out var paused))
                                pauseDecisions[key] = paused = await media.TryPauseAsync(rule).ConfigureAwait(false);
                            if (paused) continue;
                            mode = Reaction.Duck;
                        }
                        try { leases.Add(session.Key, (session, new VolumeLease(session, mode))); }
                        catch (Exception ex) { Log.Write("Zapamiętywanie stanu głośności.", ex); }
                    }
                }
                else if (wasOpen)
                {
                    await media.RestoreAsync().ConfigureAwait(false);
                    pauseDecisions.Clear();
                }

                foreach (var pair in leases.ToArray())
                {
                    try
                    {
                        if (pair.Value.Session.Removed)
                        {
                            leases.Remove(pair.Key);
                            continue;
                        }
                        var becameSource = dynamicSources.Any(a => a.Matches(pair.Value.Session.App));
                        if (becameSource)
                        {
                            pair.Value.Volume.Restore(TimeSpan.Zero, TimeSpan.Zero, immediate: true);
                            leases.Remove(pair.Key);
                        }
                        else if (open) pair.Value.Volume.Apply(profile.DuckLevel, step, fade);
                        else if (pair.Value.Volume.Restore(step, fade)) leases.Remove(pair.Key);
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Sterowanie poziomem sesji audio.", ex);
                        try { pair.Value.Volume.Restore(TimeSpan.Zero, TimeSpan.Zero, true); }
                        catch (Exception restoreError) { Log.Write("Sesja nie jest już dostępna do przywrócenia.", restoreError); }
                        leases.Remove(pair.Key);
                    }
                }
                wasOpen = open;
                if (clock.Elapsed >= nextSnapshot)
                {
                    Updated?.Invoke(new(configuration.Enabled, open, peak, capture,
                        audio.Playback.Select(s => s.App).DistinctBy(a => a.ExecutablePath ?? a.Name).ToArray(),
                        mediaIds, audio.Warning ?? media.Warning));
                    nextSnapshot = clock.Elapsed + TimeSpan.FromMilliseconds(225);
                }
            } while (await timer.WaitForNextTickAsync(_stop.Token).ConfigureAwait(false));
        }
        catch (OperationCanceledException) when (_stop.IsCancellationRequested) { }
        catch (Exception ex)
        {
            Log.Write("Zatrzymano automatykę po błędzie.", ex);
            Updated?.Invoke(new(false, false, 0, [], [], [], null, ex.Message));
        }
        finally
        {
            await RestoreAllAsync().ConfigureAwait(false);
            try { audio?.Dispose(); }
            catch (Exception ex) { Log.Write("Zwalnianie zasobów audio przy wyjściu.", ex); }
        }
    }
}
