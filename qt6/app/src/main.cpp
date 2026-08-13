#include <QApplication>

#include "MainWindow.h"
#include "p5ccs/engine/SketchUrlSchemeHandler.h"

int main(int argc, char* argv[])
{
    // Must happen before QApplication is constructed, per
    // QWebEngineUrlScheme::registerScheme()'s documented requirement.
    p5ccs::engine::SketchUrlSchemeHandler::RegisterScheme();

    QApplication app(argc, argv);
    QApplication::setApplicationName("Processing 5 - Creative Coding Station");
    QApplication::setOrganizationName("Patrick JAILLET");

    p5ccs::app::MainWindow window;
    window.show();

    return QApplication::exec();
}
