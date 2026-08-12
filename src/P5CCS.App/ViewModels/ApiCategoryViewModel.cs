using P5CCS.Editor.Completion;

namespace P5CCS.App.ViewModels;

public sealed class ApiCategoryViewModel
{
    public ApiCategoryViewModel(string name, IEnumerable<P5ApiEntry> entries)
    {
        Name = name;
        Entries = entries.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public string Name { get; }

    public IReadOnlyList<P5ApiEntry> Entries { get; }
}
