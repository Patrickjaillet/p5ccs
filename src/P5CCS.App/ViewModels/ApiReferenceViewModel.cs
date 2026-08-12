using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using P5CCS.Editor.Completion;

namespace P5CCS.App.ViewModels;

public partial class ApiReferenceViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    public ApiReferenceViewModel()
    {
        RebuildCategories();
    }

    public ObservableCollection<ApiCategoryViewModel> Categories { get; } = new();

    partial void OnSearchTextChanged(string value) => RebuildCategories();

    private void RebuildCategories()
    {
        Categories.Clear();

        var query = string.IsNullOrWhiteSpace(SearchText)
            ? P5ApiCatalog.Entries
            : P5ApiCatalog.Entries.Where(e =>
                e.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        var groups = query
            .GroupBy(e => e.Category)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var group in groups)
        {
            Categories.Add(new ApiCategoryViewModel(group.Key, group));
        }
    }
}
