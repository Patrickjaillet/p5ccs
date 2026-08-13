#pragma once

#include <QPlainTextEdit>

namespace p5ccs::editor {

// Foundation for the code editor. Genuinely functional as a plain text
// editor today (QPlainTextEdit is real, not a stub); p5.js-aware syntax
// highlighting, folding, autocomplete, and inline error markers — the
// features the legacy AvalonEdit-based editor had — are not yet ported.
// A future pass should either extend this with QSyntaxHighlighter or
// evaluate QScintilla, per the open decision in
// qt6/docs/QT6-MIGRATION.md.
class CodeEditorWidget : public QPlainTextEdit {
    Q_OBJECT

public:
    explicit CodeEditorWidget(QWidget* parent = nullptr);
};

} // namespace p5ccs::editor
