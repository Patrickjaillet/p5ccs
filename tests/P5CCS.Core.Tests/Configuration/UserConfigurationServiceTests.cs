using P5CCS.Core.Configuration;

namespace P5CCS.Core.Tests.Configuration;

public class UserConfigurationServiceTests : IDisposable
{
    private readonly string _filePath;

    public UserConfigurationServiceTests()
    {
        _filePath = Path.Combine(Path.GetTempPath(), $"p5ccs-config-{Guid.NewGuid():N}.json");
    }

    [Fact]
    public void Get_WithoutStoredValue_ReturnsDefault()
    {
        var sut = new UserConfigurationService(_filePath);

        var value = sut.Get("missing-key", "fallback");

        Assert.Equal("fallback", value);
    }

    [Fact]
    public void SetAndSave_ThenReload_PersistsValue()
    {
        var sut = new UserConfigurationService(_filePath);
        sut.Set("editor.fontSize", 14);
        sut.Save();

        var reloaded = new UserConfigurationService(_filePath);

        Assert.Equal(14, reloaded.Get<int>("editor.fontSize"));
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }
}
