using System.Text.Json;
using P5CCS.Core.Configuration;

namespace P5CCS.Core.Preferences;

public sealed class PreferencesService : IPreferencesService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _filePath;

    public PreferencesService()
        : this(AppPaths.PreferencesFilePath)
    {
    }

    public PreferencesService(string filePath)
    {
        _filePath = filePath;
        Current = LoadFromDisk(filePath);
    }

    public UserPreferences Current { get; private set; }

    public void Save()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(Current, SerializerOptions);
        File.WriteAllText(_filePath, json);
    }

    public void Reload()
    {
        Current = LoadFromDisk(_filePath);
    }

    private static UserPreferences LoadFromDisk(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new UserPreferences();
        }

        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<UserPreferences>(json, SerializerOptions) ?? new UserPreferences();
    }
}
