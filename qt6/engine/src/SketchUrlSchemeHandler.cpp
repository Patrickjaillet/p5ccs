#include "p5ccs/engine/SketchUrlSchemeHandler.h"

#include <QBuffer>
#include <QFile>
#include <QWebEngineUrlRequestJob>
#include <QWebEngineUrlScheme>

namespace p5ccs::engine {

void SketchUrlSchemeHandler::RegisterScheme()
{
    QWebEngineUrlScheme scheme(SchemeName);
    scheme.setFlags(
        QWebEngineUrlScheme::SecureScheme
        | QWebEngineUrlScheme::CorsEnabled);
    QWebEngineUrlScheme::registerScheme(scheme);
}

SketchUrlSchemeHandler::SketchUrlSchemeHandler(SketchSourceProvider sketchSourceProvider, QObject* parent)
    : QWebEngineUrlSchemeHandler(parent)
    , m_sketchSourceProvider(std::move(sketchSourceProvider))
{
}

void SketchUrlSchemeHandler::requestStarted(QWebEngineUrlRequestJob* job)
{
    const QString path = job->requestUrl().path();

    if (path.isEmpty() || path == "/" || path == "/index.html") {
        auto* file = new QFile(":/p5ccs/engine/index.html", job);
        if (!file->open(QIODevice::ReadOnly)) {
            job->fail(QWebEngineUrlRequestJob::UrlNotFound);
            return;
        }
        job->reply("text/html", file);
        return;
    }

    if (path == "/p5.min.js") {
        auto* file = new QFile(":/p5ccs/engine/p5.min.js", job);
        if (!file->open(QIODevice::ReadOnly)) {
            job->fail(QWebEngineUrlRequestJob::UrlNotFound);
            return;
        }
        job->reply("text/javascript", file);
        return;
    }

    if (path == "/sketch.js") {
        auto* buffer = new QBuffer(job);
        buffer->setData(m_sketchSourceProvider ? m_sketchSourceProvider() : QByteArray());
        buffer->open(QIODevice::ReadOnly);
        job->reply("text/javascript", buffer);
        return;
    }

    job->fail(QWebEngineUrlRequestJob::UrlNotFound);
}

} // namespace p5ccs::engine
