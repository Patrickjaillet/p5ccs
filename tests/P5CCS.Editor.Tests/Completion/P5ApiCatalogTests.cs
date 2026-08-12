using P5CCS.Editor.Completion;

namespace P5CCS.Editor.Tests.Completion;

public class P5ApiCatalogTests
{
    [Fact]
    public void Entries_IsNotEmpty()
    {
        Assert.NotEmpty(P5ApiCatalog.Entries);
    }

    [Fact]
    public void Entries_HaveNoDuplicateNames()
    {
        var names = P5ApiCatalog.Entries.Select(e => e.Name).ToList();

        Assert.Equal(names.Count, names.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Entries_AllHaveNonEmptySignatureAndDescription()
    {
        Assert.All(P5ApiCatalog.Entries, entry =>
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.Name));
            Assert.False(string.IsNullOrWhiteSpace(entry.Signature));
            Assert.False(string.IsNullOrWhiteSpace(entry.Description));
        });
    }

    [Theory]
    [InlineData("setup")]
    [InlineData("draw")]
    [InlineData("createCanvas")]
    [InlineData("fill")]
    [InlineData("background")]
    public void Entries_ContainCoreP5Functions(string expectedName)
    {
        Assert.Contains(P5ApiCatalog.Entries, e => e.Name == expectedName);
    }

    [Theory]
    [InlineData("loadSound", "Sound")]
    [InlineData("p5.Oscillator", "Sound")]
    [InlineData("createShader", "WEBGL / 3D")]
    [InlineData("sphere", "WEBGL / 3D")]
    [InlineData("loadModel", "WEBGL / 3D")]
    [InlineData("loadJSON", "Vector & Data")]
    [InlineData("p5.Table", "Vector & Data")]
    [InlineData("createNumberDict", "Vector & Data")]
    [InlineData("loadImage", "Image")]
    [InlineData("createDiv", "DOM")]
    public void Entries_ContainExpandedApiModulesWithCorrectCategory(string expectedName, string expectedCategory)
    {
        var entry = Assert.Single(P5ApiCatalog.Entries, e => e.Name == expectedName);
        Assert.Equal(expectedCategory, entry.Category);
    }

    [Fact]
    public void Entries_AllHaveNonEmptyCategory()
    {
        Assert.All(P5ApiCatalog.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Category)));
    }
}
