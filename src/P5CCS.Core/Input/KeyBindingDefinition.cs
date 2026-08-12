namespace P5CCS.Core.Input;

public sealed class KeyBindingDefinition
{
    public required string CommandName { get; init; }

    public required string DisplayName { get; init; }

    public required string Gesture { get; set; }
}
