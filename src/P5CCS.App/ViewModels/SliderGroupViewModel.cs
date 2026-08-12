using System.Collections.ObjectModel;

namespace P5CCS.App.ViewModels;

public sealed class SliderGroupViewModel
{
    public SliderGroupViewModel(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public ObservableCollection<SliderItemViewModel> Items { get; } = new();
}
