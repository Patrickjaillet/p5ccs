using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;
using P5CCS.Engine.Http;

namespace P5CCS.Engine;

public partial class SketchViewport : UserControl, IP5jsEngineHost, IDisposable
{
    private const double MinZoom = 0.25;
    private const double MaxZoom = 4.0;
    private const double GridSpacing = 50;

    private readonly LocalSketchServer _server;
    private string _source = string.Empty;
    private bool _isNavigated;
    private bool _isPaused;
    private bool _isInitialized;
    private bool _isDisposed;

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

    public double Zoom
    {
        get => ZoomTransform.ScaleX;
        set
        {
            var clamped = Math.Clamp(value, MinZoom, MaxZoom);
            ZoomTransform.ScaleX = clamped;
            ZoomTransform.ScaleY = clamped;
        }
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
        DrawGrid();
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
                    Dispatcher.Invoke(() => Ready?.Invoke(this, EventArgs.Empty));
                    break;

                case "fps":
                    var fps = document.RootElement.GetProperty("value").GetDouble();
                    Dispatcher.Invoke(() =>
                    {
                        FpsOverlay.Text = $"{fps:0} FPS";
                        FpsChanged?.Invoke(this, fps);
                    });
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

        Zoom += e.Delta > 0 ? 0.1 : -0.1;
        e.Handled = true;
    }

    private static void OnShowGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SketchViewport)d).DrawGrid();
    }

    private void DrawGrid()
    {
        OverlayCanvas.Children.Clear();

        if (!ShowGrid)
        {
            return;
        }

        var brush = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255));

        for (double x = 0; x <= 800; x += GridSpacing)
        {
            OverlayCanvas.Children.Add(new Line
            {
                X1 = x,
                Y1 = 0,
                X2 = x,
                Y2 = 450,
                Stroke = brush,
                StrokeThickness = 1,
            });
        }

        for (double y = 0; y <= 450; y += GridSpacing)
        {
            OverlayCanvas.Children.Add(new Line
            {
                X1 = 0,
                Y1 = y,
                X2 = 800,
                Y2 = y,
                Stroke = brush,
                StrokeThickness = 1,
            });
        }
    }
}
