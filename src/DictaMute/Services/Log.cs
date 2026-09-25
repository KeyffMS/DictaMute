namespace DictaMute.Services;

internal static class Log
{
    private static readonly object Sync = new();
    public static string DirectoryPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DictaMute");

    public static void Write(string message, Exception? error = null)
    {
        lock (Sync)
        {
            try
            {
                Directory.CreateDirectory(DirectoryPath);
                var path = Path.Combine(DirectoryPath, "DictaMute.log");
                if (File.Exists(path) && new FileInfo(path).Length > 1_048_576)
                    File.Move(path, path + ".1", overwrite: true);
                File.AppendAllText(path, $"{DateTimeOffset.Now:O} {message} {error}\n");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Logging must not prevent restoring an application's audio state.
            }
        }
    }
}
