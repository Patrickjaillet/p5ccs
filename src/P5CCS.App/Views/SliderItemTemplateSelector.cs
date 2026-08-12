using System.Windows;
using System.Windows.Controls;
using P5CCS.App.ViewModels;
using P5CCS.Core.Sliders;

namespace P5CCS.App.Views;

public sealed class SliderItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? NumberTemplate { get; set; }

    public DataTemplate? BooleanTemplate { get; set; }

    public DataTemplate? ColorTemplate { get; set; }

    public DataTemplate? EnumTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is not SliderItemViewModel slider)
        {
            return base.SelectTemplate(item, container);
        }

        return slider.Kind switch
        {
            SliderControlKind.Number => NumberTemplate,
            SliderControlKind.Boolean => BooleanTemplate,
            SliderControlKind.Color => ColorTemplate,
            SliderControlKind.Enum => EnumTemplate,
            _ => base.SelectTemplate(item, container),
        };
    }
}
