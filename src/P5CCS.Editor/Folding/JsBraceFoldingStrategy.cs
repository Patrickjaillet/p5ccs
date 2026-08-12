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
        var openBraceOffsets = new Stack<int>();

        var text = document.Text;

        for (var offset = 0; offset < text.Length; offset++)
        {
            var c = text[offset];

            if (c == '{' || c == '[')
            {
                openBraceOffsets.Push(offset);
            }
            else if ((c == '}' || c == ']') && openBraceOffsets.Count > 0)
            {
                var start = openBraceOffsets.Pop();
                if (offset - start > 1 && document.GetLineByOffset(start).LineNumber != document.GetLineByOffset(offset).LineNumber)
                {
                    foldings.Add(new NewFolding(start, offset + 1));
                }
            }
        }

        foldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
        return foldings;
    }
}
