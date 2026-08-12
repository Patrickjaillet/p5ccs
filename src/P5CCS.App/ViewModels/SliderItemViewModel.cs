using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using P5CCS.Core.Sliders;

namespace P5CCS.App.ViewModels;

public partial class SliderItemViewModel : ObservableObject
{
    private bool _suppressValueChanged;

    public SliderItemViewModel(SliderCandidate candidate)
    {
        Candidate = candidate;
        Name = candidate.Name;
        Kind = candidate.Kind;
        GroupName = candidate.GroupName;
        EnumOptions = candidate.EnumOptions;

        ApplyCandidate(candidate, resetBounds: true);
    }

    public SliderCandidate Candidate { get; private set; }

    public string Name { get; }

    public SliderControlKind Kind { get; }

    public string GroupName { get; }

    public IReadOnlyList<string> EnumOptions { get; }

    [ObservableProperty]
    private double _numberValue;

    [ObservableProperty]
    private bool _booleanValue;

    [ObservableProperty]
    private byte _colorR;

    [ObservableProperty]
    private byte _colorG;

    [ObservableProperty]
    private byte _colorB;

    [ObservableProperty]
    private string? _enumValue;

    [ObservableProperty]
    private double _min;

    [ObservableProperty]
    private double _max;

    [ObservableProperty]
    private double _step;

    [ObservableProperty]
    private double _editableMin;

    [ObservableProperty]
    private double _editableMax;

    [ObservableProperty]
    private SliderAnimationMode _animationMode = SliderAnimationMode.None;

    [ObservableProperty]
    private double _animationPeriodSeconds = 2;

    public Color PreviewColor => Color.FromRgb(ColorR, ColorG, ColorB);

    public static IReadOnlyList<SliderAnimationMode> AnimationModeOptions { get; } = Enum.GetValues<SliderAnimationMode>();

    public event EventHandler? ValueEditedByUser;

    public void ApplyCandidate(SliderCandidate candidate, bool resetBounds)
    {
        Candidate = candidate;
        _suppressValueChanged = true;

        NumberValue = candidate.NumberValue;
        BooleanValue = candidate.BooleanValue;
        ColorR = candidate.ColorR;
        ColorG = candidate.ColorG;
        ColorB = candidate.ColorB;
        EnumValue = candidate.EnumValue;

        if (resetBounds || AnimationMode == SliderAnimationMode.None)
        {
            Min = candidate.Min;
            Max = candidate.Max;
            Step = candidate.Step;
            EditableMin = Min;
            EditableMax = Max;
        }

        _suppressValueChanged = false;
    }

    public void SetAnimatedValue(double value)
    {
        _suppressValueChanged = true;
        NumberValue = value;
        _suppressValueChanged = false;
    }

    public string FormatSourceValue() => Kind switch
    {
        SliderControlKind.Number => FormatNumber(NumberValue),
        SliderControlKind.Boolean => BooleanValue ? "true" : "false",
        SliderControlKind.Color => $"{ColorR}, {ColorG}, {ColorB}",
        SliderControlKind.Enum => EnumOptions.ToList().IndexOf(EnumValue ?? string.Empty).ToString(),
        _ => string.Empty,
    };

    [RelayCommand]
    private void ApplyBounds()
    {
        Min = EditableMin;
        Max = EditableMax;
        NumberValue = Math.Clamp(NumberValue, Min, Max);
    }

    partial void OnNumberValueChanged(double value) => RaiseValueEdited();

    partial void OnBooleanValueChanged(bool value) => RaiseValueEdited();

    partial void OnColorRChanged(byte value)
    {
        OnPropertyChanged(nameof(PreviewColor));
        RaiseValueEdited();
    }

    partial void OnColorGChanged(byte value)
    {
        OnPropertyChanged(nameof(PreviewColor));
        RaiseValueEdited();
    }

    partial void OnColorBChanged(byte value)
    {
        OnPropertyChanged(nameof(PreviewColor));
        RaiseValueEdited();
    }

    partial void OnEnumValueChanged(string? value) => RaiseValueEdited();

    private void RaiseValueEdited()
    {
        if (!_suppressValueChanged)
        {
            ValueEditedByUser?.Invoke(this, EventArgs.Empty);
        }
    }

    private static string FormatNumber(double value) =>
        value == Math.Floor(value) ? ((long)value).ToString() : value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}
