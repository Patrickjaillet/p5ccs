using System.Windows;
using P5CCS.App.ViewModels;

namespace P5CCS.App;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
