using P5CCS.Core.Sliders;

namespace P5CCS.Core.Tests.Sliders;

public class SketchSourceAnalyzerTests
{
    [Fact]
    public void Analyze_TopLevelNumericDeclaration_IsDetected()
    {
        var source = "let speed = 3;\n\nfunction setup() {\n  createCanvas(800, 450);\n}\n";

        var candidates = SketchSourceAnalyzer.Analyze(source);

        var speed = Assert.Single(candidates);
        Assert.Equal("speed", speed.Name);
        Assert.Equal(SliderControlKind.Number, speed.Kind);
        Assert.Equal(3, speed.NumberValue);
    }

    [Fact]
    public void Analyze_DeclarationInsideFunction_IsNotDetected()
    {
        var source = "function setup() {\n  let inner = 5;\n}\n";

        var candidates = SketchSourceAnalyzer.Analyze(source);

        Assert.Empty(candidates);
    }

    [Fact]
    public void Analyze_BooleanDeclaration_IsDetectedAsBoolean()
    {
        var source = "let showGrid = true;\n";

        var candidates = SketchSourceAnalyzer.Analyze(source);

        var candidate = Assert.Single(candidates);
        Assert.Equal(SliderControlKind.Boolean, candidate.Kind);
        Assert.True(candidate.BooleanValue);
    }

    [Fact]
    public void Analyze_ColorFunctionCall_IsDetectedWithRgbValues()
    {
        var source = "function draw() {\n  fill(200, 100, 50);\n}\n";

        var candidates = SketchSourceAnalyzer.Analyze(source);

        var candidate = Assert.Single(candidates);
        Assert.Equal(SliderControlKind.Color, candidate.Kind);
        Assert.Equal(200, candidate.ColorR);
        Assert.Equal(100, candidate.ColorG);
        Assert.Equal(50, candidate.ColorB);
    }

    [Fact]
    public void Analyze_AngleNamedVariable_InfersZeroToTwoPiBounds()
    {
        var source = "let rotationAngle = 0;\n";

        var candidates = SketchSourceAnalyzer.Analyze(source);

        var candidate = Assert.Single(candidates);
        Assert.Equal(0, candidate.Min);
        Assert.Equal(Math.Round(2 * Math.PI, 3), candidate.Max);
    }

    [Fact]
    public void Analyze_SliderAnnotation_OverridesInferredBounds()
    {
        var source = "// @slider 10 20 0.5\nlet size = 15;\n";

        var candidates = SketchSourceAnalyzer.Analyze(source);

        var candidate = Assert.Single(candidates);
        Assert.True(candidate.IsBoundsAnnotated);
        Assert.Equal(10, candidate.Min);
        Assert.Equal(20, candidate.Max);
        Assert.Equal(0.5, candidate.Step);
    }

    [Fact]
    public void Analyze_EnumAnnotation_ProducesEnumCandidate()
    {
        var source = "// @slider enum small, medium, large\nlet sizeMode = 1;\n";

        var candidates = SketchSourceAnalyzer.Analyze(source);

        var candidate = Assert.Single(candidates);
        Assert.Equal(SliderControlKind.Enum, candidate.Kind);
        Assert.Equal(new[] { "small", "medium", "large" }, candidate.EnumOptions);
        Assert.Equal("medium", candidate.EnumValue);
    }

    [Fact]
    public void Analyze_PrecedingCommentWithoutAnnotation_BecomesGroupName()
    {
        var source = "// Motion\nlet speed = 3;\n";

        var candidates = SketchSourceAnalyzer.Analyze(source);

        var candidate = Assert.Single(candidates);
        Assert.Equal("Motion", candidate.GroupName);
    }

    [Fact]
    public void Analyze_OffsetAndLength_PointToValueTextOnly()
    {
        var source = "let x = 42;\n";

        var candidates = SketchSourceAnalyzer.Analyze(source);

        var candidate = Assert.Single(candidates);
        var extracted = source.Substring(candidate.Offset, candidate.Length);
        Assert.Equal("42", extracted);
    }
}
