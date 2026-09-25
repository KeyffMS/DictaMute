namespace DictaMute.Core;

/// <summary>Amplitude gate with retriggerable release delay; not speech recognition.</summary>
public sealed class NoiseGate
{
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
        if (sourceActive && float.IsFinite(peak) && peak > threshold)
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
