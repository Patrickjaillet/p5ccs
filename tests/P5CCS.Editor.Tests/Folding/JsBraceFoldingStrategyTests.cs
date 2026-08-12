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

    [Fact]
    public void CreateNewFoldings_MismatchedBracketTypes_DoesNotProduceSpuriousLargeFolding()
    {
        var document = new TextDocument(
            "function draw() {\n" +
            "  background(30);\n" +
            "  fill(0kkopkpokpok\n" +
            "\n" +
            "okpokpokpok\n" +
            "\n" +
            "pijpjp\n" +
            "ke();\n" +
            "  circle(x, y, 40);\n" +
            "}\n");

        var foldings = JsBraceFoldingStrategy.CreateNewFoldings(document).ToList();

        Assert.All(foldings, folding =>
        {
            var startLine = document.GetLineByOffset(folding.StartOffset).LineNumber;
            var endLine = document.GetLineByOffset(folding.EndOffset).LineNumber;
            Assert.True(endLine - startLine <= document.LineCount, "Folding should not span more lines than the document contains.");
        });

        var outerFolding = Assert.Single(foldings);
        var outerStartLine = document.GetLineByOffset(outerFolding.StartOffset).LineNumber;
        var outerEndLine = document.GetLineByOffset(outerFolding.EndOffset).LineNumber;
        Assert.Equal(1, outerStartLine);
        Assert.Equal(document.LineCount - 1, outerEndLine);
    }

    [Fact]
    public void CreateNewFoldings_BracketsInsideStringLiteral_AreIgnored()
    {
        var document = new TextDocument("function label() {\n  text(\"[not a fold] {nope}\", 0, 0);\n}\n");

        var foldings = JsBraceFoldingStrategy.CreateNewFoldings(document).ToList();

        Assert.Single(foldings);
    }

    [Fact]
    public void CreateNewFoldings_BracketsInsideLineComment_AreIgnored()
    {
        var document = new TextDocument("function noop() {\n  // stray brace {\n  return;\n}\n");

        var foldings = JsBraceFoldingStrategy.CreateNewFoldings(document).ToList();

        Assert.Single(foldings);
    }
}
