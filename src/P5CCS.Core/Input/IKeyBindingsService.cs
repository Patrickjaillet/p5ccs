namespace P5CCS.Core.Input;

public interface IKeyBindingsService
{
    IReadOnlyList<KeyBindingDefinition> Bindings { get; }

    void SetGesture(string commandName, string gesture);

    void ResetToDefaults();

    void Save();
}
