namespace P5CCS.Core.Configuration;

public interface IUserConfigurationService
{
    string ConfigurationFilePath { get; }

    T? Get<T>(string key, T? defaultValue = default);

    void Set<T>(string key, T value);

    void Save();
}
