using FFMpegCore;

namespace P5CCS.Export;

public static class FFmpegLocator
{
    private static bool _configured;

    public static string BinaryFolder => Path.Combine(AppContext.BaseDirectory, "tools", "ffmpeg");

    public static string ExecutablePath => Path.Combine(BinaryFolder, "ffmpeg.exe");

    public static bool IsAvailable => File.Exists(ExecutablePath);

    public static void EnsureConfigured()
    {
        if (_configured)
        {
            return;
        }

        GlobalFFOptions.Configure(new FFOptions
        {
            BinaryFolder = BinaryFolder,
            WorkingDirectory = Path.GetTempPath(),
        });

        _configured = true;
    }
}
