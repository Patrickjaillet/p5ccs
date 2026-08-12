using CommunityToolkit.Mvvm.ComponentModel;
using P5CCS.Core.Preferences;
using P5CCS.Core.Versioning;

namespace P5CCS.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IPreferencesService _preferencesService;

    public MainWindowViewModel(IVersionService versionService, IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
        VersionText = $"Processing 5 - Creative Coding Station v{versionService.InformationalVersion}";
        CurrentTheme = _preferencesService.Current.Theme;
    }

    [ObservableProperty]
    private string _versionText;

    [ObservableProperty]
    private AppTheme _currentTheme;

    partial void OnCurrentThemeChanged(AppTheme value)
    {
        _preferencesService.Current.Theme = value;
        _preferencesService.Save();
    }
}
