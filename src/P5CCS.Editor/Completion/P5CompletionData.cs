using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace P5CCS.Editor.Completion;

public sealed class P5CompletionData : ICompletionData
{
    private readonly P5ApiEntry _entry;

    public P5CompletionData(P5ApiEntry entry)
    {
        _entry = entry;
    }

    public ImageSource? Image => null;

    public string Text => _entry.Name;

    public object Content => _entry.Signature;

    public object Description => _entry.Description;

    public double Priority => 0;

    public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
    {
        textArea.Document.Replace(completionSegment, _entry.Name);
    }
}
