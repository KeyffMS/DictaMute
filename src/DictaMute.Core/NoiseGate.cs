namespace DictaMute.Core;

/// <summary>Amplitude gate with retriggerable release delay; not speech recognition.</summary>
public sealed class NoiseGate
{
    // Once the gate is open, softer speech is allowed to retrigger the release
    // timer. This hysteresis prevents audible open/close chatter around the
    // configured opening threshold without changing the user's Hold Time.
    public const float ReleaseThresholdRatio = 0.60f;

    private TimeSpan? _lastAbove;
    public bool IsOpen { get; private set; }

    public bool Update(bool enabled, bool sourceActive, float peak, float threshold,
        TimeSpan hold, TimeSpan now)
    {
        if (!enabled)
        {
            Reset();
            return false;
        }
        var effectiveThreshold = IsOpen ? threshold * ReleaseThresholdRatio : threshold;
        if (sourceActive && float.IsFinite(peak) && peak > effectiveThreshold)
        {
            _lastAbove = now;
            return IsOpen = true;
        }
        return IsOpen = _lastAbove is { } last && now >= last && now - last < hold;
    }

    public void Reset()
    {
        _lastAbove = null;
        IsOpen = false;
    }
}
