namespace P5CCS.App.Export;

public sealed record ExportRequest(
    ExportFormat Format,
    int Width,
    int Height,
    double Fps,
    double DurationSeconds,
    string OutputPath,
    int GifColorCount = 256,
    int VideoConstantRateFactor = 30,
    int Mp4BitrateKbps = 4000,
    int JpegQuality = 90)
{
    public bool IsStillImage => Format is ExportFormat.Png or ExportFormat.Jpeg;

    public int FrameCount => IsStillImage ? 1 : Math.Max(1, (int)Math.Round(DurationSeconds * Fps));
}
