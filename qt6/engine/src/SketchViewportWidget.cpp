#include "p5ccs/engine/SketchViewportWidget.h"

#include <QLabel>
#include <QVBoxLayout>

namespace p5ccs::engine {

SketchViewportWidget::SketchViewportWidget(QWidget* parent)
    : QWidget(parent)
{
    auto* layout = new QVBoxLayout(this);
    auto* placeholder = new QLabel(
        tr("Sketch viewport (Qt6 rewrite foundation)\n"
           "QWebEngineView integration not yet implemented."),
        this);
    placeholder->setAlignment(Qt::AlignCenter);
    layout->addWidget(placeholder);
}

} // namespace p5ccs::engine
