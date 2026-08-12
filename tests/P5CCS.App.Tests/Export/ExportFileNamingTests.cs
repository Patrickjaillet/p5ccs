using P5CCS.App.Export;

namespace P5CCS.App.Tests.Export;

public class ExportFileNamingTests
{
    [Fact]
    public void GenerateFileName_MatchesExpectedPattern()
    {
        var fileName = ExportFileNaming.GenerateFileName("My Sketch.js", "webm");

        Assert.Matches(@"^my-sketch_v[0-9a-zA-Z.\-]+_\d{8}-\d{6}\.webm$", fileName);
    }

    [Fact]
    public void GenerateFileName_UsesRequestedExtension()
    {
        var fileName = ExportFileNaming.GenerateFileName("bouncing-ball.js", "gif");

        Assert.EndsWith(".gif", fileName, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("My Sketch!!.js", "my-sketch")]
    [InlineData("  spaced  out  ", "spaced-out")]
    [InlineData("Sketch_With_Underscores", "sketch-with-underscores")]
    public void GenerateFileName_SlugifiesTitle(string title, string expectedSlugPrefix)
    {
        var fileName = ExportFileNaming.GenerateFileName(title, "png");

        Assert.StartsWith(expectedSlugPrefix + "_v", fileName, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateFileName_EmptyTitle_FallsBackToSketch()
    {
        var fileName = ExportFileNaming.GenerateFileName("", "png");

        Assert.StartsWith("sketch_v", fileName, StringComparison.Ordinal);
    }

    [Fact]
    public void GenerateFileName_TwoCallsHaveDistinctOrEqualTimestampsButNeverThrow()
    {
        var first = ExportFileNaming.GenerateFileName("test", "png");
        var second = ExportFileNaming.GenerateFileName("test", "png");

        Assert.Matches(@"_\d{8}-\d{6}\.png$", first);
        Assert.Matches(@"_\d{8}-\d{6}\.png$", second);
    }
}
