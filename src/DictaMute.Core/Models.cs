namespace DictaMute.Core;

public enum Reaction { Duck, Mute, Pause }

public sealed record AppIdentity(string ProcessName, string? ExecutablePath = null,
    string? AppUserModelId = null)
{
    public string Name => NormalizeName(ProcessName);

    public bool Matches(AppIdentity other)
    {
        if (!string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)) return false;
        // Full paths distinguish unrelated executables with the same name. A name-only
        // rule remains useful for protected processes whose image path cannot be read.
        return string.IsNullOrWhiteSpace(ExecutablePath) || string.IsNullOrWhiteSpace(other.ExecutablePath)
            || string.Equals(ExecutablePath.Replace('/', '\\'), other.ExecutablePath.Replace('/', '\\'),
                StringComparison.OrdinalIgnoreCase);
    }

    public static string NormalizeName(string name)
    {
        var result = (name ?? "").Replace('\\', '/').Split('/').Last().Trim();
        return result.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? result[..^4] : result;
    }
}

public sealed record TargetRule(AppIdentity App, Reaction Mode = Reaction.Duck,
    string? MediaSessionId = null);

public sealed record Profile
{
    public string Name { get; init; } = "Dyktowanie";
    public AppIdentity[] Sources { get; init; } = [];
    public TargetRule[] Targets { get; init; } = [];
    public bool AllExceptSources { get; init; }
    public bool AnyMicrophoneApp { get; init; }
    public Reaction GlobalMode { get; init; } = Reaction.Duck;
    public float Threshold { get; init; } = 0.03f;
    public int HoldMilliseconds { get; init; } = 1500;
    public float DuckLevel { get; init; } = 0.20f;
    public int FadeMilliseconds { get; init; } = 180;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name) || Name.Length > 80)
            throw new InvalidDataException("Nazwa profilu musi mieć od 1 do 80 znaków.");
        if (!float.IsFinite(Threshold) || Threshold is < 0 or > 1)
            throw new InvalidDataException("Próg mikrofonu musi mieścić się w zakresie 0–100%.");
        if (!float.IsFinite(DuckLevel) || DuckLevel is < 0 or > 1)
            throw new InvalidDataException("Poziom ściszenia musi mieścić się w zakresie 0–100%.");
        if (HoldMilliseconds is < 0 or > 10000 || FadeMilliseconds is < 0 or > 3000)
            throw new InvalidDataException("Nieprawidłowy czas podtrzymania lub przejścia.");
        if (Sources is null || Targets is null || Sources.Length > 256 || Targets.Length > 256)
            throw new InvalidDataException("Nieprawidłowe listy aplikacji (maksymalnie 256 wpisów).");
        if (!Enum.IsDefined(GlobalMode)) throw new InvalidDataException("Nieznany tryb globalny.");
        foreach (var source in Sources) ValidateApp(source);
        foreach (var target in Targets)
        {
            if (target is null || !Enum.IsDefined(target.Mode))
                throw new InvalidDataException("Nieprawidłowa reguła docelowa.");
            ValidateApp(target.App);
        }
    }

    private static void ValidateApp(AppIdentity? app)
    {
        if (app is null || string.IsNullOrWhiteSpace(app.Name) || app.Name.Length > 260)
            throw new InvalidDataException("Brak poprawnej nazwy procesu w regule.");
    }
}

public sealed record HotkeySettings
{
    public string AddSource { get; init; } = "Ctrl+Alt+X";
    public string AddTarget { get; init; } = "Ctrl+Alt+Y";
    public string Toggle { get; init; } = "Ctrl+Alt+M";
}

public sealed record AppSettings
{
    public int Version { get; init; } = 1;
    public bool Enabled { get; init; } = true;
    public int ActiveProfile { get; init; }
    public Profile[] Profiles { get; init; } =
        [new(), new() { Name = "Spotkania", HoldMilliseconds = 2200 }];
    public HotkeySettings Hotkeys { get; init; } = new();

    public Profile Current => Profiles[ActiveProfile];

    public void Validate()
    {
        if (Version != 1) throw new InvalidDataException("Nieobsługiwana wersja konfiguracji.");
        if (Profiles is null || Profiles.Length is < 1 or > 32 ||
            ActiveProfile < 0 || ActiveProfile >= Profiles.Length)
            throw new InvalidDataException("Nieprawidłowa lista profili.");
        foreach (var profile in Profiles)
        {
            if (profile is null) throw new InvalidDataException("Pusty profil.");
            profile.Validate();
        }
        if (Hotkeys is null || new[] { Hotkeys.AddSource, Hotkeys.AddTarget, Hotkeys.Toggle }
            .Any(s => string.IsNullOrWhiteSpace(s) || s.Length > 80))
            throw new InvalidDataException("Nieprawidłowa konfiguracja skrótów.");
    }
}

public static class RuleResolver
{
    public static bool IsSource(Profile profile, AppIdentity app) => profile.Sources.Any(s => s.Matches(app));

    public static TargetRule? TargetFor(Profile profile, AppIdentity app, bool isSelf = false)
    {
        // Source exclusion always wins, including in global mode.
        if (isSelf || IsSource(profile, app)) return null;
        return profile.Targets.FirstOrDefault(t => t.App.Matches(app))
            ?? (profile.AllExceptSources ? new TargetRule(app, profile.GlobalMode) : null);
    }
}
