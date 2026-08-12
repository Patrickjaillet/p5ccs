using P5CCS.Engine;
using P5CCS.Export;

namespace P5CCS.App.Export;

public sealed class EngineFrameSourceAdapter : IExportFrameSource
{
    private readonly IP5jsEngineHost _engine;

    public EngineFrameSourceAdapter(IP5jsEngineHost engine)
    {
        _engine = engine;
    }

    public Task BeginExportAsync() => _engine.BeginExportAsync();

    public Task<byte[]> CaptureExportFrameAsync(double virtualMillis) => _engine.CaptureExportFrameAsync(virtualMillis);

    public Task EndExportAsync() => _engine.EndExportAsync();

    public Task ResizeCanvasForExportAsync(int width, int height) => _engine.ResizeCanvasForExportAsync(width, height);
}
