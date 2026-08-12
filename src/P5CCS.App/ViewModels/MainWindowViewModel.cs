using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using P5CCS.Core.Configuration;
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
    private readonly Func<AboutWindow> _aboutWindowFactory;

    private int _untitledCounter = 1;

    public MainWindowViewModel(
        IVersionService versionService,
        IPreferencesService preferencesService,
        IThemeService themeService,
        IDialogService dialogService,
        Func<AboutWindow> aboutWindowFactory)
    {
        _preferencesService = preferencesService;
        _themeService = themeService;
        _dialogService = dialogService;
        _aboutWindowFactory = aboutWindowFactory;

        VersionText = $"Processing 5 - Creative Coding Station v{versionService.InformationalVersion}";
        CurrentTheme = _preferencesService.Current.Theme;

        OpenSketches = new ObservableCollection<SketchTabViewModel>();
        RecoverOrphanedSketches();
        if (OpenSketches.Count == 0)
        {
            NewSketch();
        }

        CommandsByName = new Dictionary<string, ICommand>
        {
            ["NewSketch"] = NewSketchCommand,
            ["OpenSketch"] = OpenSketchCommand,
            ["SaveSketch"] = SaveSketchCommand,
            ["CloseTab"] = CloseTabCommand,
            ["Run"] = RunCommand,
            ["Stop"] = StopCommand,
            ["Reset"] = ResetCommand,
            ["OpenExport"] = OpenExportCommand,
            ["QuickExport"] = QuickExportCommand,
            ["ToggleTheme"] = ToggleThemeCommand,
            ["FullscreenViewport"] = FullscreenViewportCommand,
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
    private bool _showGrid;

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
        var tab = new SketchTabViewModel($"Untitled-{_untitledCounter++}.js", null, _preferencesService, _dialogService);
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

        var source = File.Exists(path) ? File.ReadAllText(path) : Sketches.DefaultSketch.Source;
        var tab = new SketchTabViewModel(Path.GetFileName(path), path, source, _preferencesService, _dialogService);
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

        File.WriteAllText(SelectedSketch.FilePath, SelectedSketch.Source);
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

        File.WriteAllText(path, SelectedSketch.Source);
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
        tab.Dispose();

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
        foreach (var tab in OpenSketches)
        {
            tab.Dispose();
        }

        OpenSketches.Clear();
        NewSketch();
    }

    private void RecoverOrphanedSketches()
    {
        AppPaths.EnsureDirectoriesExist();

        var recoveryFiles = Directory.GetFiles(AppPaths.RecoveryDirectory, "*.js");
        var recoveredIndex = 1;

        foreach (var recoveryFile in recoveryFiles)
        {
            string source;
            try
            {
                source = File.ReadAllText(recoveryFile);
            }
            catch (IOException)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                File.Delete(recoveryFile);
                continue;
            }

            var tab = new SketchTabViewModel($"Recovered-{recoveredIndex++}.js", null, source, _preferencesService, _dialogService)
            {
                IsModified = true,
            };
            OpenSketches.Add(tab);

            File.Delete(recoveryFile);
        }

        if (OpenSketches.Count > 0)
        {
            SelectedSketch = OpenSketches[0];
        }
    }

    [RelayCommand]
    private static void Exit()
    {
        Application.Current.Shutdown();
    }

    [RelayCommand]
    private void Run() => SelectedSketch?.Run();

    [RelayCommand]
    private void Pause() => SelectedSketch?.Pause();

    [RelayCommand]
    private void Stop() => SelectedSketch?.Stop();

    [RelayCommand]
    private void Reset() => SelectedSketch?.Reset();

    [RelayCommand]
    private void OpenExport()
    {
        if (SelectedSketch is null)
        {
            return;
        }

        var viewModel = new ExportViewModel(SelectedSketch, _dialogService);
        var window = new ExportWindow(viewModel) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    [RelayCommand]
    private async Task QuickExport()
    {
        if (SelectedSketch?.Engine is null)
        {
            return;
        }

        var path = _dialogService.ShowSaveFileDialog("PNG Image (*.png)|*.png", "Quick Export", $"{Path.GetFileNameWithoutExtension(SelectedSketch.Title)}.png");
        if (path is null)
        {
            return;
        }

        var pngBytes = await SelectedSketch.Engine.CaptureScreenshotPngAsync();
        if (pngBytes.Length > 0)
        {
            await File.WriteAllBytesAsync(path, pngBytes);
        }
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
    private void ShowAbout()
    {
        var aboutWindow = _aboutWindowFactory();
        aboutWindow.Owner = Application.Current.MainWindow;
        aboutWindow.ShowDialog();
    }

    [RelayCommand]
    private void FullscreenViewport()
    {
        if (SelectedSketch is null)
        {
            return;
        }

        var window = new FullscreenViewportWindow(SelectedSketch.Source)
        {
            Owner = Application.Current.MainWindow,
        };
        window.Show();
    }
}
