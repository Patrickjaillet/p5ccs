#include "p5ccs/editor/CodeEditorWidget.h"

#include <QFont>

namespace p5ccs::editor {

CodeEditorWidget::CodeEditorWidget(QWidget* parent)
    : QPlainTextEdit(parent)
{
    QFont font("Consolas");
    font.setStyleHint(QFont::Monospace);
    font.setPointSize(11);
    setFont(font);
    setLineWrapMode(QPlainTextEdit::NoWrap);
    setPlaceholderText(tr("// New sketch"));
}

} // namespace p5ccs::editor
