namespace DictaMute.Core;

public readonly record struct SourceSelectionResult(AppIdentity? App, bool Ambiguous, bool FromActiveMicrophone);

/// <summary>
/// Chooses which application a source hotkey should add without confusing the
/// foreground text receiver with the process that actually owns the microphone.
/// </summary>
public static class SourceCandidateResolver
{
    public static SourceSelectionResult Resolve(IEnumerable<AppIdentity> activeMicrophoneApps,
        AppIdentity? selectedMicrophoneApp, AppIdentity? foregroundApp)
    {
        var active = new List<AppIdentity>();
        foreach (var candidate in activeMicrophoneApps ?? [])
        {
            if (candidate is null || active.Any(existing => existing.Matches(candidate))) continue;
            active.Add(candidate);
        }

        if (active.Count == 1)
            return new(active[0], Ambiguous: false, FromActiveMicrophone: true);

        if (active.Count > 1)
        {
            var selected = selectedMicrophoneApp is null
                ? null
                : active.FirstOrDefault(candidate => candidate.Matches(selectedMicrophoneApp));
            if (selected is not null)
                return new(selected, Ambiguous: false, FromActiveMicrophone: true);

            var foreground = foregroundApp is null
                ? null
                : active.FirstOrDefault(candidate => candidate.Matches(foregroundApp));
            if (foreground is not null)
                return new(foreground, Ambiguous: false, FromActiveMicrophone: true);

            return new(null, Ambiguous: true, FromActiveMicrophone: false);
        }

        return new(foregroundApp, Ambiguous: false, FromActiveMicrophone: false);
    }
}
