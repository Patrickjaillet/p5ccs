using FFMpegCore;
using FFMpegCore.Enums;

namespace P5CCS.Export;

public static class VideoExporter
{
    public static async Task ExportAsync(
        IReadOnlyList<byte[]> framePngBytes,
        double fps,
        string outputPath,
        VideoFormat format,
        int constantRateFactor = 30,
        int mp4BitrateKbps = 4000,
        CancellationToken cancellationToken = default)
    {
        if (framePngBytes.Count == 0)
        {
            throw new ArgumentException("At least one frame is required to export a video.", nameof(framePngBytes));
        }

        if (!FFmpegLocator.IsAvailable)
        {
            throw new FileNotFoundException(
                $"ffmpeg.exe was not found at '{FFmpegLocator.ExecutablePath}'.", FFmpegLocator.ExecutablePath);
        }

        FFmpegLocator.EnsureConfigured();

        var tempDirectory = Directory.CreateTempSubdirectory("p5ccs-video-frames-");
        try
        {
            for (var i = 0; i < framePngBytes.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var framePath = Path.Combine(tempDirectory.FullName, $"frame_{i:D6}.png");
                await File.WriteAllBytesAsync(framePath, framePngBytes[i], cancellationToken);
            }

            var inputPattern = Path.Combine(tempDirectory.FullName, "frame_%06d.png");

            var success = await FFMpegArguments
                .FromFileInput(inputPattern, verifyExists: false, options => options
                    .WithFramerate(fps))
                .OutputToFile(outputPath, overwrite: true, options =>
                {
                    options.ForcePixelFormat("yuv420p").WithFramerate(fps);

                    if (format == VideoFormat.WebM)
                    {
                        // libvpx-vp9 is BSD-licensed and available in LGPL ffmpeg builds; unlike
                        // libx264 (GPL-only), it supports CRF-based constant-quality encoding.
                        options.WithVideoCodec("libvpx-vp9").WithConstantRateFactor(constantRateFactor);
                    }
                    else
                    {
                        // libx264 is GPL-only and absent from LGPL ffmpeg builds. libopenh264
                        // (Cisco, BSD-licensed) is the LGPL-compatible H.264 encoder, but it only
                        // supports bitrate-based rate control, not CRF.
                        options.WithVideoCodec("libopenh264").WithVideoBitrate(mp4BitrateKbps);
                    }
                })
                .CancellableThrough(cancellationToken)
                .ProcessAsynchronously();

            if (!success)
            {
                throw new InvalidOperationException("ffmpeg exited with a non-zero status code.");
            }
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }
}
