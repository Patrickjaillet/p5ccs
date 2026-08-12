using P5CCS.Core.Configuration;

namespace P5CCS.Core.Input;

public sealed class KeyBindingsService : IKeyBindingsService
{
    private const string StorageKey = "input.keyBindings";

    private static readonly IReadOnlyList<KeyBindingDefinition> Defaults = new List<KeyBindingDefinition>
    {
        new() { CommandName = "NewSketch", DisplayName = "New Sketch", Gesture = "Ctrl+N" },
        new() { CommandName = "OpenSketch", DisplayName = "Open Sketch", Gesture = "Ctrl+O" },
        new() { CommandName = "SaveSketch", DisplayName = "Save Sketch", Gesture = "Ctrl+S" },
        new() { CommandName = "CloseTab", DisplayName = "Close Tab", Gesture = "Ctrl+W" },
        new() { CommandName = "Run", DisplayName = "Run Sketch", Gesture = "F5" },
        new() { CommandName = "Stop", DisplayName = "Stop Sketch", Gesture = "Shift+F5" },
        new() { CommandName = "Reset", DisplayName = "Reset Sketch", Gesture = "Ctrl+F5" },
        new() { CommandName = "QuickExport", DisplayName = "Quick Export", Gesture = "Ctrl+E" },
        new() { CommandName = "ToggleTheme", DisplayName = "Toggle Theme", Gesture = "Ctrl+T" },
        new() { CommandName = "FullscreenViewport", DisplayName = "Fullscreen Viewport", Gesture = "F11" },
    };

    private readonly IUserConfigurationService _configurationService;
    private readonly List<KeyBindingDefinition> _bindings;

    public KeyBindingsService(IUserConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _bindings = LoadBindings();
    }

    public IReadOnlyList<KeyBindingDefinition> Bindings => _bindings;

    public void SetGesture(string commandName, string gesture)
    {
        var binding = _bindings.FirstOrDefault(b => b.CommandName == commandName);
        if (binding is not null)
        {
            binding.Gesture = gesture;
        }
    }

    public void ResetToDefaults()
    {
        _bindings.Clear();
        _bindings.AddRange(Defaults.Select(d => new KeyBindingDefinition
        {
            CommandName = d.CommandName,
            DisplayName = d.DisplayName,
            Gesture = d.Gesture,
        }));
    }

    public void Save()
    {
        var overrides = _bindings.ToDictionary(b => b.CommandName, b => b.Gesture);
        _configurationService.Set(StorageKey, overrides);
        _configurationService.Save();
    }

    private List<KeyBindingDefinition> LoadBindings()
    {
        var overrides = _configurationService.Get<Dictionary<string, string>>(StorageKey) ?? new Dictionary<string, string>();

        return Defaults
            .Select(d => new KeyBindingDefinition
            {
                CommandName = d.CommandName,
                DisplayName = d.DisplayName,
                Gesture = overrides.GetValueOrDefault(d.CommandName, d.Gesture),
            })
            .ToList();
    }
}
