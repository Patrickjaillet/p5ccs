using System.Windows.Input;
using P5CCS.App.ViewModels;
using P5CCS.Core.Input;
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
        MouseMove += OnMouseMove;
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

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(this);
        _viewModel.MousePositionText = $"{position.X:0}, {position.Y:0}";
    }
}
