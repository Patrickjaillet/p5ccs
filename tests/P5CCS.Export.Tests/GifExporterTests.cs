using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace P5CCS.Export.Tests;

public class GifExporterTests
{
    [Fact]
    public async Task ExportAsync_ProducesGifWithSameFrameCountAsInput()
    {
        var frames = new[]
        {
            CreateSolidColorPng(Color.Red),
            CreateSolidColorPng(Color.Green),
            CreateSolidColorPng(Color.Blue),
        };
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-gif-{Guid.NewGuid():N}.gif");

        try
        {
            await GifExporter.ExportAsync(frames, fps: 10, outputPath);

            using var result = await Image.LoadAsync<Rgba32>(outputPath);
            Assert.Equal(3, result.Frames.Count);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportAsync_PreservesApproximateFrameColors()
    {
        var frames = new[]
        {
            CreateSolidColorPng(Color.Red),
            CreateSolidColorPng(Color.Lime),
        };
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-gif-{Guid.NewGuid():N}.gif");

        try
        {
            await GifExporter.ExportAsync(frames, fps: 10, outputPath);

            using var result = await Image.LoadAsync<Rgba32>(outputPath);
            var firstPixel = result.Frames.CloneFrame(0)[0, 0];
            var secondPixel = result.Frames.CloneFrame(1)[0, 0];

            Assert.True(firstPixel.R > 200 && firstPixel.G < 60);
            Assert.True(secondPixel.G > 200 && secondPixel.R < 60);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportAsync_SetsFrameDelayFromFps()
    {
        var frames = new[] { CreateSolidColorPng(Color.Red), CreateSolidColorPng(Color.Blue) };
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-gif-{Guid.NewGuid():N}.gif");

        try
        {
            await GifExporter.ExportAsync(frames, fps: 20, outputPath);

            using var result = await Image.LoadAsync<Rgba32>(outputPath);
            var metadata = result.Frames.RootFrame.Metadata.GetGifMetadata();

            Assert.Equal(5, metadata.FrameDelay);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportAsync_SetsInfiniteRepeatCount()
    {
        var frames = new[] { CreateSolidColorPng(Color.Red), CreateSolidColorPng(Color.Blue) };
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-gif-{Guid.NewGuid():N}.gif");

        try
        {
            await GifExporter.ExportAsync(frames, fps: 10, outputPath);

            using var result = await Image.LoadAsync<Rgba32>(outputPath);
            var metadata = result.Metadata.GetGifMetadata();

            Assert.Equal(0, metadata.RepeatCount);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportAsync_EmptyFrameList_Throws()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-gif-{Guid.NewGuid():N}.gif");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            GifExporter.ExportAsync(Array.Empty<byte[]>(), fps: 10, outputPath));
    }

    private static byte[] CreateSolidColorPng(Color color)
    {
        var pixel = color.ToPixel<Rgba32>();
        using var image = new Image<Rgba32>(8, 8);

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
