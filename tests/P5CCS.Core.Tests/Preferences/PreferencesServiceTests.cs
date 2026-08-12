using P5CCS.Core.Preferences;

namespace P5CCS.Core.Tests.Preferences;

public class PreferencesServiceTests : IDisposable
{
    private readonly string _filePath;

    public PreferencesServiceTests()
    {
        _filePath = Path.Combine(Path.GetTempPath(), $"p5ccs-preferences-{Guid.NewGuid():N}.json");
    }

    [Fact]
    public void Current_WithoutStoredFile_ReturnsDefaults()
    {
        var sut = new PreferencesService(_filePath);

        Assert.Equal(AppTheme.System, sut.Current.Theme);
        Assert.Equal("en", sut.Current.UiLanguage);
    }

    [Fact]
    public void Save_ThenReload_PersistsChanges()
    {
        var sut = new PreferencesService(_filePath);
        sut.Current.Theme = AppTheme.Dark;
        sut.Current.LastProjectPath = @"C:\sketches\demo.p5ccsproj";
        sut.Save();

        var reloaded = new PreferencesService(_filePath);

        Assert.Equal(AppTheme.Dark, reloaded.Current.Theme);
        Assert.Equal(@"C:\sketches\demo.p5ccsproj", reloaded.Current.LastProjectPath);
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }
}
