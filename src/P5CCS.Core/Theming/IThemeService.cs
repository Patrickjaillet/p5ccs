using P5CCS.Core.Preferences;

namespace P5CCS.Core.Theming;

public interface IThemeService
{
    AppTheme CurrentTheme { get; }

    void ApplyTheme(AppTheme theme);

    void ApplyAccentColor(string hexColor);
}
