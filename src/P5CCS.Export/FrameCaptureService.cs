using System.Diagnostics;

namespace P5CCS.Export;

public sealed class FrameCaptureService
{
    public async IAsyncEnumerable<byte[]> CaptureFramesAsync(
        IExportFrameSource source,
        ExportFrameRequest request,
        IProgress<ExportProgress>? progress = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (request.FrameCount <= 0)
        {
            yield break;
        }

        var stopwatch = Stopwatch.StartNew();

        await source.ResizeCanvasForExportAsync(request.Width, request.Height);
        await source.BeginExportAsync();

        try
        {
            var millisecondsPerFrame = 1000.0 / request.Fps;

            for (var frameIndex = 0; frameIndex < request.FrameCount; frameIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var virtualMillis = frameIndex * millisecondsPerFrame;
                var frameBytes = await source.CaptureExportFrameAsync(virtualMillis);

                yield return frameBytes;

                progress?.Report(new ExportProgress(frameIndex + 1, request.FrameCount, stopwatch.Elapsed));
            }
        }
        finally
        {
            await source.EndExportAsync();
        }
    }
}
