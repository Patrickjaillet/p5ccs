#pragma once

#include <QMainWindow>

namespace p5ccs::app {

class MainWindow : public QMainWindow {
    Q_OBJECT

public:
    explicit MainWindow(QWidget* parent = nullptr);
};

} // namespace p5ccs::app
