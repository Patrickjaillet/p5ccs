using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace P5CCS.Export.Tests;

public class VideoExporterTests
{
    [Fact]
    public async Task ExportAsync_WebM_ProducesFileWithValidEbmlHeader()
    {
        var frames = CreateAnimatedFrames(6);
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-video-{Guid.NewGuid():N}.webm");

        try
        {
            await VideoExporter.ExportAsync(frames, fps: 10, outputPath, VideoFormat.WebM);

            Assert.True(File.Exists(outputPath));
            var header = await ReadHeaderBytesAsync(outputPath, 4);
            Assert.Equal(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }, header);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportAsync_Mp4_ProducesFileWithFtypBox()
    {
        var frames = CreateAnimatedFrames(6);
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-video-{Guid.NewGuid():N}.mp4");

        try
        {
            await VideoExporter.ExportAsync(frames, fps: 10, outputPath, VideoFormat.Mp4);

            Assert.True(File.Exists(outputPath));
            var header = await ReadHeaderBytesAsync(outputPath, 12);
            var ftypAscii = System.Text.Encoding.ASCII.GetString(header, 4, 4);
            Assert.Equal("ftyp", ftypAscii);
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportAsync_ProducesNonTrivialFileSize()
    {
        var frames = CreateAnimatedFrames(10);
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-video-{Guid.NewGuid():N}.mp4");

        try
        {
            await VideoExporter.ExportAsync(frames, fps: 10, outputPath, VideoFormat.Mp4);

            var info = new FileInfo(outputPath);
            Assert.True(info.Length > 500, $"Expected a non-trivial encoded file, got {info.Length} bytes.");
        }
        finally
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportAsync_EmptyFrameList_Throws()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-video-{Guid.NewGuid():N}.mp4");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            VideoExporter.ExportAsync(Array.Empty<byte[]>(), fps: 10, outputPath, VideoFormat.Mp4));
    }

    [Fact]
    public async Task ExportAsync_Cancellation_ThrowsOperationCanceled()
    {
        var frames = CreateAnimatedFrames(30);
        var outputPath = Path.Combine(Path.GetTempPath(), $"p5ccs-video-{Guid.NewGuid():N}.mp4");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                VideoExporter.ExportAsync(frames, fps: 10, outputPath, VideoFormat.Mp4, cancellationToken: cts.Token));
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    private static byte[][] CreateAnimatedFrames(int count)
    {
        var frames = new byte[count][];
        for (var i = 0; i < count; i++)
        {
            var shade = (byte)(i * 255 / Math.Max(1, count - 1));
            frames[i] = CreateSolidColorPng(new Rgba32(shade, 0, (byte)(255 - shade), 255));
        }

        return frames;
    }

    private static byte[] CreateSolidColorPng(Rgba32 pixel)
    {
        using var image = new Image<Rgba32>(64, 64);

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

    private static async Task<byte[]> ReadHeaderBytesAsync(string path, int count)
    {
        await using var stream = File.OpenRead(path);
        var buffer = new byte[count];
        var read = 0;
        while (read < count)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(read, count - read));
            if (n == 0)
            {
                break;
            }

            read += n;
        }

        return buffer;
    }
}
