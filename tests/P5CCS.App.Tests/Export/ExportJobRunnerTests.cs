using System.IO;
using P5CCS.App.Export;
using P5CCS.Engine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace P5CCS.App.Tests.Export;

public class ExportJobRunnerTests
{
    [Fact]
    public async Task RunAsync_PngExport_WritesFrameToOutputPath()
    {
        var engine = new FakeP5jsEngineHost();
        var runner = new ExportJobRunner();
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-job-{Guid.NewGuid():N}.png");
        var request = new ExportRequest(ExportFormat.Png, Width: 100, Height: 80, Fps: 30, DurationSeconds: 0, outputPath);

        try
        {
            await runner.RunAsync(engine, request);

            Assert.True(File.Exists(outputPath));
            using var image = await Image.LoadAsync<Rgba32>(outputPath);
            Assert.Equal(100, image.Width);
            Assert.Equal(80, image.Height);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task RunAsync_GifExport_ProducesFrameCountMatchingDurationAndFps()
    {
        var engine = new FakeP5jsEngineHost();
        var runner = new ExportJobRunner();
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-job-{Guid.NewGuid():N}.gif");
        var request = new ExportRequest(ExportFormat.Gif, Width: 40, Height: 30, Fps: 10, DurationSeconds: 0.5, outputPath);

        try
        {
            await runner.RunAsync(engine, request);

            using var image = await Image.LoadAsync<Rgba32>(outputPath);
            Assert.Equal(5, image.Frames.Count);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task RunAsync_ResizesCanvasToRequestedDimensionsBeforeCapture()
    {
        var engine = new FakeP5jsEngineHost();
        var runner = new ExportJobRunner();
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-job-{Guid.NewGuid():N}.png");
        var request = new ExportRequest(ExportFormat.Png, Width: 1920, Height: 1080, Fps: 30, DurationSeconds: 0, outputPath);

        try
        {
            await runner.RunAsync(engine, request);

            Assert.Equal((1920, 1080), engine.ResizeCalls[0]);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task RunAsync_WithRestoreSize_ResizesBackAfterExportCompletes()
    {
        var engine = new FakeP5jsEngineHost();
        var runner = new ExportJobRunner();
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-job-{Guid.NewGuid():N}.png");
        var request = new ExportRequest(ExportFormat.Png, Width: 1920, Height: 1080, Fps: 30, DurationSeconds: 0, outputPath);

        try
        {
            await runner.RunAsync(engine, request, restoreWidth: 800, restoreHeight: 450);

            Assert.Equal(2, engine.ResizeCalls.Count);
            Assert.Equal((1920, 1080), engine.ResizeCalls[0]);
            Assert.Equal((800, 450), engine.ResizeCalls[1]);
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task RunAsync_Cancelled_StillRestoresCanvasSize()
    {
        var engine = new FakeP5jsEngineHost();
        var runner = new ExportJobRunner();
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-job-{Guid.NewGuid():N}.gif");
        var request = new ExportRequest(ExportFormat.Gif, Width: 40, Height: 30, Fps: 10, DurationSeconds: 5, outputPath);
        using var cts = new CancellationTokenSource();
        engine.CancelAfterFrames = 3;
        engine.CancellationSource = cts;

        var exception = await Record.ExceptionAsync(() =>
            runner.RunAsync(engine, request, restoreWidth: 800, restoreHeight: 450, cancellationToken: cts.Token));

        Assert.IsType<OperationCanceledException>(exception);
        Assert.Equal((800, 450), engine.ResizeCalls[^1]);
    }

    private sealed class FakeP5jsEngineHost : IP5jsEngineHost
    {
        private int _captureCount;
        private int _currentWidth = 800;
        private int _currentHeight = 450;

        public List<(int Width, int Height)> ResizeCalls { get; } = new();

        public int? CancelAfterFrames { get; set; }

        public CancellationTokenSource? CancellationSource { get; set; }

        public bool IsReady => true;

        public event EventHandler? Ready;

#pragma warning disable CS0067 // required by IP5jsEngineHost, unused by this test fake
        public event EventHandler<double>? FpsChanged;

        public event EventHandler<string>? ConsoleMessageReceived;

        public event EventHandler<System.Windows.Point>? SketchMouseMoved;
#pragma warning restore CS0067

        public void LoadSketch(string source)
        {
        }

        public void SetAssetDirectory(string? directoryPath)
        {
        }

        public void Run() => Ready?.Invoke(this, EventArgs.Empty);

        public void Pause()
        {
        }

        public void Stop()
        {
        }

        public void Reset()
        {
        }

        public void SetFrameRate(int framesPerSecond)
        {
        }

        public void SetGlobalNumber(string name, double value)
        {
        }

        public Task<byte[]> CaptureScreenshotPngAsync() => Task.FromResult(Array.Empty<byte>());

        public Task BeginExportAsync() => Task.CompletedTask;

        public Task<byte[]> CaptureExportFrameAsync(double virtualMillis)
        {
            _captureCount++;
            if (CancelAfterFrames is int limit && _captureCount >= limit)
            {
                CancellationSource?.Cancel();
            }

            var shade = (byte)(virtualMillis % 255);
            return Task.FromResult(CreateSolidColorPng(new Rgba32(shade, 0, (byte)(255 - shade), 255), _currentWidth, _currentHeight));
        }

        public Task EndExportAsync() => Task.CompletedTask;

        public Task ResizeCanvasForExportAsync(int width, int height)
        {
            ResizeCalls.Add((width, height));
            _currentWidth = width;
            _currentHeight = height;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }

        private static byte[] CreateSolidColorPng(Rgba32 pixel, int width, int height)
        {
            using var image = new Image<Rgba32>(width, height);
            for (var y = 0; y < image.Height; y++)
            {
                for (var x = 0; x < image.Width; x++)
                {
                    image[x, y] = pixel;
                }
            }

            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            return stream.ToArray();
        }
    }
}
