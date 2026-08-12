using System.IO;
using System.Text.Json;

namespace P5CCS.Core.Sliders;

public static class SliderPresetSerializer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string ToJson(SliderPreset preset) => JsonSerializer.Serialize(preset, Options);

    public static SliderPreset FromJson(string json) =>
        JsonSerializer.Deserialize<SliderPreset>(json, Options)
        ?? throw new InvalidDataException("Unable to parse slider preset JSON.");

    public static string ToJson(IReadOnlyList<SliderPreset> presets) => JsonSerializer.Serialize(presets, Options);

    public static IReadOnlyList<SliderPreset> ManyFromJson(string json) =>
        JsonSerializer.Deserialize<List<SliderPreset>>(json, Options)
        ?? throw new InvalidDataException("Unable to parse slider presets JSON.");
}
