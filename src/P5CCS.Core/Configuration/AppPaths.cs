namespace P5CCS.Core.Configuration;

public static class AppPaths
{
    private const string ApplicationFolderName = "P5CCS";

    public static string RootDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        ApplicationFolderName);

    public static string LogsDirectory => Path.Combine(RootDirectory, "logs");

    public static string PreferencesFilePath => Path.Combine(RootDirectory, "preferences.json");

    public static string ConfigurationFilePath => Path.Combine(RootDirectory, "config.json");

    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(RootDirectory);
        Directory.CreateDirectory(LogsDirectory);
    }
}
