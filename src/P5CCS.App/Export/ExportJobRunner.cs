using P5CCS.Engine;
using P5CCS.Export;

namespace P5CCS.App.Export;

public sealed class ExportJobRunner
{
    private readonly FrameCaptureService _frameCaptureService = new();

    public async Task RunAsync(
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

            await ExportFramesAsync(frames, request, cancellationToken);
        }
        finally
        {
            if (restoreWidth is int width && restoreHeight is int height)
            {
                await adapter.ResizeCanvasForExportAsync(width, height);
            }
        }
    }

    private static Task ExportFramesAsync(IReadOnlyList<byte[]> frames, ExportRequest request, CancellationToken cancellationToken)
    {
        return request.Format switch
        {
            ExportFormat.Png => StillImageExporter.ExportAsync(frames[0], request.OutputPath, StillImageFormat.Png, cancellationToken: cancellationToken),
            ExportFormat.Jpeg => StillImageExporter.ExportAsync(frames[0], request.OutputPath, StillImageFormat.Jpeg, request.JpegQuality, cancellationToken),
            ExportFormat.Gif => GifExporter.ExportAsync(frames, request.Fps, request.OutputPath, request.GifColorCount, cancellationToken),
            ExportFormat.WebM => VideoExporter.ExportAsync(frames, request.Fps, request.OutputPath, P5CCS.Export.VideoFormat.WebM, request.VideoConstantRateFactor, cancellationToken: cancellationToken),
            ExportFormat.Mp4 => VideoExporter.ExportAsync(frames, request.Fps, request.OutputPath, P5CCS.Export.VideoFormat.Mp4, mp4BitrateKbps: request.Mp4BitrateKbps, cancellationToken: cancellationToken),
            _ => throw new NotSupportedException($"Export format '{request.Format}' is not supported."),
        };
    }
}
