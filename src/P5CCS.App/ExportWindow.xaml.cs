using P5CCS.App.ViewModels;
using Wpf.Ui.Controls;

namespace P5CCS.App;

public partial class ExportWindow : FluentWindow
{
    public ExportWindow(ExportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
