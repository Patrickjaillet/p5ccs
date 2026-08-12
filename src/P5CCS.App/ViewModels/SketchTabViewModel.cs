using CommunityToolkit.Mvvm.ComponentModel;

namespace P5CCS.App.ViewModels;

public partial class SketchTabViewModel : ObservableObject
{
    public SketchTabViewModel(string title, string? filePath)
    {
        _title = title;
        FilePath = filePath;
    }

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private bool _isModified;

    public string? FilePath { get; set; }
}
