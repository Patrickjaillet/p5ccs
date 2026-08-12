namespace P5CCS.Export;

public sealed record ExportFrameRequest(int Width, int Height, double Fps, int FrameCount)
{
    public double DurationSeconds => FrameCount / Fps;
}
