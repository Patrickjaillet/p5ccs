namespace P5CCS.Core.Preferences;

public sealed class UserPreferences
{
    public AppTheme Theme { get; set; } = AppTheme.System;

    public string UiLanguage { get; set; } = "en";

    public string? LastProjectPath { get; set; }

    public string? PanelLayout { get; set; }

    public string AccentColorHex { get; set; } = "#0078D4";

    public bool AutoSaveEnabled { get; set; } = true;

    public int AutoSaveIntervalSeconds { get; set; } = 30;

    public bool LiveReloadEnabled { get; set; } = true;
}
