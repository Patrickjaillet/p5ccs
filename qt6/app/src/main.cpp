#include <QApplication>

#include "MainWindow.h"

int main(int argc, char* argv[])
{
    QApplication app(argc, argv);
    QApplication::setApplicationName("Processing 5 - Creative Coding Station");
    QApplication::setOrganizationName("Patrick JAILLET");

    p5ccs::app::MainWindow window;
    window.show();

    return QApplication::exec();
}
