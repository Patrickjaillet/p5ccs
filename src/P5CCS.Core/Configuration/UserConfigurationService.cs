using System.Text.Json;

namespace P5CCS.Core.Configuration;

public sealed class UserConfigurationService : IUserConfigurationService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly Dictionary<string, JsonElement> _values;

    public UserConfigurationService()
        : this(AppPaths.ConfigurationFilePath)
    {
    }

    public UserConfigurationService(string configurationFilePath)
    {
        ConfigurationFilePath = configurationFilePath;
        _values = Load(configurationFilePath);
    }

    public string ConfigurationFilePath { get; }

    public T? Get<T>(string key, T? defaultValue = default)
    {
        if (!_values.TryGetValue(key, out var element))
        {
            return defaultValue;
        }

        return element.Deserialize<T>(SerializerOptions) ?? defaultValue;
    }

    public void Set<T>(string key, T value)
    {
        _values[key] = JsonSerializer.SerializeToElement(value, SerializerOptions);
    }

    public void Save()
    {
        var directory = Path.GetDirectoryName(ConfigurationFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_values, SerializerOptions);
        File.WriteAllText(ConfigurationFilePath, json);
    }

    private static Dictionary<string, JsonElement> Load(string configurationFilePath)
    {
        if (!File.Exists(configurationFilePath))
        {
            return new Dictionary<string, JsonElement>();
        }

        var json = File.ReadAllText(configurationFilePath);
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, SerializerOptions)
               ?? new Dictionary<string, JsonElement>();
    }
}
