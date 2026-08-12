using ICSharpCode.AvalonEdit.Document;
using P5CCS.Editor.Folding;

namespace P5CCS.Editor.Tests.Folding;

public class JsBraceFoldingStrategyTests
{
    [Fact]
    public void CreateNewFoldings_MultilineFunctionBody_ProducesOneFolding()
    {
        var document = new TextDocument("function setup() {\n  createCanvas(800, 450);\n}\n");

        var foldings = JsBraceFoldingStrategy.CreateNewFoldings(document).ToList();

        Assert.Single(foldings);
    }

    [Fact]
    public void CreateNewFoldings_SingleLineBraces_ProducesNoFolding()
    {
        var document = new TextDocument("function noop() { return; }\n");

        var foldings = JsBraceFoldingStrategy.CreateNewFoldings(document).ToList();

        Assert.Empty(foldings);
    }

    [Fact]
    public void CreateNewFoldings_NestedBlocks_ProducesFoldingPerBlock()
    {
        var document = new TextDocument("function draw() {\n  if (true) {\n    background(0);\n  }\n}\n");

        var foldings = JsBraceFoldingStrategy.CreateNewFoldings(document).ToList();

        Assert.Equal(2, foldings.Count);
    }

    [Fact]
    public void CreateNewFoldings_UnbalancedBraces_DoesNotThrow()
    {
        var document = new TextDocument("function broken() {\n  background(0);\n");

        var exception = Record.Exception(() => JsBraceFoldingStrategy.CreateNewFoldings(document).ToList());

        Assert.Null(exception);
    }
}
