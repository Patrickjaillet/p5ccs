namespace P5CCS.Core.Services;

public sealed class ExportSettings
{
    public ExportFormat Format { get; set; } = ExportFormat.Png;

    public string DestinationPath { get; set; } = string.Empty;

    public int Width { get; set; } = 800;

    public int Height { get; set; } = 450;

    public int DurationFrames { get; set; }

    public int FrameRate { get; set; } = 60;
}
