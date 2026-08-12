using System.Text.RegularExpressions;

namespace P5CCS.Core.Sliders;

public static partial class SketchSourceAnalyzer
{
    private const string DefaultNumberGroup = "Variables";
    private const string DefaultColorGroup = "Colors";
    private const string DefaultBooleanGroup = "Flags";

    public static IReadOnlyList<SliderCandidate> Analyze(string source)
    {
        var candidates = new List<SliderCandidate>();
        candidates.AddRange(AnalyzeTopLevelDeclarations(source));
        candidates.AddRange(AnalyzeColorCalls(source));
        return candidates.OrderBy(c => c.Offset).ToList();
    }

    private static IEnumerable<SliderCandidate> AnalyzeTopLevelDeclarations(string source)
    {
        var lines = SplitLinesWithOffsets(source);
        var depths = ComputeLineStartDepths(lines);

        for (var i = 0; i < lines.Count; i++)
        {
            if (depths[i] != 0)
            {
                continue;
            }

            var (lineText, lineOffset) = lines[i];
            var match = DeclarationRegex().Match(lineText);
            if (!match.Success)
            {
                continue;
            }

            var name = match.Groups["name"].Value;
            var valueText = match.Groups["value"].Value;
            var valueGroup = match.Groups["value"];
            var absoluteOffset = lineOffset + valueGroup.Index;

            var annotation = FindAnnotation(lines, i);

            if (valueText is "true" or "false")
            {
                yield return new SliderCandidate
                {
                    Name = name,
                    Kind = SliderControlKind.Boolean,
                    GroupName = annotation.GroupName ?? DefaultBooleanGroup,
                    Offset = absoluteOffset,
                    Length = valueText.Length,
                    LineNumber = i + 1,
                    BooleanValue = valueText == "true",
                };
                continue;
            }

            if (!double.TryParse(valueText, System.Globalization.CultureInfo.InvariantCulture, out var numberValue))
            {
                continue;
            }

            if (annotation.EnumOptions is { Count: > 0 })
            {
                yield return new SliderCandidate
                {
                    Name = name,
                    Kind = SliderControlKind.Enum,
                    GroupName = annotation.GroupName ?? DefaultNumberGroup,
                    Offset = absoluteOffset,
                    Length = valueText.Length,
                    LineNumber = i + 1,
                    EnumOptions = annotation.EnumOptions,
                    EnumValue = annotation.EnumOptions.ElementAtOrDefault((int)numberValue),
                };
                continue;
            }

            var (min, max, step) = annotation.Bounds ?? InferNumericBounds(name, numberValue);

            yield return new SliderCandidate
            {
                Name = name,
                Kind = SliderControlKind.Number,
                GroupName = annotation.GroupName ?? DefaultNumberGroup,
                Offset = absoluteOffset,
                Length = valueText.Length,
                LineNumber = i + 1,
                NumberValue = numberValue,
                Min = min,
                Max = max,
                Step = step,
                IsBoundsAnnotated = annotation.Bounds is not null,
            };
        }
    }

    private static IEnumerable<SliderCandidate> AnalyzeColorCalls(string source)
    {
        foreach (Match match in ColorCallRegex().Matches(source))
        {
            var argsGroup = match.Groups["args"];
            var r = byte.Parse(match.Groups["r"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var g = byte.Parse(match.Groups["g"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var b = byte.Parse(match.Groups["b"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var lineNumber = source[..match.Index].Count(c => c == '\n') + 1;

            yield return new SliderCandidate
            {
                Name = match.Groups["fn"].Value,
                Kind = SliderControlKind.Color,
                GroupName = DefaultColorGroup,
                Offset = argsGroup.Index,
                Length = argsGroup.Length,
                LineNumber = lineNumber,
                ColorR = r,
                ColorG = g,
                ColorB = b,
            };
        }
    }

    private static (double Min, double Max, double Step) InferNumericBounds(string name, double value)
    {
        var lowerName = name.ToLowerInvariant();

        if (lowerName.Contains("angle") || lowerName.Contains("rot") || lowerName.Contains("theta"))
        {
            return (0, Math.Round(2 * Math.PI, 3), 0.01);
        }

        if (lowerName.Contains("alpha") || lowerName.Contains("opacity"))
        {
            return (0, 255, 1);
        }

        var hasDecimals = value != Math.Floor(value);
        var step = hasDecimals ? 0.1 : 1;

        if (value >= 0)
        {
            var max = Math.Max(value * 2, value + 10);
            return (0, RoundStep(max, step), step);
        }

        var bound = Math.Abs(value) * 2;
        return (RoundStep(-bound, step), RoundStep(bound, step), step);
    }

    private static double RoundStep(double value, double step) => step < 1 ? Math.Round(value, 2) : Math.Round(value);

    private static List<(string Text, int Offset)> SplitLinesWithOffsets(string source)
    {
        var result = new List<(string, int)>();
        var offset = 0;

        foreach (var line in source.Split('\n'))
        {
            result.Add((line.TrimEnd('\r'), offset));
            offset += line.Length + 1;
        }

        return result;
    }

    private static int[] ComputeLineStartDepths(List<(string Text, int Offset)> lines)
    {
        var depths = new int[lines.Count];
        var depth = 0;

        for (var i = 0; i < lines.Count; i++)
        {
            depths[i] = depth;
            foreach (var c in lines[i].Text)
            {
                if (c == '{')
                {
                    depth++;
                }
                else if (c == '}')
                {
                    depth = Math.Max(0, depth - 1);
                }
            }
        }

        return depths;
    }

    private readonly record struct AnnotationInfo(string? GroupName, (double Min, double Max, double Step)? Bounds, IReadOnlyList<string>? EnumOptions);

    private static AnnotationInfo FindAnnotation(List<(string Text, int Offset)> lines, int declarationLineIndex)
    {
        var i = declarationLineIndex - 1;
        while (i >= 0 && string.IsNullOrWhiteSpace(lines[i].Text))
        {
            i--;
        }

        if (i < 0)
        {
            return default;
        }

        var text = lines[i].Text.Trim();

        var boundsMatch = SliderBoundsAnnotationRegex().Match(text);
        if (boundsMatch.Success)
        {
            var min = double.Parse(boundsMatch.Groups["min"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var max = double.Parse(boundsMatch.Groups["max"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var step = boundsMatch.Groups["step"].Success
                ? double.Parse(boundsMatch.Groups["step"].Value, System.Globalization.CultureInfo.InvariantCulture)
                : 1;
            return new AnnotationInfo(null, (min, max, step), null);
        }

        var enumMatch = SliderEnumAnnotationRegex().Match(text);
        if (enumMatch.Success)
        {
            var options = enumMatch.Groups["options"].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();
            return new AnnotationInfo(null, null, options);
        }

        if (text.StartsWith("//", StringComparison.Ordinal))
        {
            var group = text[2..].Trim();
            return string.IsNullOrWhiteSpace(group) ? default : new AnnotationInfo(group, null, null);
        }

        return default;
    }

    [GeneratedRegex(@"^\s*(?:let|const|var)\s+(?<name>[A-Za-z_$][\w$]*)\s*=\s*(?<value>-?\d+(?:\.\d+)?|true|false)\s*;")]
    private static partial Regex DeclarationRegex();

    [GeneratedRegex(@"\b(?<fn>fill|stroke|background)\s*\(\s*(?<args>(?<r>\d{1,3})\s*,\s*(?<g>\d{1,3})\s*,\s*(?<b>\d{1,3}))\s*\)")]
    private static partial Regex ColorCallRegex();

    [GeneratedRegex(@"^//\s*@slider\s+(?<min>-?\d+(?:\.\d+)?)\s+(?<max>-?\d+(?:\.\d+)?)(?:\s+(?<step>-?\d+(?:\.\d+)?))?\s*$")]
    private static partial Regex SliderBoundsAnnotationRegex();

    [GeneratedRegex(@"^//\s*@slider\s+enum\s+(?<options>.+)$")]
    private static partial Regex SliderEnumAnnotationRegex();
}
