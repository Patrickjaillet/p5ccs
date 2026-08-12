namespace P5CCS.Core.Sliders;

public sealed class SliderCandidate
{
    public required string Name { get; init; }

    public required SliderControlKind Kind { get; init; }

    public required string GroupName { get; init; }

    public required int Offset { get; init; }

    public required int Length { get; init; }

    public required int LineNumber { get; init; }

    public bool IsBoundsAnnotated { get; init; }

    public double NumberValue { get; init; }

    public double Min { get; init; }

    public double Max { get; init; }

    public double Step { get; init; } = 1;

    public bool BooleanValue { get; init; }

    public byte ColorR { get; init; }

    public byte ColorG { get; init; }

    public byte ColorB { get; init; }

    public IReadOnlyList<string> EnumOptions { get; init; } = Array.Empty<string>();

    public string? EnumValue { get; init; }
}
