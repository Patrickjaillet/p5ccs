using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using ICSharpCode.AvalonEdit.Search;
using P5CCS.Editor.Completion;
using P5CCS.Editor.ErrorMarkers;
using P5CCS.Editor.Folding;

namespace P5CCS.Editor;

public partial class SketchCodeEditor : UserControl
{
    private static readonly IHighlightingDefinition DarkHighlighting = LoadHighlighting("P5JavaScriptDark.xshd");
    private static readonly IHighlightingDefinition LightHighlighting = LoadHighlighting("P5JavaScriptLight.xshd");

    private readonly JsBraceFoldingStrategy _foldingStrategy = new();
    private readonly DispatcherTimer _foldingTimer;
    private readonly DispatcherTimer _textChangedDebounceTimer;
    private readonly ToolTip _hoverToolTip = new();

    private FoldingManager? _foldingManager;
    private ErrorMargin? _errorMargin;
    private SquigglyUnderlineRenderer? _errorRenderer;
    private CompletionWindow? _completionWindow;
    private bool _suppressTextChanged;

    public SketchCodeEditor()
    {
        InitializeComponent();

        _foldingTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(400) };
        _foldingTimer.Tick += (_, _) =>
        {
            _foldingTimer.Stop();
            UpdateFoldings();
        };

        _textChangedDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        _textChangedDebounceTimer.Tick += (_, _) =>
        {
            _textChangedDebounceTimer.Stop();
            TextChangedDebounced?.Invoke(this, Editor.Text);
        };

        Editor.SyntaxHighlighting = DarkHighlighting;
        Editor.TextArea.IndentationStrategy = new CSharpIndentationStrategy(Editor.Options);
        Editor.Options.ConvertTabsToSpaces = true;
        Editor.Options.IndentationSize = 2;

        _foldingManager = FoldingManager.Install(Editor.TextArea);
        _errorMargin = new ErrorMargin(Editor.TextArea);
        _errorRenderer = new SquigglyUnderlineRenderer();
        Editor.TextArea.LeftMargins.Add(_errorMargin);
        Editor.TextArea.TextView.BackgroundRenderers.Add(_errorRenderer);

        SearchPanel.Install(Editor);

        Editor.TextArea.TextEntering += OnTextEntering;
        Editor.TextArea.TextEntered += OnTextEntered;
        Editor.TextChanged += OnEditorTextChanged;
        Editor.TextArea.TextView.MouseHover += OnMouseHover;
        Editor.TextArea.TextView.MouseHoverStopped += OnMouseHoverStopped;

        UpdateFoldings();
    }

    public event EventHandler<string>? TextChangedDebounced;

    public string Text
    {
        get => Editor.Text;
        set
        {
            if (Editor.Text == value)
            {
                return;
            }

            _suppressTextChanged = true;
            var caretOffset = Math.Min(Editor.CaretOffset, value.Length);
            Editor.Text = value;
            Editor.CaretOffset = caretOffset;
            _suppressTextChanged = false;
        }
    }

    public void ApplyTheme(bool isDark)
    {
        Editor.SyntaxHighlighting = isDark ? DarkHighlighting : LightHighlighting;
        Editor.Background = isDark ? new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x1E)) : Brushes.White;
        Editor.Foreground = isDark ? Brushes.White : Brushes.Black;
    }

    public void SetErrors(IReadOnlyList<EditorErrorMarker> markers)
    {
        if (_errorMargin is not null)
        {
            _errorMargin.Markers = markers;
            _errorMargin.InvalidateVisual();
        }

        if (_errorRenderer is not null)
        {
            _errorRenderer.Markers = markers;
            Editor.TextArea.TextView.InvalidateLayer(_errorRenderer.Layer);
        }
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        _foldingTimer.Stop();
        _foldingTimer.Start();

        if (_suppressTextChanged)
        {
            return;
        }

        _textChangedDebounceTimer.Stop();
        _textChangedDebounceTimer.Start();
    }

    private void UpdateFoldings()
    {
        if (_foldingManager is not null)
        {
            _foldingStrategy.UpdateFoldings(_foldingManager, Editor.Document);
        }
    }

    private void OnTextEntering(object? sender, TextCompositionEventArgs e)
    {
        if (_completionWindow is null || e.Text.Length == 0)
        {
            return;
        }

        if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_')
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }

    private void OnTextEntered(object? sender, TextCompositionEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text) || !char.IsLetter(e.Text[0]))
        {
            return;
        }

        var wordStart = FindWordStart(Editor.CaretOffset);
        var currentWord = Editor.Document.GetText(wordStart, Editor.CaretOffset - wordStart);

        if (currentWord.Length < 2)
        {
            return;
        }

        var matches = P5ApiCatalog.Entries
            .Where(entry => entry.Name.StartsWith(currentWord, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
        {
            _completionWindow?.Close();
            return;
        }

        if (_completionWindow is null)
        {
            _completionWindow = new CompletionWindow(Editor.TextArea)
            {
                StartOffset = wordStart,
            };
            _completionWindow.Closed += (_, _) => _completionWindow = null;
            foreach (var match in matches)
            {
                _completionWindow.CompletionList.CompletionData.Add(new P5CompletionData(match));
            }

            _completionWindow.Show();
        }
    }

    private int FindWordStart(int offset)
    {
        var start = offset;
        while (start > 0 && (char.IsLetterOrDigit(Editor.Document.GetCharAt(start - 1)) || Editor.Document.GetCharAt(start - 1) == '_'))
        {
            start--;
        }

        return start;
    }

    private void OnMouseHover(object? sender, MouseEventArgs e)
    {
        var position = Editor.TextArea.TextView.GetPositionFloor(e.GetPosition(Editor.TextArea.TextView) + Editor.TextArea.TextView.ScrollOffset);
        if (position is null)
        {
            return;
        }

        var offset = Editor.Document.GetOffset(position.Value.Location);
        var wordStart = FindWordStart(offset);
        var wordEnd = wordStart;
        while (wordEnd < Editor.Document.TextLength &&
               (char.IsLetterOrDigit(Editor.Document.GetCharAt(wordEnd)) || Editor.Document.GetCharAt(wordEnd) == '_'))
        {
            wordEnd++;
        }

        if (wordEnd <= wordStart)
        {
            return;
        }

        var word = Editor.Document.GetText(wordStart, wordEnd - wordStart);
        var entry = P5ApiCatalog.Entries.FirstOrDefault(x => x.Name == word);
        if (entry is null)
        {
            return;
        }

        _hoverToolTip.PlacementTarget = Editor;
        _hoverToolTip.Content = $"{entry.Signature}\n{entry.Description}";
        _hoverToolTip.IsOpen = true;
        e.Handled = true;
    }

    private void OnMouseHoverStopped(object? sender, MouseEventArgs e)
    {
        _hoverToolTip.IsOpen = false;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control)
        {
            return;
        }

        var newSize = Editor.FontSize + (e.Delta > 0 ? 1 : -1);
        Editor.FontSize = Math.Clamp(newSize, 8, 40);
        e.Handled = true;
    }

    private static IHighlightingDefinition LoadHighlighting(string resourceFileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"P5CCS.Editor.Highlighting.{resourceFileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded resource '{resourceName}' was not found.");
        using var reader = new XmlTextReader(stream);
        return HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }
}
