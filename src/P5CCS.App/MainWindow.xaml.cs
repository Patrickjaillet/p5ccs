using System.Windows;
using System.Windows.Input;
using P5CCS.App.ViewModels;
using P5CCS.Core.Input;
using P5CCS.Engine;
using Wpf.Ui.Controls;

namespace P5CCS.App;

public partial class MainWindow : FluentWindow
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel, IKeyBindingsService keyBindingsService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        ApplyKeyBindings(keyBindingsService);
    }

    private void ApplyKeyBindings(IKeyBindingsService keyBindingsService)
    {
        var converter = new KeyGestureConverter();

        foreach (var binding in keyBindingsService.Bindings)
        {
            if (!_viewModel.CommandsByName.TryGetValue(binding.CommandName, out var command))
            {
                continue;
            }

            if (converter.ConvertFromString(binding.Gesture) is not KeyGesture gesture)
            {
                continue;
            }

            InputBindings.Add(new KeyBinding(command, gesture));
        }
    }

    private void OnSketchViewportLoaded(object sender, RoutedEventArgs e)
    {
        var viewport = (SketchViewport)sender;
        if (viewport.DataContext is SketchTabViewModel tab)
        {
            tab.Engine = viewport;
        }
    }
}
