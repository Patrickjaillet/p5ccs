using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using P5CCS.App.Export;

namespace P5CCS.App.ViewModels;

public enum ExportQueueItemStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    Cancelled,
}

public partial class ExportQueueItemViewModel : ObservableObject
{
    public ExportQueueItemViewModel(ExportRequest request)
    {
        Request = request;
    }

    public ExportRequest Request { get; }

    public string FileName => Path.GetFileName(Request.OutputPath);

    [ObservableProperty]
    private ExportQueueItemStatus _status = ExportQueueItemStatus.Pending;

    [ObservableProperty]
    private double _progressFraction;

    [ObservableProperty]
    private string _statusText = "Pending";

    [ObservableProperty]
    private string? _errorMessage;
}
