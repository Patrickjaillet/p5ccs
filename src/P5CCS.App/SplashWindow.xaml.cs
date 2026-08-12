using System.Windows;

namespace P5CCS.App;

public partial class SplashWindow : Window
{
    public SplashWindow(string versionText)
    {
        InitializeComponent();
        VersionTextBlock.Text = versionText;
    }
}
