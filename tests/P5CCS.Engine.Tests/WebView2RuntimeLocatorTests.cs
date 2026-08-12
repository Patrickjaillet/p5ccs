namespace P5CCS.Engine.Tests;

public class WebView2RuntimeLocatorTests
{
    private static string RuntimeFolder => Path.Combine(AppContext.BaseDirectory, "WebView2Runtime");

    [Fact]
    public void FixedVersionRuntimePath_WhenFolderAbsent_ReturnsNull()
    {
        if (Directory.Exists(RuntimeFolder))
        {
            Directory.Delete(RuntimeFolder, recursive: true);
        }

        Assert.Null(WebView2RuntimeLocator.FixedVersionRuntimePath);
    }

    [Fact]
    public void FixedVersionRuntimePath_WhenFolderPresentButMissingLoader_ReturnsNull()
    {
        Directory.CreateDirectory(RuntimeFolder);
        try
        {
            Assert.Null(WebView2RuntimeLocator.FixedVersionRuntimePath);
        }
        finally
        {
            Directory.Delete(RuntimeFolder, recursive: true);
        }
    }

    [Fact]
    public void FixedVersionRuntimePath_WhenFolderAndLoaderPresent_ReturnsPath()
    {
        Directory.CreateDirectory(RuntimeFolder);
        File.WriteAllText(Path.Combine(RuntimeFolder, "msedgewebview2.exe"), string.Empty);
        try
        {
            Assert.Equal(RuntimeFolder, WebView2RuntimeLocator.FixedVersionRuntimePath);
        }
        finally
        {
            Directory.Delete(RuntimeFolder, recursive: true);
        }
    }
}
