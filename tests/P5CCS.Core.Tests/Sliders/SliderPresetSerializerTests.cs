using P5CCS.Core.Sliders;

namespace P5CCS.Core.Tests.Sliders;

public class SliderPresetSerializerTests
{
    [Fact]
    public void ToJson_ThenFromJson_RoundTripsPreset()
    {
        var preset = new SliderPreset
        {
            Name = "Bright",
            Values = new Dictionary<string, string>
            {
                ["speed"] = "5",
                ["showGrid"] = "true",
                ["fill"] = "255,0,0",
            },
        };

        var json = SliderPresetSerializer.ToJson(preset);
        var restored = SliderPresetSerializer.FromJson(json);

        Assert.Equal(preset.Name, restored.Name);
        Assert.Equal(preset.Values, restored.Values);
    }

    [Fact]
    public void ToJson_ThenManyFromJson_RoundTripsMultiplePresets()
    {
        var presets = new List<SliderPreset>
        {
            new() { Name = "A", Values = new Dictionary<string, string> { ["x"] = "1" } },
            new() { Name = "B", Values = new Dictionary<string, string> { ["y"] = "2" } },
        };

        var json = SliderPresetSerializer.ToJson(presets);
        var restored = SliderPresetSerializer.ManyFromJson(json);

        Assert.Equal(2, restored.Count);
        Assert.Equal("A", restored[0].Name);
        Assert.Equal("B", restored[1].Name);
    }
}
