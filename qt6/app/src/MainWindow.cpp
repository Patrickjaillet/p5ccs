#include "MainWindow.h"

#include <QLabel>
#include <QSplitter>
#include <QStatusBar>

#include "p5ccs/core/Version.h"
#include "p5ccs/editor/CodeEditorWidget.h"
#include "p5ccs/engine/SketchViewportWidget.h"

namespace p5ccs::app {

MainWindow::MainWindow(QWidget* parent)
    : QMainWindow(parent)
{
    setWindowTitle(tr("Processing 5 - Creative Coding Station"));
    resize(1280, 800);

    auto* splitter = new QSplitter(Qt::Horizontal, this);
    splitter->addWidget(new editor::CodeEditorWidget(splitter));
    splitter->addWidget(new engine::SketchViewportWidget(splitter));
    splitter->setStretchFactor(0, 1);
    splitter->setStretchFactor(1, 1);
    setCentralWidget(splitter);

    auto* versionLabel = new QLabel(
        tr("Qt6 rewrite foundation — v%1").arg(core::Version::ToString()), this);
    statusBar()->addPermanentWidget(versionLabel);
    statusBar()->showMessage(tr("Ready"));
}

} // namespace p5ccs::app
