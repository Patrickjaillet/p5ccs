namespace P5CCS.Core.Preferences;

public interface IPreferencesService
{
    UserPreferences Current { get; }

    void Save();

    void Reload();
}
