using DictaMute.Core;

// Dependency-free deterministic test runner. A failing assertion returns exit code 1.
var tests = new (string Name, Action Run)[]
{
    ("Silence does not trigger", () => Check(!Gate(new(), true, 0, 0))),
    ("An active source is required", () => Check(!Gate(new(), false, 1, 0))),
    ("Equality with threshold does not trigger", () => Check(!Gate(new(), true, .03f, 0))),
    ("Above threshold triggers immediately", () => Check(Gate(new(), true, .031f, 0))),
    ("NaN does not trigger", () => Check(!Gate(new(), true, float.NaN, 0))),
    ("Infinity does not trigger", () => Check(!Gate(new(), true, float.PositiveInfinity, 0))),
    ("Hold includes short speech gaps", () => { var g = new NoiseGate(); Gate(g, true, 1, 0); Check(Gate(g, true, 0, 1499)); }),
    ("Hold ends at the configured boundary", () => { var g = new NoiseGate(); Gate(g, true, 1, 0); Check(!Gate(g, true, 0, 1500)); }),
    ("New speech restarts hold", () => { var g = new NoiseGate(); Gate(g, true, 1, 0); Gate(g, true, 1, 1000); Check(Gate(g, true, 0, 2000)); }),
    ("Disable releases without hold", () => { var g = new NoiseGate(); Gate(g, true, 1, 0); Check(!g.Update(false, true, 1, .03f, TimeSpan.FromSeconds(2), TimeSpan.Zero)); }),
    ("Reset forgets the previous trigger", () => { var g = new NoiseGate(); Gate(g, true, 1, 0); g.Reset(); Check(!Gate(g, true, 0, 1)); }),
    ("Zero hold still triggers on signal", () => { var g = new NoiseGate(); Check(g.Update(true, true, 1, .03f, TimeSpan.Zero, TimeSpan.Zero)); Check(!g.Update(true, true, 0, .03f, TimeSpan.Zero, TimeSpan.Zero)); }),
    ("Names are case insensitive", () => Check(new AppIdentity("SPOTIFY.EXE").Matches(new("spotify")))),
    ("Different known paths do not match", () => Check(!new AppIdentity("app", @"C:\one\app.exe").Matches(new("app", @"C:\two\app.exe")))),
    ("Unknown path can match a name rule", () => Check(new AppIdentity("app").Matches(new("app", @"C:\two\app.exe")))),
    ("Explicit target is selected", () => { var app = new AppIdentity("player"); var p = new Profile { Targets = [new(app, Reaction.Mute)] }; Check(RuleResolver.TargetFor(p, app)?.Mode == Reaction.Mute); }),
    ("Global mode selects other applications", () => Check(RuleResolver.TargetFor(new() { AllExceptSources = true }, new("player")) is not null)),
    ("Global mode excludes sources", () => { var a = new AppIdentity("browser"); var p = new Profile { Sources = [a], AllExceptSources = true, Targets = [new(a)] }; Check(RuleResolver.TargetFor(p, a) is null); }),
    ("The application never targets itself", () => Check(RuleResolver.TargetFor(new() { AllExceptSources = true }, new("DictaMute"), true) is null)),
    ("Unlisted applications are unchanged", () => Check(RuleResolver.TargetFor(new(), new("player")) is null)),
    ("Ducking restores original volume", () => { var s = new FakeVolume(.8f); var l = new VolumeLease(s, Reaction.Duck); l.Apply(.2f, TimeSpan.Zero, TimeSpan.Zero); Near(.2f, s.Volume); l.Restore(TimeSpan.Zero, TimeSpan.Zero); Near(.8f, s.Volume); }),
    ("Ducking never raises a quieter application", () => { var s = new FakeVolume(.1f); new VolumeLease(s, Reaction.Duck).Apply(.2f, TimeSpan.Zero, TimeSpan.Zero); Near(.1f, s.Volume); }),
    ("Fade progresses rather than jumps", () => { var s = new FakeVolume(1); new VolumeLease(s, Reaction.Duck).Apply(0, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(200)); Near(.75f, s.Volume); }),
    ("Mute preserves an already muted session", () => { var s = new FakeVolume(.7f) { Muted = true }; var l = new VolumeLease(s, Reaction.Mute); l.Apply(0, TimeSpan.Zero, TimeSpan.Zero); l.Restore(TimeSpan.Zero, TimeSpan.Zero); Check(s.Muted); Near(.7f, s.Volume); }),
    ("Mute restores an originally unmuted session", () => { var s = new FakeVolume(.7f); var l = new VolumeLease(s, Reaction.Mute); l.Apply(0, TimeSpan.Zero, TimeSpan.Zero); Check(s.Muted); l.Restore(TimeSpan.Zero, TimeSpan.Zero); Check(!s.Muted); }),
    ("Manual volume changes are respected", () => { var s = new FakeVolume(.8f); var l = new VolumeLease(s, Reaction.Duck); l.Apply(.2f, TimeSpan.Zero, TimeSpan.Zero); s.Volume = .5f; l.Apply(.2f, TimeSpan.Zero, TimeSpan.Zero); l.Restore(TimeSpan.Zero, TimeSpan.Zero); Near(.5f, s.Volume); }),
    ("Manual unmute is respected", () => { var s = new FakeVolume(.8f); var l = new VolumeLease(s, Reaction.Mute); l.Apply(0, TimeSpan.Zero, TimeSpan.Zero); s.Muted = false; l.Apply(0, TimeSpan.Zero, TimeSpan.Zero); Check(!s.Muted); }),
    ("Duck does not change the mute bit", () => { var s = new FakeVolume(.8f) { Muted = true }; var l = new VolumeLease(s, Reaction.Duck); l.Apply(.2f, TimeSpan.Zero, TimeSpan.Zero); l.Restore(TimeSpan.Zero, TimeSpan.Zero); Check(s.Muted); }),
    ("Invalid profile is rejected", () => Throws<InvalidDataException>(() => new Profile { Threshold = float.NaN }.Validate())),
    ("Invalid active profile is rejected", () => Throws<InvalidDataException>(() => new AppSettings { ActiveProfile = 99 }.Validate())),
    ("Settings round trip and atomic overwrite", SettingsRoundTrip),
    ("Malformed settings are backed up", CorruptSettings)
};

var failures = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception ex) { failures++; Console.Error.WriteLine($"FAIL {test.Name}: {ex.Message}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} passed.");
return failures == 0 ? 0 : 1;

static bool Gate(NoiseGate gate, bool source, float peak, int milliseconds) =>
    gate.Update(true, source, peak, .03f, TimeSpan.FromMilliseconds(1500), TimeSpan.FromMilliseconds(milliseconds));
static void Check(bool condition) { if (!condition) throw new Exception("Assertion failed."); }
static void Near(float expected, float actual) => Check(Math.Abs(expected - actual) < .001f);
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}
static void SettingsRoundTrip()
{
    var directory = Path.Combine(Path.GetTempPath(), "DictaMute-test-" + Guid.NewGuid().ToString("N"));
    try
    {
        var store = new SettingsStore(Path.Combine(directory, "settings.json"));
        store.Save(new() { Enabled = false });
        Check(!store.Load().Enabled);
        store.Save(new() { ActiveProfile = 1 });
        Check(store.Load().Current.Name == "Spotkania");
        Check(Directory.GetFiles(directory, "*.tmp").Length == 0);
    }
    finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
}
static void CorruptSettings()
{
    var directory = Path.Combine(Path.GetTempPath(), "DictaMute-test-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(directory);
    try
    {
        var path = Path.Combine(directory, "settings.json");
        File.WriteAllText(path, "not-json");
        var store = new SettingsStore(path);
        store.Load().Validate();
        Check(store.RecoveryNotice is not null);
        Check(Directory.GetFiles(directory, "*.bak").Length == 1);
        Check(File.ReadAllText(path) == "not-json");
    }
    finally { Directory.Delete(directory, true); }
}

sealed class FakeVolume(float volume) : ISessionVolume
{
    public float Volume { get; set; } = volume;
    public bool Muted { get; set; }
}
