using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using P5CCS.Core.Dialogs;
using P5CCS.Core.Preferences;
using P5CCS.Core.Theming;
using P5CCS.Core.Versioning;

namespace P5CCS.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IPreferencesService _preferencesService;
    private readonly IThemeService _themeService;
    private readonly IDialogService _dialogService;

    private int _untitledCounter = 1;

    public MainWindowViewModel(
        IVersionService versionService,
        IPreferencesService preferencesService,
        IThemeService themeService,
        IDialogService dialogService)
    {
        _preferencesService = preferencesService;
        _themeService = themeService;
        _dialogService = dialogService;

        VersionText = $"Processing 5 - Creative Coding Station v{versionService.InformationalVersion}";
        CurrentTheme = _preferencesService.Current.Theme;

        OpenSketches = new ObservableCollection<SketchTabViewModel>();
        NewSketch();

        CommandsByName = new Dictionary<string, ICommand>
        {
            ["NewSketch"] = NewSketchCommand,
            ["OpenSketch"] = OpenSketchCommand,
            ["SaveSketch"] = SaveSketchCommand,
            ["CloseTab"] = CloseTabCommand,
            ["Run"] = RunCommand,
            ["Stop"] = StopCommand,
            ["Reset"] = ResetCommand,
            ["QuickExport"] = QuickExportCommand,
            ["ToggleTheme"] = ToggleThemeCommand,
        };
    }

    public IReadOnlyDictionary<string, ICommand> CommandsByName { get; }

    [ObservableProperty]
    private string _versionText;

    [ObservableProperty]
    private AppTheme _currentTheme;

    [ObservableProperty]
    private SketchTabViewModel? _selectedSketch;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private double _fps;

    [ObservableProperty]
    private string _engineStatus = "Not Initialized";

    [ObservableProperty]
    private string _mousePositionText = "-, -";

    public ObservableCollection<SketchTabViewModel> OpenSketches { get; }

    partial void OnCurrentThemeChanged(AppTheme value)
    {
        _preferencesService.Current.Theme = value;
        _preferencesService.Save();
        _themeService.ApplyTheme(value);
    }

    [RelayCommand]
    private void NewSketch()
    {
        var tab = new SketchTabViewModel($"Untitled-{_untitledCounter++}.js", null);
        OpenSketches.Add(tab);
        SelectedSketch = tab;
    }

    [RelayCommand]
    private void OpenSketch()
    {
        var path = _dialogService.ShowOpenFileDialog("p5.js Sketch (*.js)|*.js|All files (*.*)|*.*", "Open Sketch");
        if (path is null)
        {
            return;
        }

        var existing = OpenSketches.FirstOrDefault(s => s.FilePath == path);
        if (existing is not null)
        {
            SelectedSketch = existing;
            return;
        }

        var tab = new SketchTabViewModel(Path.GetFileName(path), path);
        OpenSketches.Add(tab);
        SelectedSketch = tab;
    }

    [RelayCommand]
    private void SaveSketch()
    {
        if (SelectedSketch is null)
        {
            return;
        }

        if (SelectedSketch.FilePath is null)
        {
            SaveSketchAs();
            return;
        }

        SelectedSketch.IsModified = false;
    }

    [RelayCommand]
    private void SaveSketchAs()
    {
        if (SelectedSketch is null)
        {
            return;
        }

        var path = _dialogService.ShowSaveFileDialog("p5.js Sketch (*.js)|*.js", "Save Sketch As", SelectedSketch.Title);
        if (path is null)
        {
            return;
        }

        SelectedSketch.FilePath = path;
        SelectedSketch.Title = Path.GetFileName(path);
        SelectedSketch.IsModified = false;
    }

    [RelayCommand]
    private void CloseTab(SketchTabViewModel? tab)
    {
        tab ??= SelectedSketch;
        if (tab is null)
        {
            return;
        }

        var index = OpenSketches.IndexOf(tab);
        OpenSketches.Remove(tab);

        if (OpenSketches.Count == 0)
        {
            NewSketch();
            return;
        }

        SelectedSketch = OpenSketches[Math.Max(0, index - 1)];
    }

    [RelayCommand]
    private void CloseAllTabs()
    {
        OpenSketches.Clear();
        NewSketch();
    }

    [RelayCommand]
    private static void Exit()
    {
        Application.Current.Shutdown();
    }

    [RelayCommand]
    private void Run()
    {
        IsRunning = true;
        EngineStatus = "Running (engine not yet implemented)";
    }

    [RelayCommand]
    private void Pause()
    {
        EngineStatus = "Paused";
    }

    [RelayCommand]
    private void Stop()
    {
        IsRunning = false;
        EngineStatus = "Stopped";
    }

    [RelayCommand]
    private void Reset()
    {
        IsRunning = false;
        Fps = 0;
        EngineStatus = "Not Initialized";
    }

    [RelayCommand]
    private void QuickExport()
    {
        _dialogService.ShowSaveFileDialog("PNG Image (*.png)|*.png", "Quick Export", "export.png");
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        CurrentTheme = CurrentTheme switch
        {
            AppTheme.Light => AppTheme.Dark,
            AppTheme.Dark => AppTheme.System,
            _ => AppTheme.Light,
        };
    }

    [RelayCommand]
    private void SetLightTheme() => CurrentTheme = AppTheme.Light;

    [RelayCommand]
    private void SetDarkTheme() => CurrentTheme = AppTheme.Dark;

    [RelayCommand]
    private void SetSystemTheme() => CurrentTheme = AppTheme.System;

    [RelayCommand]
    private static void ShowAbout()
    {
        MessageBox.Show(
            "Processing 5 - Creative Coding Station\nCopyright (c) 2026 Patrick JAILLET",
            "About",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
