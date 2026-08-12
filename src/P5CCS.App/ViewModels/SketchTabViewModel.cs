using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using P5CCS.App.Sketches;
using P5CCS.Core.Configuration;
using P5CCS.Core.Preferences;
using P5CCS.Editor.ErrorMarkers;
using P5CCS.Engine;

namespace P5CCS.App.ViewModels;

public partial class SketchTabViewModel : ObservableObject, IDisposable
{
    private readonly IPreferencesService _preferencesService;
    private readonly DispatcherTimer _autoSaveTimer;
    private IP5jsEngineHost? _engine;

    public SketchTabViewModel(string title, string? filePath, IPreferencesService preferencesService)
        : this(title, filePath, DefaultSketch.Source, preferencesService)
    {
    }

    public SketchTabViewModel(string title, string? filePath, string source, IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
        _title = title;
        FilePath = filePath;
        Source = source;

        _autoSaveTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(Math.Max(5, _preferencesService.Current.AutoSaveIntervalSeconds)),
        };
        _autoSaveTimer.Tick += (_, _) => AutoSave();
        if (_preferencesService.Current.AutoSaveEnabled)
        {
            _autoSaveTimer.Start();
        }
    }

    public Guid Id { get; } = Guid.NewGuid();

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private string _source;

    [ObservableProperty]
    private double _fps;

    [ObservableProperty]
    private string _engineStatus = "Not Initialized";

    [ObservableProperty]
    private string _mousePositionText = "-, -";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private int _targetFrameRate = 60;

    public ObservableCollection<string> ConsoleLines { get; } = new();

    public string? FilePath { get; set; }

    public string RecoveryFilePath => Path.Combine(AppPaths.RecoveryDirectory, $"{Id:N}.js");

    public event EventHandler<IReadOnlyList<EditorErrorMarker>>? ErrorMarkersChanged;

    partial void OnTargetFrameRateChanged(int value) => Engine?.SetFrameRate(value);

    public IP5jsEngineHost? Engine
    {
        get => _engine;
        set
        {
            if (ReferenceEquals(_engine, value))
            {
                return;
            }

            if (_engine is not null)
            {
                _engine.Ready -= OnEngineReady;
                _engine.FpsChanged -= OnFpsChanged;
                _engine.SketchMouseMoved -= OnMouseMoved;
                _engine.ConsoleMessageReceived -= OnConsoleMessage;
            }

            _engine = value;

            if (_engine is not null)
            {
                _engine.Ready += OnEngineReady;
                _engine.FpsChanged += OnFpsChanged;
                _engine.SketchMouseMoved += OnMouseMoved;
                _engine.ConsoleMessageReceived += OnConsoleMessage;
                _engine.LoadSketch(Source);
                _engine.Run();
                EngineStatus = "Starting";
            }
        }
    }

    public void Run()
    {
        Engine?.Run();
        EngineStatus = "Running";
        IsRunning = true;
    }

    public void Pause()
    {
        Engine?.Pause();
        EngineStatus = "Paused";
        IsRunning = false;
    }

    public void Stop()
    {
        Engine?.Stop();
        EngineStatus = "Stopped";
        IsRunning = false;
        Fps = 0;
    }

    public void Reset()
    {
        Engine?.Reset();
        EngineStatus = "Starting";
        IsRunning = true;
        Fps = 0;
        ClearErrors();
    }

    public void UpdateSourceFromEditor(string newSource)
    {
        if (Source == newSource)
        {
            return;
        }

        Source = newSource;
        IsModified = true;

        if (_preferencesService.Current.LiveReloadEnabled && Engine is not null)
        {
            ClearErrors();
            Engine.LoadSketch(Source);
            Engine.Reset();
            EngineStatus = "Starting";
            IsRunning = true;
        }
    }

    public void AutoSave()
    {
        try
        {
            AppPaths.EnsureDirectoriesExist();
            File.WriteAllText(RecoveryFilePath, Source);

            if (FilePath is not null && IsModified)
            {
                File.WriteAllText(FilePath, Source);
                IsModified = false;
            }
        }
        catch (IOException)
        {
        }
    }

    public void DeleteRecoveryFile()
    {
        try
        {
            if (File.Exists(RecoveryFilePath))
            {
                File.Delete(RecoveryFilePath);
            }
        }
        catch (IOException)
        {
        }
    }

    public void Dispose()
    {
        _autoSaveTimer.Stop();
        DeleteRecoveryFile();
    }

    private void ClearErrors() => ErrorMarkersChanged?.Invoke(this, Array.Empty<EditorErrorMarker>());

    private void OnEngineReady(object? sender, EventArgs e)
    {
        EngineStatus = "Running";
        IsRunning = true;
        Engine?.SetFrameRate(TargetFrameRate);
    }

    private void OnFpsChanged(object? sender, double fps) => Fps = fps;

    private void OnMouseMoved(object? sender, Point position) => MousePositionText = $"{position.X:0}, {position.Y:0}";

    private void OnConsoleMessage(object? sender, string message)
    {
        ConsoleLines.Add(message);
        while (ConsoleLines.Count > 500)
        {
            ConsoleLines.RemoveAt(0);
        }

        var line = TryExtractSketchLine(message);
        if (line is not null)
        {
            ErrorMarkersChanged?.Invoke(this, new[] { new EditorErrorMarker(line.Value, message) });
        }
    }

    private static int? TryExtractSketchLine(string message)
    {
        var markerIndex = message.IndexOf("sketch.js:", StringComparison.OrdinalIgnoreCase);
        if (markerIndex < 0)
        {
            return null;
        }

        var afterColon = message[(markerIndex + "sketch.js:".Length)..];
        var digits = new string(afterColon.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var line) ? line : null;
    }
}
