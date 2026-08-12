using System.Windows.Media;
using P5CCS.Core.Preferences;
using P5CCS.Core.Theming;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace P5CCS.App.Services;

public sealed class WpfThemeService : IThemeService
{
    private readonly IPreferencesService _preferencesService;

    public WpfThemeService(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    public AppTheme CurrentTheme { get; private set; } = AppTheme.System;

    public void ApplyTheme(AppTheme theme)
    {
        CurrentTheme = theme;

        var applicationTheme = theme switch
        {
            AppTheme.Light => ApplicationTheme.Light,
            AppTheme.Dark => ApplicationTheme.Dark,
            _ => ApplicationThemeManager.GetSystemTheme() switch
            {
                SystemTheme.Light => ApplicationTheme.Light,
                _ => ApplicationTheme.Dark,
            },
        };

        ApplicationThemeManager.Apply(applicationTheme, WindowBackdropType.Mica, updateAccent: false);
        ApplyAccentColor(_preferencesService.Current.AccentColorHex);
    }

    public void ApplyAccentColor(string hexColor)
    {
        var color = (Color)ColorConverter.ConvertFromString(hexColor);
        ApplicationAccentColorManager.Apply(color, ApplicationThemeManager.GetAppTheme());
    }
}
