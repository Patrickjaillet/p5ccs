using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Editing;

namespace P5CCS.Editor.ErrorMarkers;

public sealed class ErrorMargin : AbstractMargin
{
    private readonly TextArea _textArea;

    public ErrorMargin(TextArea textArea)
    {
        _textArea = textArea;
        Width = 16;
        Cursor = Cursors.Hand;
    }

    public IReadOnlyList<EditorErrorMarker> Markers { get; set; } = Array.Empty<EditorErrorMarker>();

    protected override Size MeasureOverride(Size availableSize) => new(16, 0);

    protected override void OnRender(DrawingContext drawingContext)
    {
        drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));

        if (TextView is null || Document is null || Markers.Count == 0)
        {
            return;
        }

        foreach (var marker in Markers)
        {
            if (marker.Line < 1 || marker.Line > Document.LineCount)
            {
                continue;
            }

            var y = TextView.GetVisualTopByDocumentLine(marker.Line) - TextView.VerticalOffset;
            var center = new Point(8, y + TextView.DefaultLineHeight / 2);
            drawingContext.DrawEllipse(Brushes.OrangeRed, null, center, 4, 4);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var marker = FindMarkerAt(e.GetPosition(this).Y);
        ToolTip = marker?.Message;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        var marker = FindMarkerAt(e.GetPosition(this).Y);
        if (marker is null || TextView is null)
        {
            return;
        }

        var line = Document.GetLineByNumber(marker.Line);
        _textArea.Caret.Offset = line.Offset;
        _textArea.Caret.BringCaretToView();
        _textArea.Focus();
    }

    private EditorErrorMarker? FindMarkerAt(double y)
    {
        if (TextView is null)
        {
            return null;
        }

        var documentY = y + TextView.VerticalOffset;
        var visualLine = TextView.GetVisualLineFromVisualTop(documentY);
        if (visualLine is null)
        {
            return null;
        }

        var lineNumber = visualLine.FirstDocumentLine.LineNumber;
        return Markers.FirstOrDefault(m => m.Line == lineNumber);
    }
}
