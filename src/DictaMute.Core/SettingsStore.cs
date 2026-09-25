using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DictaMute.Core;

public sealed class SettingsStore(string filePath)
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };
    public string FilePath { get; } = Path.GetFullPath(filePath);
    public string? RecoveryNotice { get; private set; }

    public AppSettings Load()
    {
        if (!File.Exists(FilePath)) return new AppSettings();
        try
        {
            var settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath, Encoding.UTF8), Json)
                ?? throw new InvalidDataException("Pusta konfiguracja.");
            settings.Validate();
            return settings;
        }
        catch (Exception ex) when (ex is JsonException or InvalidDataException or NotSupportedException)
        {
            // Never silently overwrite a malformed or newer-version configuration.
            var backup = FilePath + ".corrupt-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-ffff") + ".bak";
            File.Copy(FilePath, backup);
            RecoveryNotice = $"Nie można odczytać konfiguracji. Wczytano ustawienia domyślne. Kopia: {backup}";
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        settings.Validate();
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                JsonSerializer.Serialize(stream, settings, Json);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, FilePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
