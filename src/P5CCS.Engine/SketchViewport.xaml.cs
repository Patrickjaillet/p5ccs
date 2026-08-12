using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using P5CCS.Engine.Http;

namespace P5CCS.Engine;

public partial class SketchViewport : UserControl, IP5jsEngineHost, IDisposable
{
    private const double MinZoom = 0.25;
    private const double MaxZoom = 4.0;

    private readonly LocalSketchServer _server;
    private string _source = string.Empty;
    private bool _isNavigated;
    private bool _isPaused;
    private bool _isInitialized;
    private bool _isDisposed;
    private double _fitScale = 1.0;
    private double _userZoomMultiplier = 1.0;

    public SketchViewport()
    {
        InitializeComponent();
        _server = new LocalSketchServer(() => _source);
        Loaded += OnLoaded;
    }

    public bool IsReady { get; private set; }

    public event EventHandler? Ready;

    public event EventHandler<double>? FpsChanged;

    public event EventHandler<string>? ConsoleMessageReceived;

    public event EventHandler<Point>? SketchMouseMoved;

    public static readonly DependencyProperty ShowGridProperty = DependencyProperty.Register(
        nameof(ShowGrid), typeof(bool), typeof(SketchViewport),
        new PropertyMetadata(false, OnShowGridChanged));

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public void LoadSketch(string source)
    {
        _source = source;
        if (_isNavigated)
        {
            WebView.CoreWebView2?.Reload();
        }
    }

    public void Run()
    {
        if (!_isNavigated)
        {
            NavigateAsync();
            return;
        }

        if (_isPaused)
        {
            PostCommand("loop");
            _isPaused = false;
        }
    }

    public void Pause()
    {
        PostCommand("noLoop");
        _isPaused = true;
    }

    public void Stop()
    {
        _isNavigated = false;
        IsReady = false;
        _isPaused = false;
        WebView.CoreWebView2?.NavigateToString("<html><body style=\"background:#1e1e1e\"></body></html>");
    }

    public void Reset()
    {
        if (_isNavigated)
        {
            _isPaused = false;
            WebView.CoreWebView2?.Reload();
        }
        else
        {
            NavigateAsync();
        }
    }

    public void SetFrameRate(int framesPerSecond) => PostCommand("setFrameRate", framesPerSecond);

    public void SetGlobalNumber(string name, double value) => PostCommand("setVariable", name, value);

    public async Task<byte[]> CaptureScreenshotPngAsync()
    {
        if (WebView.CoreWebView2 is null)
        {
            return Array.Empty<byte>();
        }

        using var stream = new MemoryStream();
        await WebView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);
        return stream.ToArray();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_isInitialized || _isDisposed)
        {
            return;
        }

        _isInitialized = true;

        _server.Start();
        await WebView.EnsureCoreWebView2Async();
        WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        UpdateFitScale(new Size(ViewportContainer.ActualWidth, ViewportContainer.ActualHeight));
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _server.Dispose();
    }

    private async void NavigateAsync()
    {
        await WebView.EnsureCoreWebView2Async();
        _isNavigated = true;
        _isPaused = false;
        WebView.CoreWebView2.Navigate(_server.BaseUri.ToString());
    }

    private void PostCommand(string command, string name, double value)
    {
        if (WebView.CoreWebView2 is null)
        {
            return;
        }

        var payload = JsonSerializer.Serialize(new { command, name, value });
        WebView.CoreWebView2.PostWebMessageAsJson(payload);
    }

    private void PostCommand(string command, object? value = null)
    {
        if (WebView.CoreWebView2 is null)
        {
            return;
        }

        var payload = value is null
            ? JsonSerializer.Serialize(new { command })
            : JsonSerializer.Serialize(new { command, value });

        WebView.CoreWebView2.PostWebMessageAsJson(payload);
    }

    private void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(e.WebMessageAsJson);
        }
        catch (JsonException)
        {
            return;
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("type", out var typeElement))
            {
                return;
            }

            switch (typeElement.GetString())
            {
                case "ready":
                    IsReady = true;
                    Dispatcher.Invoke(() =>
                    {
                        Ready?.Invoke(this, EventArgs.Empty);
                        PostCommand("showGrid", ShowGrid);
                    });
                    break;

                case "fps":
                    var fps = document.RootElement.GetProperty("value").GetDouble();
                    Dispatcher.Invoke(() => FpsChanged?.Invoke(this, fps));
                    break;

                case "mouse":
                    var x = document.RootElement.GetProperty("x").GetDouble();
                    var y = document.RootElement.GetProperty("y").GetDouble();
                    Dispatcher.Invoke(() => SketchMouseMoved?.Invoke(this, new Point(x, y)));
                    break;

                case "console":
                case "console-error":
                case "error":
                    var message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
                    Dispatcher.Invoke(() => ConsoleMessageReceived?.Invoke(this, message));
                    break;
            }
        }
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control)
        {
            return;
        }

        _userZoomMultiplier = Math.Clamp(_userZoomMultiplier + (e.Delta > 0 ? 0.1 : -0.1), MinZoom, MaxZoom);
        ApplyZoom();
        e.Handled = true;
    }

    private static void OnShowGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var viewport = (SketchViewport)d;
        if (viewport._isNavigated)
        {
            viewport.PostCommand("showGrid", (bool)e.NewValue);
        }
    }

    private void OnContainerSizeChanged(object sender, SizeChangedEventArgs e) => UpdateFitScale(e.NewSize);

    private void UpdateFitScale(Size available)
    {
        if (available.Width <= 0 || available.Height <= 0 || RenderRoot.Width <= 0 || RenderRoot.Height <= 0)
        {
            return;
        }

        var scale = Math.Min(available.Width / RenderRoot.Width, available.Height / RenderRoot.Height);
        if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0)
        {
            scale = 1.0;
        }

        _fitScale = scale;
        ApplyZoom();
    }

    private void ApplyZoom()
    {
        var effective = Math.Clamp(_fitScale * _userZoomMultiplier, MinZoom, MaxZoom);
        ZoomTransform.ScaleX = effective;
        ZoomTransform.ScaleY = effective;
    }
}
