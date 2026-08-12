namespace P5CCS.Export;

public interface IExportFrameSource
{
    Task BeginExportAsync();

    Task<byte[]> CaptureExportFrameAsync(double virtualMillis);

    Task EndExportAsync();

    Task ResizeCanvasForExportAsync(int width, int height);
}
