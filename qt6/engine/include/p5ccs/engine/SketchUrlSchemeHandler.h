#pragma once

#include <functional>

#include <QWebEngineUrlSchemeHandler>

namespace p5ccs::engine {

// Serves the sketch viewport's content (index.html, the embedded p5.js
// runtime, and the current sketch source) over a custom "p5ccs://" scheme
// instead of a loopback HTTP server. This is the Qt6 rewrite's equivalent
// of the legacy WPF app's LocalSketchServer, but with a stronger security
// property: there is no listening socket at all (not even on loopback),
// so there is nothing for a malicious sketch to discover or connect to
// beyond what QWebEngineUrlRequestInterceptor already sees.
//
// index.html and p5.min.js are compiled into the binary via Qt's resource
// system (see engine/CMakeLists.txt); sketch.js is produced on demand by
// calling sketchSourceProvider, mirroring the legacy server's
// Func<string> sketch source callback.
class SketchUrlSchemeHandler : public QWebEngineUrlSchemeHandler {
    Q_OBJECT

public:
    using SketchSourceProvider = std::function<QByteArray()>;

    static constexpr const char* SchemeName = "p5ccs";

    // Must be called exactly once, before QApplication is constructed —
    // QWebEngineUrlScheme::registerScheme() has that requirement.
    static void RegisterScheme();

    explicit SketchUrlSchemeHandler(SketchSourceProvider sketchSourceProvider, QObject* parent = nullptr);

    void requestStarted(QWebEngineUrlRequestJob* job) override;

private:
    SketchSourceProvider m_sketchSourceProvider;
};

} // namespace p5ccs::engine
