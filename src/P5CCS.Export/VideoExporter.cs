using FFMpegCore;
using FFMpegCore.Exceptions;

namespace P5CCS.Export;

public static class VideoExporter
{
    // Hardware encoders are tried first if the ffmpeg build supports them, but "supports"
    // only means it was compiled in — the actual driver/API stack (e.g. NVENC's
    // nvEncodeAPI64.dll) may still be missing at runtime, especially in virtualized or
    // GPU-passthrough environments. Each candidate is attempted and falls through to the
    // next on failure, ending in a CPU software encoder that is always expected to work.
    private static readonly string[] Mp4GpuEncoderCandidates = { "h264_nvenc", "h264_amf", "h264_qsv" };
    private const string Mp4CpuFallbackEncoder = "libopenh264";

    private static readonly string[] WebMGpuEncoderCandidates = { "vp9_qsv" };
    private const string WebMCpuFallbackEncoder = "libvpx-vp9";

    public static async Task<string> ExportAsync(
        IReadOnlyList<byte[]> framePngBytes,
        double fps,
        string outputPath,
        VideoFormat format,
        int constantRateFactor = 30,
        int mp4BitrateKbps = 4000,
        bool preferGpuEncoding = true,
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
            var gpuCandidates = format == VideoFormat.WebM ? WebMGpuEncoderCandidates : Mp4GpuEncoderCandidates;
            var cpuFallback = format == VideoFormat.WebM ? WebMCpuFallbackEncoder : Mp4CpuFallbackEncoder;

            var candidates = preferGpuEncoding
                ? gpuCandidates.Append(cpuFallback).ToArray()
                : new[] { cpuFallback };

            for (var i = 0; i < candidates.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var encoder = candidates[i];
                var isLastCandidate = i == candidates.Length - 1;

                try
                {
                    var success = await FFMpegArguments
                        .FromFileInput(inputPattern, verifyExists: false, options => options
                            .WithFramerate(fps))
                        .OutputToFile(outputPath, overwrite: true, options =>
                            ConfigureEncoder(options, encoder, cpuFallback, fps, constantRateFactor, mp4BitrateKbps))
                        .CancellableThrough(cancellationToken)
                        .ProcessAsynchronously();

                    if (!success)
                    {
                        throw new InvalidOperationException($"ffmpeg exited with a non-zero status code using encoder '{encoder}'.");
                    }

                    return encoder;
                }
                catch (Exception ex) when (!isLastCandidate && ex is FFMpegException or InvalidOperationException)
                {
                    // This candidate encoder isn't actually usable on this machine
                    // (missing driver/API, unsupported hardware, etc.) — try the next one.
                }
            }

            throw new InvalidOperationException("No usable video encoder was found.");
        }
        finally
        {
            tempDirectory.Delete(recursive: true);
        }
    }

    private static void ConfigureEncoder(
        FFMpegArgumentOptions options,
        string encoder,
        string cpuFallback,
        double fps,
        int constantRateFactor,
        int mp4BitrateKbps)
    {
        options.ForcePixelFormat("yuv420p").WithFramerate(fps).WithVideoCodec(encoder);

        if (encoder == cpuFallback && encoder == WebMCpuFallbackEncoder)
        {
            // libvpx-vp9 (BSD-licensed, available in LGPL ffmpeg builds) supports CRF-based
            // constant-quality encoding, unlike the GPU/bitrate-only candidates above it.
            options.WithConstantRateFactor(constantRateFactor);
        }
        else
        {
            options.WithVideoBitrate(mp4BitrateKbps);
        }
    }
}
