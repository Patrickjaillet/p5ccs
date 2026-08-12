using System.Windows.Controls;
using P5CCS.App.ViewModels;

namespace P5CCS.App.Views;

public partial class ApiReferencePanel : UserControl
{
    public ApiReferencePanel()
    {
        InitializeComponent();
        DataContext = new ApiReferenceViewModel();
    }
}
