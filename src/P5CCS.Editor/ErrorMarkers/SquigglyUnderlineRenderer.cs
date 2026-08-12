using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace P5CCS.Editor.ErrorMarkers;

public sealed class SquigglyUnderlineRenderer : IBackgroundRenderer
{
    private static readonly Pen ErrorPen = CreatePen();

    public IReadOnlyList<EditorErrorMarker> Markers { get; set; } = Array.Empty<EditorErrorMarker>();

    public KnownLayer Layer => KnownLayer.Selection;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (Markers.Count == 0 || textView.Document is null)
        {
            return;
        }

        foreach (var marker in Markers)
        {
            if (marker.Line < 1 || marker.Line > textView.Document.LineCount)
            {
                continue;
            }

            var line = textView.Document.GetLineByNumber(marker.Line);
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, line, false))
            {
                DrawSquiggly(drawingContext, rect);
            }
        }
    }

    private static void DrawSquiggly(DrawingContext drawingContext, Rect rect)
    {
        const double amplitude = 2.5;
        var geometry = new StreamGeometry();

        using (var ctx = geometry.Open())
        {
            var start = new Point(rect.Left, rect.Bottom);
            ctx.BeginFigure(start, false, false);

            var x = rect.Left;
            var up = true;
            while (x < rect.Right)
            {
                var nextX = Math.Min(x + 4, rect.Right);
                var y = rect.Bottom + (up ? -amplitude : amplitude);
                ctx.LineTo(new Point(nextX, y), true, false);
                x = nextX;
                up = !up;
            }
        }

        geometry.Freeze();
        drawingContext.DrawGeometry(null, ErrorPen, geometry);
    }

    private static Pen CreatePen()
    {
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(0xE5, 0x14, 0x00)), 1.0);
        pen.Freeze();
        return pen;
    }
}
