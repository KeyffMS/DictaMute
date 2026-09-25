using Windows.Media.Control;

namespace DictaMute.Services;

internal sealed class MediaService
{
    private GlobalSystemMediaTransportControlsSessionManager? _manager;
    private DateTime _retryAfter;
    private readonly List<OwnedPause> _paused = [];
    public string? Warning { get; private set; }

    private sealed class OwnedPause
    {
        public GlobalSystemMediaTransportControlsSession Session { get; }
        private int _superseded;
        public bool Superseded => Volatile.Read(ref _superseded) != 0;
        public OwnedPause(GlobalSystemMediaTransportControlsSession session)
        {
            Session = session;
            session.MediaPropertiesChanged += MediaChanged;
            session.PlaybackInfoChanged += PlaybackChanged;
        }
        private void MediaChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args) =>
            Interlocked.Exchange(ref _superseded, 1);
        private void PlaybackChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            try
            {
                if (sender.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                    Interlocked.Exchange(ref _superseded, 1);
            }
            catch { Interlocked.Exchange(ref _superseded, 1); }
        }
        public void Detach()
        {
            Session.MediaPropertiesChanged -= MediaChanged;
            Session.PlaybackInfoChanged -= PlaybackChanged;
        }
    }

    private async Task<bool> ConnectAsync()
    {
        if (_manager is not null) return true;
        if (DateTime.UtcNow < _retryAfter) return false;
        try
        {
            _manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            return true;
        }
        catch (Exception ex)
        {
            _retryAfter = DateTime.UtcNow.AddSeconds(15);
            Warning = "Sterowanie multimediami jest niedostępne; tryb Pause użyje ściszenia.";
            Log.Write("Połączenie z GSMTC.", ex);
            return false;
        }
    }

    public async Task<string[]> GetSessionIdsAsync()
    {
        if (!await ConnectAsync().ConfigureAwait(false)) return [];
        try { return _manager!.GetSessions().Select(s => s.SourceAppUserModelId).Distinct().Order().ToArray(); }
        catch (Exception ex) { Log.Write("Lista sesji multimedialnych.", ex); return []; }
    }

    public async Task<bool> TryPauseAsync(TargetRule rule)
    {
        if (!await ConnectAsync().ConfigureAwait(false)) return false;
        try
        {
            var explicitId = string.IsNullOrWhiteSpace(rule.MediaSessionId) ? rule.App.AppUserModelId : rule.MediaSessionId;
            var matches = _manager!.GetSessions().Where(s =>
                !string.IsNullOrWhiteSpace(explicitId)
                    ? string.Equals(s.SourceAppUserModelId, explicitId, StringComparison.OrdinalIgnoreCase)
                    : string.Equals(AppIdentity.NormalizeName(s.SourceAppUserModelId), rule.App.Name, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            // Never send an unaddressed global Play/Pause key or guess a substring match.
            if (matches.Length != 1)
            {
                Warning = $"Pause: brak jednoznacznej sesji dla {rule.App.Name}. Użyto Duck; przypisz ID sesji w ustawieniach.";
                return false;
            }
            var session = matches[0];
            if (_paused.Any(p => ReferenceEquals(p.Session, session))) return true;
            var info = session.GetPlaybackInfo();
            if (info.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused) return true;
            if (info.PlaybackStatus != GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing ||
                !info.Controls.IsPauseEnabled || !info.Controls.IsPlayEnabled || !await session.TryPauseAsync())
            {
                Warning = $"Odtwarzacz {rule.App.Name} nie przyjął pauzy. Użyto Duck.";
                return false;
            }
            _paused.Add(new(session));
            return true;
        }
        catch (Exception ex)
        {
            Warning = $"Błąd pauzy dla {rule.App.Name}; użyto ściszenia.";
            Log.Write("Pauzowanie sesji multimedialnej.", ex);
            return false;
        }
    }

    public async Task RestoreAsync()
    {
        foreach (var owned in _paused.ToArray())
        {
            try
            {
                owned.Detach();
                var info = owned.Session.GetPlaybackInfo();
                // Sessions paused before DictaMute acted were never added to this list.
                // Do not restart playback after the user changed track or resumed it.
                if (!owned.Superseded && info.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused &&
                    info.Controls.IsPlayEnabled && !await owned.Session.TryPlayAsync())
                {
                    Warning = "Odtwarzacz nie przyjął wznowienia. Wznów odtwarzanie ręcznie.";
                    Log.Write(Warning);
                }
            }
            catch (Exception ex) { Log.Write("Przywracanie odtwarzania.", ex); }
        }
        _paused.Clear();
    }
}
