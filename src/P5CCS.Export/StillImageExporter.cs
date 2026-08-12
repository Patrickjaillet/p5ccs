using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace P5CCS.Export;

public enum StillImageFormat
{
    Png,
    Jpeg,
}

public static class StillImageExporter
{
    public static async Task ExportAsync(
        byte[] framePngBytes,
        string outputPath,
        StillImageFormat format,
        int jpegQuality = 90,
        CancellationToken cancellationToken = default)
    {
        if (framePngBytes.Length == 0)
        {
            throw new ArgumentException("Frame data must not be empty.", nameof(framePngBytes));
        }

        if (format == StillImageFormat.Png)
        {
            await File.WriteAllBytesAsync(outputPath, framePngBytes, cancellationToken);
            return;
        }

        using var image = Image.Load<Rgba32>(framePngBytes);
        var encoder = new JpegEncoder { Quality = Math.Clamp(jpegQuality, 1, 100) };
        await using var outputStream = File.Create(outputPath);
        await image.SaveAsJpegAsync(outputStream, encoder, cancellationToken);
    }
}
