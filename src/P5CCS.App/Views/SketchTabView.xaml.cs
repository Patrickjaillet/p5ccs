using System.Windows;
using System.Windows.Controls;
using P5CCS.App.ViewModels;
using P5CCS.Editor.ErrorMarkers;
using Wpf.Ui.Appearance;

namespace P5CCS.App.Views;

public partial class SketchTabView : UserControl
{
    private SketchTabViewModel? _tab;

    public SketchTabView()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ShowGridProperty = DependencyProperty.Register(
        nameof(ShowGrid), typeof(bool), typeof(SketchTabView),
        new PropertyMetadata(false, OnShowGridChanged));

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    private static void OnShowGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SketchTabView)d).Viewport.ShowGrid = (bool)e.NewValue;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SketchTabViewModel tab)
        {
            return;
        }

        _tab = tab;

        CodeEditor.Text = tab.Source;
        CodeEditor.TextChangedDebounced += OnEditorTextChangedDebounced;
        tab.ErrorMarkersChanged += OnErrorMarkersChanged;
        ApplicationThemeManager.Changed += OnApplicationThemeChanged;

        ApplyEditorTheme();
        tab.Engine = Viewport;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        CodeEditor.TextChangedDebounced -= OnEditorTextChangedDebounced;
        ApplicationThemeManager.Changed -= OnApplicationThemeChanged;

        if (_tab is not null)
        {
            _tab.ErrorMarkersChanged -= OnErrorMarkersChanged;
        }
    }

    private void OnEditorTextChangedDebounced(object? sender, string newSource)
    {
        _tab?.UpdateSourceFromEditor(newSource);
    }

    private void OnErrorMarkersChanged(object? sender, IReadOnlyList<EditorErrorMarker> markers)
    {
        CodeEditor.SetErrors(markers);
    }

    private void OnApplicationThemeChanged(ApplicationTheme currentApplicationTheme, System.Windows.Media.Color systemAccent)
    {
        ApplyEditorTheme();
    }

    private void ApplyEditorTheme()
    {
        var isDark = ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Dark;
        CodeEditor.ApplyTheme(isDark);
    }
}
