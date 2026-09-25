namespace DictaMute.Core;

public interface ISessionVolume
{
    float Volume { get; set; }
    bool Muted { get; set; }
}

/// <summary>
/// Owns only the values actually changed by DictaMute. If another client changes
/// a value, relinquish it rather than overwriting the user's mixer adjustment.
/// </summary>
public sealed class VolumeLease
{
    private const float Epsilon = 0.001f;
    private readonly ISessionVolume _session;
    private readonly Reaction _mode;
    private readonly float _originalVolume;
    private readonly bool _originalMute;
    private float _lastVolume;
    private bool _lastMute;
    private bool _ownsVolume;
    private bool _ownsMute;

    public VolumeLease(ISessionVolume session, Reaction mode)
    {
        if (mode == Reaction.Pause) throw new ArgumentException("Pause is handled by media sessions.", nameof(mode));
        _session = session;
        _mode = mode;
        _lastVolume = _originalVolume = session.Volume;
        _lastMute = _originalMute = session.Muted;
        _ownsVolume = mode == Reaction.Duck;
        _ownsMute = mode == Reaction.Mute;
    }

    public void Apply(float limit, TimeSpan step, TimeSpan fade)
    {
        CheckOwnership();
        if (_mode == Reaction.Mute && _ownsMute) WriteMute(true);
        if (_mode == Reaction.Duck && _ownsVolume)
            MoveVolume(Math.Min(_originalVolume, Math.Clamp(limit, 0, 1)), step, fade);
    }

    public bool Restore(TimeSpan step, TimeSpan fade, bool immediate = false)
    {
        CheckOwnership();
        if (_ownsMute) WriteMute(_originalMute);
        if (_ownsVolume) MoveVolume(_originalVolume, step, immediate ? TimeSpan.Zero : fade);
        return !_ownsVolume || Math.Abs(_lastVolume - _originalVolume) <= Epsilon;
    }

    private void CheckOwnership()
    {
        if (_ownsVolume && Math.Abs(_session.Volume - _lastVolume) > Epsilon) _ownsVolume = false;
        if (_ownsMute && _session.Muted != _lastMute) _ownsMute = false;
    }

    private void WriteMute(bool value)
    {
        if (_lastMute == value) return;
        _session.Muted = value;
        _lastMute = value;
    }

    private void MoveVolume(float target, TimeSpan step, TimeSpan fade)
    {
        var amount = fade <= TimeSpan.Zero ? 1f : Math.Clamp((float)(step.TotalSeconds / fade.TotalSeconds), 0, 1);
        var delta = target - _lastVolume;
        var next = Math.Abs(delta) <= amount ? target : _lastVolume + Math.Sign(delta) * amount;
        if (Math.Abs(next - _lastVolume) < 0.00001f) return;
        _session.Volume = next;
        _lastVolume = next;
    }
}
