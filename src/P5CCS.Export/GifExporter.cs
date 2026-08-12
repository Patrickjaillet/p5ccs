using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace P5CCS.Export;

public static class GifExporter
{
    public static async Task ExportAsync(
        IReadOnlyList<byte[]> framePngBytes,
        double fps,
        string outputPath,
        int colorCount = 256,
        CancellationToken cancellationToken = default)
    {
        if (framePngBytes.Count == 0)
        {
            throw new ArgumentException("At least one frame is required to export a GIF.", nameof(framePngBytes));
        }

        var frameDelayInHundredthsOfSecond = (int)Math.Round(100.0 / fps);
        var quantizer = new OctreeQuantizer(new QuantizerOptions { MaxColors = Math.Clamp(colorCount, 2, 256) });

        using var firstFrame = Image.Load<Rgba32>(framePngBytes[0]);
        firstFrame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = frameDelayInHundredthsOfSecond;

        for (var i = 1; i < framePngBytes.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var nextFrame = Image.Load<Rgba32>(framePngBytes[i]);
            var addedFrame = firstFrame.Frames.AddFrame(nextFrame.Frames.RootFrame);
            addedFrame.Metadata.GetGifMetadata().FrameDelay = frameDelayInHundredthsOfSecond;
        }

        var gifMetadata = firstFrame.Metadata.GetGifMetadata();
        gifMetadata.RepeatCount = 0;

        var encoder = new GifEncoder
        {
            Quantizer = quantizer,
            ColorTableMode = GifColorTableMode.Global,
        };

        await using var outputStream = File.Create(outputPath);
        await firstFrame.SaveAsGifAsync(outputStream, encoder, cancellationToken);
    }
}
