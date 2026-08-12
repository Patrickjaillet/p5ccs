using P5CCS.Engine;
using P5CCS.Export;

namespace P5CCS.App.Export;

public sealed class ExportJobRunner
{
    private readonly FrameCaptureService _frameCaptureService = new();

    public async Task<string?> RunAsync(
        IP5jsEngineHost engine,
        ExportRequest request,
        int? restoreWidth = null,
        int? restoreHeight = null,
        IProgress<P5CCS.Export.ExportProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var adapter = new EngineFrameSourceAdapter(engine);
        var frameRequest = new ExportFrameRequest(request.Width, request.Height, request.Fps, request.FrameCount);

        var frames = new List<byte[]>();
        try
        {
            await foreach (var frame in _frameCaptureService.CaptureFramesAsync(adapter, frameRequest, progress, cancellationToken))
            {
                frames.Add(frame);
            }

            return await ExportFramesAsync(frames, request, cancellationToken);
        }
        finally
        {
            if (restoreWidth is int width && restoreHeight is int height)
            {
                await adapter.ResizeCanvasForExportAsync(width, height);
            }
        }
    }

    private static async Task<string?> ExportFramesAsync(IReadOnlyList<byte[]> frames, ExportRequest request, CancellationToken cancellationToken)
    {
        switch (request.Format)
        {
            case ExportFormat.Png:
                await StillImageExporter.ExportAsync(frames[0], request.OutputPath, StillImageFormat.Png, cancellationToken: cancellationToken);
                return null;
            case ExportFormat.Jpeg:
                await StillImageExporter.ExportAsync(frames[0], request.OutputPath, StillImageFormat.Jpeg, request.JpegQuality, cancellationToken);
                return null;
            case ExportFormat.Gif:
                await GifExporter.ExportAsync(frames, request.Fps, request.OutputPath, request.GifColorCount, cancellationToken);
                return null;
            case ExportFormat.WebM:
                return await VideoExporter.ExportAsync(frames, request.Fps, request.OutputPath, P5CCS.Export.VideoFormat.WebM, request.VideoConstantRateFactor, cancellationToken: cancellationToken);
            case ExportFormat.Mp4:
                return await VideoExporter.ExportAsync(frames, request.Fps, request.OutputPath, P5CCS.Export.VideoFormat.Mp4, mp4BitrateKbps: request.Mp4BitrateKbps, cancellationToken: cancellationToken);
            default:
                throw new NotSupportedException($"Export format '{request.Format}' is not supported.");
        }
    }
}
