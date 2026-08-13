#pragma once

#include <QWidget>

class QWebEngineView;

namespace p5ccs::engine {

class SketchUrlSchemeHandler;

// Hosts the p5.js sketch viewport via QWebEngineView, loading content
// through the custom "p5ccs://" scheme served by SketchUrlSchemeHandler
// (see that class for why this replaces a loopback HTTP server). This is
// the Qt6 rewrite's equivalent of the legacy WPF app's SketchViewport.
//
// Still missing versus the legacy viewport: FPS/console/mouse reporting
// back to the host (the old bridge.js -> WebMessageReceived channel),
// export frame capture, and the WebResourceRequested-based network
// isolation for anything the sketch itself tries to fetch (the custom
// scheme here has no network access by construction, which covers the
// same goal for top-level navigation, but per-request interception for
// sketch-issued fetch() calls hasn't been ported yet).
class SketchViewportWidget : public QWidget {
    Q_OBJECT

public:
    explicit SketchViewportWidget(QWidget* parent = nullptr);

    void SetSketchSource(const QString& source);

private:
    QByteArray CurrentSketchSource() const;

    QWebEngineView* m_view = nullptr;
    QString m_sketchSource;
};

} // namespace p5ccs::engine
