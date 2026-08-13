#pragma once

#include <QWidget>

namespace p5ccs::engine {

// Placeholder for the sketch viewport. In the legacy WPF app this hosted a
// WebView2 control pointed at a loopback-only local HTTP server serving
// p5.js + the user's sketch (see the old SketchViewport.xaml.cs and
// LocalSketchServer.cs). The Qt6 equivalent is expected to be
// QWebEngineView + a Qt-side local server, but the QtWebEngine module is
// not installed in this environment's Qt distribution yet (only
// Core/Gui/Widgets and a handful of others are present) — see
// qt6/docs/QT6-MIGRATION.md for what's verified vs. still open. This
// widget currently just proves the module links into the app; it does not
// yet run any p5.js content.
class SketchViewportWidget : public QWidget {
    Q_OBJECT

public:
    explicit SketchViewportWidget(QWidget* parent = nullptr);
};

} // namespace p5ccs::engine
