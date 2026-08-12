namespace P5CCS.Export.Tests;

public class FrameCaptureServiceTests
{
    [Fact]
    public async Task CaptureFramesAsync_ProducesExactlyRequestedFrameCount()
    {
        var source = new FakeExportFrameSource();
        var service = new FrameCaptureService();
        var request = new ExportFrameRequest(Width: 400, Height: 300, Fps: 30, FrameCount: 10);

        var frames = new List<byte[]>();
        await foreach (var frame in service.CaptureFramesAsync(source, request))
        {
            frames.Add(frame);
        }

        Assert.Equal(10, frames.Count);
    }

    [Fact]
    public async Task CaptureFramesAsync_RequestsVirtualMillisecondsAtFixedFrameInterval()
    {
        var source = new FakeExportFrameSource();
        var service = new FrameCaptureService();
        var request = new ExportFrameRequest(Width: 100, Height: 100, Fps: 25, FrameCount: 4);

        await foreach (var _ in service.CaptureFramesAsync(source, request))
        {
        }

        Assert.Equal(new[] { 0.0, 40.0, 80.0, 120.0 }, source.CapturedVirtualMillis);
    }

    [Fact]
    public async Task CaptureFramesAsync_CallsLifecycleMethodsInOrder()
    {
        var source = new FakeExportFrameSource();
        var service = new FrameCaptureService();
        var request = new ExportFrameRequest(Width: 640, Height: 480, Fps: 10, FrameCount: 2);

        await foreach (var _ in service.CaptureFramesAsync(source, request))
        {
        }

        Assert.Equal(
            new[] { "Resize(640,480)", "Begin", "Capture(0)", "Capture(100)", "End" },
            source.CallLog);
    }

    [Fact]
    public async Task CaptureFramesAsync_ZeroFrameCount_ProducesNoFramesAndSkipsLifecycle()
    {
        var source = new FakeExportFrameSource();
        var service = new FrameCaptureService();
        var request = new ExportFrameRequest(Width: 100, Height: 100, Fps: 30, FrameCount: 0);

        var frames = new List<byte[]>();
        await foreach (var frame in service.CaptureFramesAsync(source, request))
        {
            frames.Add(frame);
        }

        Assert.Empty(frames);
        Assert.Empty(source.CallLog);
    }

    [Fact]
    public async Task CaptureFramesAsync_ReportsProgressAfterEachFrame()
    {
        var source = new FakeExportFrameSource();
        var service = new FrameCaptureService();
        var request = new ExportFrameRequest(Width: 100, Height: 100, Fps: 30, FrameCount: 3);
        var reports = new List<ExportProgress>();
        var progress = new Progress<ExportProgress>(reports.Add);

        await foreach (var _ in service.CaptureFramesAsync(source, request, progress))
        {
        }

        // Progress<T> callbacks are marshaled asynchronously; give them a moment to flush.
        await Task.Delay(50);

        Assert.Equal(3, reports.Count);
        Assert.Equal(1, reports[0].CompletedFrames);
        Assert.Equal(3, reports[2].CompletedFrames);
        Assert.Equal(3, reports[2].TotalFrames);
        Assert.Equal(1.0, reports[2].FractionComplete);
    }

    [Fact]
    public async Task CaptureFramesAsync_CancellationStopsEarlyButStillEndsExport()
    {
        var source = new FakeExportFrameSource();
        var service = new FrameCaptureService();
        var request = new ExportFrameRequest(Width: 100, Height: 100, Fps: 30, FrameCount: 100);
        using var cts = new CancellationTokenSource();

        var frames = new List<byte[]>();
        var exception = await Record.ExceptionAsync(async () =>
        {
            await foreach (var frame in service.CaptureFramesAsync(source, request, cancellationToken: cts.Token))
            {
                frames.Add(frame);
                if (frames.Count == 5)
                {
                    cts.Cancel();
                }
            }
        });

        Assert.IsType<OperationCanceledException>(exception);
        Assert.Equal(5, frames.Count);
        Assert.Contains("End", source.CallLog);
    }

    [Fact]
    public async Task CaptureFramesAsync_EachCapturedFrameMatchesFakeSourceOutput()
    {
        var source = new FakeExportFrameSource();
        var service = new FrameCaptureService();
        var request = new ExportFrameRequest(Width: 100, Height: 100, Fps: 10, FrameCount: 2);

        var frames = new List<byte[]>();
        await foreach (var frame in service.CaptureFramesAsync(source, request))
        {
            frames.Add(frame);
        }

        Assert.Equal(new byte[] { 0 }, frames[0]);
        Assert.Equal(new byte[] { 1 }, frames[1]);
    }

    private sealed class FakeExportFrameSource : IExportFrameSource
    {
        private int _captureCount;

        public List<double> CapturedVirtualMillis { get; } = new();

        public List<string> CallLog { get; } = new();

        public Task ResizeCanvasForExportAsync(int width, int height)
        {
            CallLog.Add($"Resize({width},{height})");
            return Task.CompletedTask;
        }

        public Task BeginExportAsync()
        {
            CallLog.Add("Begin");
            return Task.CompletedTask;
        }

        public Task<byte[]> CaptureExportFrameAsync(double virtualMillis)
        {
            CapturedVirtualMillis.Add(virtualMillis);
            CallLog.Add($"Capture({virtualMillis})");
            return Task.FromResult(new[] { (byte)_captureCount++ });
        }

        public Task EndExportAsync()
        {
            CallLog.Add("End");
            return Task.CompletedTask;
        }
    }
}
