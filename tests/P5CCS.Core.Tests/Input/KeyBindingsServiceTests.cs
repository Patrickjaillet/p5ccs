using P5CCS.Core.Configuration;
using P5CCS.Core.Input;

namespace P5CCS.Core.Tests.Input;

public class KeyBindingsServiceTests : IDisposable
{
    private readonly string _filePath;

    public KeyBindingsServiceTests()
    {
        _filePath = Path.Combine(Path.GetTempPath(), $"p5ccs-keybindings-{Guid.NewGuid():N}.json");
    }

    [Fact]
    public void Bindings_WithoutStoredOverrides_ReturnsDefaults()
    {
        var configurationService = new UserConfigurationService(_filePath);
        var sut = new KeyBindingsService(configurationService);

        var runBinding = sut.Bindings.Single(b => b.CommandName == "Run");

        Assert.Equal("F5", runBinding.Gesture);
    }

    [Fact]
    public void SetGesture_ThenSave_PersistsAcrossInstances()
    {
        var configurationService = new UserConfigurationService(_filePath);
        var sut = new KeyBindingsService(configurationService);

        sut.SetGesture("Run", "Ctrl+R");
        sut.Save();

        var reloadedConfiguration = new UserConfigurationService(_filePath);
        var reloaded = new KeyBindingsService(reloadedConfiguration);

        Assert.Equal("Ctrl+R", reloaded.Bindings.Single(b => b.CommandName == "Run").Gesture);
    }

    [Fact]
    public void ResetToDefaults_RestoresOriginalGesture()
    {
        var configurationService = new UserConfigurationService(_filePath);
        var sut = new KeyBindingsService(configurationService);
        sut.SetGesture("Run", "Ctrl+R");

        sut.ResetToDefaults();

        Assert.Equal("F5", sut.Bindings.Single(b => b.CommandName == "Run").Gesture);
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }
}
