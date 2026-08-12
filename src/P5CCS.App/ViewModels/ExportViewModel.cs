using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using P5CCS.Core.Dialogs;

namespace P5CCS.App.ViewModels;

public partial class ExportViewModel : ObservableObject
{
    private const int DefaultCanvasWidth = 800;
    private const int DefaultCanvasHeight = 450;

    private readonly SketchTabViewModel _sketch;
    private readonly IDialogService _dialogService;
    private readonly Export.ExportJobRunner _jobRunner = new();

    private CancellationTokenSource? _currentExportCts;

    public ExportViewModel(SketchTabViewModel sketch, IDialogService dialogService)
    {
        _sketch = sketch;
        _dialogService = dialogService;

        DestinationFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        UpdateFileNamePreview();
    }

    public IReadOnlyList<Export.ExportFormat> AvailableFormats { get; } = Enum.GetValues<Export.ExportFormat>();

    public ObservableCollection<ExportQueueItemViewModel> Queue { get; } = new();

    [ObservableProperty]
    private Export.ExportFormat _selectedFormat = Export.ExportFormat.Png;

    [ObservableProperty]
    private int _width = DefaultCanvasWidth;

    [ObservableProperty]
    private int _height = DefaultCanvasHeight;

    [ObservableProperty]
    private double _fps = 30;

    [ObservableProperty]
    private double _durationSeconds = 3;

    [ObservableProperty]
    private string _destinationFolder;

    [ObservableProperty]
    private int _gifColorCount = 256;

    [ObservableProperty]
    private int _videoConstantRateFactor = 30;

    [ObservableProperty]
    private int _mp4BitrateKbps = 4000;

    [ObservableProperty]
    private int _jpegQuality = 90;

    [ObservableProperty]
    private string _fileNamePreview = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartQueueCommand))]
    private bool _isExporting;

    [ObservableProperty]
    private double _overallProgressFraction;

    [ObservableProperty]
    private string _progressStatusText = string.Empty;

    public bool IsStillFormat => SelectedFormat is Export.ExportFormat.Png or Export.ExportFormat.Jpeg;

    public bool ShowGifOptions => SelectedFormat == Export.ExportFormat.Gif;

    public bool ShowVideoOptions => SelectedFormat is Export.ExportFormat.WebM or Export.ExportFormat.Mp4;

    public bool ShowMp4Options => SelectedFormat == Export.ExportFormat.Mp4;

    public bool ShowJpegOptions => SelectedFormat == Export.ExportFormat.Jpeg;

    partial void OnSelectedFormatChanged(Export.ExportFormat value)
    {
        OnPropertyChanged(nameof(IsStillFormat));
        OnPropertyChanged(nameof(ShowGifOptions));
        OnPropertyChanged(nameof(ShowVideoOptions));
        OnPropertyChanged(nameof(ShowMp4Options));
        OnPropertyChanged(nameof(ShowJpegOptions));
        UpdateFileNamePreview();
    }

    [RelayCommand]
    private void BrowseDestination()
    {
        var folder = _dialogService.ShowFolderBrowserDialog("Choose export destination folder");
        if (folder is not null)
        {
            DestinationFolder = folder;
        }
    }

    [RelayCommand]
    private void AddToQueue()
    {
        var extension = SelectedFormat switch
        {
            Export.ExportFormat.Png => "png",
            Export.ExportFormat.Jpeg => "jpg",
            Export.ExportFormat.Gif => "gif",
            Export.ExportFormat.WebM => "webm",
            Export.ExportFormat.Mp4 => "mp4",
            _ => "dat",
        };

        var fileName = Export.ExportFileNaming.GenerateFileName(_sketch.Title, extension);
        var outputPath = Path.Combine(DestinationFolder, fileName);

        var request = new Export.ExportRequest(
            SelectedFormat,
            Width,
            Height,
            Fps,
            IsStillFormat ? 0 : DurationSeconds,
            outputPath,
            GifColorCount,
            VideoConstantRateFactor,
            Mp4BitrateKbps,
            JpegQuality);

        Queue.Add(new ExportQueueItemViewModel(request));
        UpdateFileNamePreview();
        StartQueueCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void RemoveFromQueue(ExportQueueItemViewModel? item)
    {
        if (item is not null)
        {
            Queue.Remove(item);
            StartQueueCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanStartQueue() => !IsExporting && Queue.Count > 0 && _sketch.Engine is not null;

    [RelayCommand(CanExecute = nameof(CanStartQueue))]
    private async Task StartQueueAsync()
    {
        if (_sketch.Engine is null)
        {
            return;
        }

        IsExporting = true;
        _currentExportCts = new CancellationTokenSource();

        try
        {
            var pendingItems = Queue.Where(q => q.Status == ExportQueueItemStatus.Pending).ToList();
            for (var i = 0; i < pendingItems.Count; i++)
            {
                var item = pendingItems[i];
                item.Status = ExportQueueItemStatus.Running;
                item.StatusText = "Exporting...";

                var progress = new Progress<P5CCS.Export.ExportProgress>(p =>
                {
                    item.ProgressFraction = p.FractionComplete;
                    OverallProgressFraction = (i + p.FractionComplete) / pendingItems.Count;
                    ProgressStatusText = p.EstimatedTimeRemaining is { } eta
                        ? $"{item.FileName}: {p.CompletedFrames}/{p.TotalFrames} frames, ~{eta.TotalSeconds:0.#}s remaining"
                        : $"{item.FileName}: {p.CompletedFrames}/{p.TotalFrames} frames";
                });

                try
                {
                    var encoderUsed = await _jobRunner.RunAsync(
                        _sketch.Engine,
                        item.Request,
                        DefaultCanvasWidth,
                        DefaultCanvasHeight,
                        progress,
                        _currentExportCts.Token);

                    item.Status = ExportQueueItemStatus.Completed;
                    item.StatusText = encoderUsed is null ? "Completed" : $"Completed ({encoderUsed})";
                    item.ProgressFraction = 1;
                }
                catch (OperationCanceledException)
                {
                    item.Status = ExportQueueItemStatus.Cancelled;
                    item.StatusText = "Cancelled";
                    break;
                }
                catch (Exception ex)
                {
                    item.Status = ExportQueueItemStatus.Failed;
                    item.StatusText = "Failed";
                    item.ErrorMessage = ex.Message;
                }
            }
        }
        finally
        {
            IsExporting = false;
            ProgressStatusText = string.Empty;
            _currentExportCts?.Dispose();
            _currentExportCts = null;
        }
    }

    [RelayCommand]
    private void CancelExport() => _currentExportCts?.Cancel();

    private void UpdateFileNamePreview()
    {
        var extension = SelectedFormat switch
        {
            Export.ExportFormat.Png => "png",
            Export.ExportFormat.Jpeg => "jpg",
            Export.ExportFormat.Gif => "gif",
            Export.ExportFormat.WebM => "webm",
            Export.ExportFormat.Mp4 => "mp4",
            _ => "dat",
        };

        FileNamePreview = Export.ExportFileNaming.GenerateFileName(_sketch.Title, extension);
    }
}
