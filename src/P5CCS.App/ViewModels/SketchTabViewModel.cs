using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using P5CCS.App.Sketches;
using P5CCS.Engine;

namespace P5CCS.App.ViewModels;

public partial class SketchTabViewModel : ObservableObject
{
    private IP5jsEngineHost? _engine;

    public SketchTabViewModel(string title, string? filePath)
        : this(title, filePath, DefaultSketch.Source)
    {
    }

    public SketchTabViewModel(string title, string? filePath, string source)
    {
        _title = title;
        FilePath = filePath;
        Source = source;
    }

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

    public string? FilePath { get; set; }

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
    }

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
    }
}
