namespace P5CCS.Core.Sliders;

public sealed class SliderPreset
{
    public required string Name { get; init; }

    public DateTimeOffset CreatedUtc { get; init; } = DateTimeOffset.UtcNow;

    public Dictionary<string, string> Values { get; init; } = new();
}
