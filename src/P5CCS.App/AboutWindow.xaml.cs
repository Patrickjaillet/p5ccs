using System.Diagnostics;
using System.Windows.Navigation;
using P5CCS.App.ViewModels;
using Wpf.Ui.Controls;

namespace P5CCS.App;

public partial class AboutWindow : FluentWindow
{
    public AboutWindow(AboutViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void OnMailtoRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        OpenUri(e.Uri);
        e.Handled = true;
    }

    private void OnWebsiteRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        OpenUri(e.Uri);
        e.Handled = true;
    }

    private static void OpenUri(Uri uri)
    {
        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
    }
}
