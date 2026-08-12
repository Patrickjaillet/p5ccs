using System.Windows;

namespace P5CCS.Engine;

public interface IP5jsEngineHost
{
    bool IsReady { get; }

    event EventHandler? Ready;

    event EventHandler<double>? FpsChanged;

    event EventHandler<string>? ConsoleMessageReceived;

    event EventHandler<Point>? SketchMouseMoved;

    void LoadSketch(string source);

    void Run();

    void Pause();

    void Stop();

    void Reset();

    void SetFrameRate(int framesPerSecond);

    Task<byte[]> CaptureScreenshotPngAsync();
}
