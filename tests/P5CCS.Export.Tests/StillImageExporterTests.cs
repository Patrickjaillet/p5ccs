using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace P5CCS.Export.Tests;

public class StillImageExporterTests
{
    [Fact]
    public async Task ExportAsync_Png_WritesExactBytesUnmodified()
    {
        var pngBytes = CreateSolidColorPng(Color.Red);
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-still-{Guid.NewGuid():N}.png");

        try
        {
            await StillImageExporter.ExportAsync(pngBytes, outputPath, StillImageFormat.Png);

            var written = await File.ReadAllBytesAsync(outputPath);
            Assert.Equal(pngBytes, written);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportAsync_Jpeg_ProducesLoadableImageWithSameDimensions()
    {
        var pngBytes = CreateSolidColorPng(Color.Blue, 32, 24);
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-still-{Guid.NewGuid():N}.jpg");

        try
        {
            await StillImageExporter.ExportAsync(pngBytes, outputPath, StillImageFormat.Jpeg, jpegQuality: 85);

            using var result = await Image.LoadAsync<Rgba32>(outputPath);
            Assert.Equal(32, result.Width);
            Assert.Equal(24, result.Height);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportAsync_EmptyBytes_Throws()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-still-{Guid.NewGuid():N}.png");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            StillImageExporter.ExportAsync(Array.Empty<byte>(), outputPath, StillImageFormat.Png));
    }

    private static byte[] CreateSolidColorPng(Color color, int width = 8, int height = 8)
    {
        var pixel = color.ToPixel<Rgba32>();
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
