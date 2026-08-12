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
}
