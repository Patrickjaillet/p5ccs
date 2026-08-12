namespace P5CCS.Core.Services;

public interface IExportService
{
    Task ExportAsync(ExportSettings settings, IProgress<double>? progress, CancellationToken cancellationToken);
}
