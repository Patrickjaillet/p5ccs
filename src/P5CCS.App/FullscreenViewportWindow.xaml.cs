using System.Windows;
using System.Windows.Input;

namespace P5CCS.App;

public partial class FullscreenViewportWindow : Window
{
    private readonly string _source;

    public FullscreenViewportWindow(string source)
    {
        InitializeComponent();
        _source = source;
        Loaded += OnLoaded;
        Closed += (_, _) => Viewport.Dispose();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Viewport.LoadSketch(_source);
        Viewport.Run();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
