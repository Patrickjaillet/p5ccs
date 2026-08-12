using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;

namespace P5CCS.Editor.Folding;

public sealed class JsBraceFoldingStrategy
{
    public void UpdateFoldings(FoldingManager manager, TextDocument document)
    {
        var newFoldings = CreateNewFoldings(document);
        manager.UpdateFoldings(newFoldings, -1);
    }

    public static IEnumerable<NewFolding> CreateNewFoldings(TextDocument document)
    {
        var foldings = new List<NewFolding>();
        var openBraces = new Stack<(int Offset, char Character)>();

        var text = document.Text;

        var inLineComment = false;
        var inBlockComment = false;
        var stringDelimiter = '\0';

        for (var offset = 0; offset < text.Length; offset++)
        {
            var c = text[offset];
            var next = offset + 1 < text.Length ? text[offset + 1] : '\0';

            if (inLineComment)
            {
                if (c == '\n')
                {
                    inLineComment = false;
                }

                continue;
            }

            if (inBlockComment)
            {
                if (c == '*' && next == '/')
                {
                    inBlockComment = false;
                    offset++;
                }

                continue;
            }

            if (stringDelimiter != '\0')
            {
                if (c == '\\')
                {
                    offset++;
                }
                else if (c == stringDelimiter)
                {
                    stringDelimiter = '\0';
                }

                continue;
            }

            if (c == '/' && next == '/')
            {
                inLineComment = true;
                offset++;
                continue;
            }

            if (c == '/' && next == '*')
            {
                inBlockComment = true;
                offset++;
                continue;
            }

            if (c is '"' or '\'' or '`')
            {
                stringDelimiter = c;
                continue;
            }

            if (c == '{' || c == '[')
            {
                openBraces.Push((offset, c));
            }
            else if (c == '}' || c == ']')
            {
                var expectedOpener = c == '}' ? '{' : '[';
                if (openBraces.Count > 0 && openBraces.Peek().Character == expectedOpener)
                {
                    var (start, _) = openBraces.Pop();
                    if (offset - start > 1 && document.GetLineByOffset(start).LineNumber != document.GetLineByOffset(offset).LineNumber)
                    {
                        foldings.Add(new NewFolding(start, offset + 1));
                    }
                }
            }
        }

        foldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
        return foldings;
    }
}
