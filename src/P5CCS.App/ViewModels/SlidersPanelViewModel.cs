using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using P5CCS.Core.Dialogs;
using P5CCS.Core.Sliders;

namespace P5CCS.App.ViewModels;

public partial class SlidersPanelViewModel : ObservableObject, IDisposable
{
    private readonly SketchTabViewModel _tab;
    private readonly IDialogService _dialogService;
    private readonly DispatcherTimer _rewriteDebounceTimer;
    private readonly DispatcherTimer _animationTimer;
    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly HashSet<SliderItemViewModel> _pendingEdits = new();

    private bool _isRewritingSource;

    public SlidersPanelViewModel(SketchTabViewModel tab, IDialogService dialogService)
    {
        _tab = tab;
        _dialogService = dialogService;

        _rewriteDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
        _rewriteDebounceTimer.Tick += (_, _) =>
        {
            _rewriteDebounceTimer.Stop();
            FlushPendingEdits();
        };

        _animationTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(33) };
        _animationTimer.Tick += (_, _) => TickAnimations();
        _animationTimer.Start();

        _tab.SourceChanged += (_, _) =>
        {
            if (!_isRewritingSource)
            {
                Reanalyze(_tab.Source);
            }
        };

        Reanalyze(_tab.Source);
    }

    public ObservableCollection<SliderGroupViewModel> Groups { get; } = new();

    public ObservableCollection<SliderPreset> Presets { get; } = new();

    [ObservableProperty]
    private string _newPresetName = string.Empty;

    [ObservableProperty]
    private bool _hasSliders;

    public bool HasNoSliders => !HasSliders;

    partial void OnHasSlidersChanged(bool value) => OnPropertyChanged(nameof(HasNoSliders));

    [RelayCommand]
    private void SavePreset()
    {
        var name = string.IsNullOrWhiteSpace(NewPresetName) ? $"Preset {Presets.Count + 1}" : NewPresetName.Trim();
        var values = new Dictionary<string, string>();

        foreach (var group in Groups)
        {
            foreach (var item in group.Items)
            {
                values[item.Name] = item.FormatSourceValue();
            }
        }

        var existing = Presets.FirstOrDefault(p => p.Name == name);
        if (existing is not null)
        {
            Presets.Remove(existing);
        }

        Presets.Add(new SliderPreset { Name = name, Values = values });
        NewPresetName = string.Empty;
    }

    [RelayCommand]
    private void DeletePreset(SliderPreset preset) => Presets.Remove(preset);

    [RelayCommand]
    private void ApplyPreset(SliderPreset preset)
    {
        foreach (var group in Groups)
        {
            foreach (var item in group.Items)
            {
                if (!preset.Values.TryGetValue(item.Name, out var rawValue))
                {
                    continue;
                }

                ApplyRawValue(item, rawValue);
                _pendingEdits.Add(item);
            }
        }

        FlushPendingEdits();
    }

    [RelayCommand]
    private void ExportPresets()
    {
        var path = _dialogService.ShowSaveFileDialog("JSON (*.json)|*.json", "Export Slider Presets", "slider-presets.json");
        if (path is null)
        {
            return;
        }

        var json = SliderPresetSerializer.ToJson(Presets.ToList());
        File.WriteAllText(path, json);
    }

    [RelayCommand]
    private void ImportPresets()
    {
        var path = _dialogService.ShowOpenFileDialog("JSON (*.json)|*.json", "Import Slider Presets");
        if (path is null || !File.Exists(path))
        {
            return;
        }

        var imported = SliderPresetSerializer.ManyFromJson(File.ReadAllText(path));
        foreach (var preset in imported)
        {
            var existing = Presets.FirstOrDefault(p => p.Name == preset.Name);
            if (existing is not null)
            {
                Presets.Remove(existing);
            }

            Presets.Add(preset);
        }
    }

    public void Dispose()
    {
        _rewriteDebounceTimer.Stop();
        _animationTimer.Stop();
    }

    private void Reanalyze(string source)
    {
        var candidates = SketchSourceAnalyzer.Analyze(source);
        var existingItems = Groups.SelectMany(g => g.Items).ToDictionary(i => (i.Name, i.Kind));

        var groupedByName = candidates
            .GroupBy(c => c.GroupName)
            .ToList();

        Groups.Clear();

        foreach (var group in groupedByName)
        {
            var groupViewModel = new SliderGroupViewModel(group.Key);

            foreach (var candidate in group)
            {
                if (existingItems.TryGetValue((candidate.Name, candidate.Kind), out var existingItem))
                {
                    existingItem.ApplyCandidate(candidate, resetBounds: false);
                    groupViewModel.Items.Add(existingItem);
                }
                else
                {
                    var item = new SliderItemViewModel(candidate);
                    item.ValueEditedByUser += OnSliderValueEdited;
                    groupViewModel.Items.Add(item);
                }
            }

            Groups.Add(groupViewModel);
        }

        HasSliders = Groups.Any(g => g.Items.Count > 0);
    }

    private void OnSliderValueEdited(object? sender, EventArgs e)
    {
        if (sender is not SliderItemViewModel item)
        {
            return;
        }

        _pendingEdits.Add(item);
        _rewriteDebounceTimer.Stop();
        _rewriteDebounceTimer.Start();
    }

    private void FlushPendingEdits()
    {
        if (_pendingEdits.Count == 0)
        {
            return;
        }

        var replacements = _pendingEdits
            .Select(item => (item.Candidate.Offset, item.Candidate.Length, Text: item.FormatSourceValue()))
            .OrderByDescending(r => r.Offset)
            .ToList();

        _pendingEdits.Clear();

        var newSource = _tab.Source;
        foreach (var (offset, length, text) in replacements)
        {
            if (offset < 0 || offset + length > newSource.Length)
            {
                continue;
            }

            newSource = newSource.Remove(offset, length).Insert(offset, text);
        }

        _isRewritingSource = true;
        _tab.UpdateSourceFromEditor(newSource);
        _isRewritingSource = false;

        Reanalyze(newSource);
    }

    private void TickAnimations()
    {
        var elapsed = _clock.Elapsed.TotalSeconds;

        foreach (var group in Groups)
        {
            foreach (var item in group.Items)
            {
                if (item.AnimationMode == SliderAnimationMode.None || item.Kind != Core.Sliders.SliderControlKind.Number)
                {
                    continue;
                }

                var value = SliderAnimator.Evaluate(item.AnimationMode, item.Min, item.Max, elapsed, item.AnimationPeriodSeconds);
                item.SetAnimatedValue(value);
                _tab.Engine?.SetGlobalNumber(item.Name, value);
            }
        }
    }

    private static void ApplyRawValue(SliderItemViewModel item, string rawValue)
    {
        switch (item.Kind)
        {
            case Core.Sliders.SliderControlKind.Number:
                if (double.TryParse(rawValue, System.Globalization.CultureInfo.InvariantCulture, out var number))
                {
                    item.NumberValue = number;
                }

                break;
            case Core.Sliders.SliderControlKind.Boolean:
                item.BooleanValue = rawValue == "true";
                break;
            case Core.Sliders.SliderControlKind.Color:
                var parts = rawValue.Split(',', StringSplitOptions.TrimEntries);
                if (parts.Length == 3 && byte.TryParse(parts[0], out var r) && byte.TryParse(parts[1], out var g) && byte.TryParse(parts[2], out var b))
                {
                    item.ColorR = r;
                    item.ColorG = g;
                    item.ColorB = b;
                }

                break;
            case Core.Sliders.SliderControlKind.Enum:
                item.EnumValue = rawValue;
                break;
        }
    }
}
