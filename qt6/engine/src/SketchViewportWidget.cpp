#include "p5ccs/engine/SketchViewportWidget.h"
#include "p5ccs/engine/SketchUrlSchemeHandler.h"

#include <QVBoxLayout>
#include <QWebEngineProfile>
#include <QWebEngineView>

namespace p5ccs::engine {

namespace {
constexpr const char* DefaultSketch = R"JS(
function setup() {
  createCanvas(400, 400);
}

function draw() {
  background(30);
  noStroke();
  fill(120, 170, 255);
  const t = millis() / 1000;
  const x = width / 2 + Math.cos(t) * 100;
  const y = height / 2 + Math.sin(t) * 100;
  circle(x, y, 60);
}
)JS";
}

SketchViewportWidget::SketchViewportWidget(QWidget* parent)
    : QWidget(parent)
    , m_sketchSource(QString::fromUtf8(DefaultSketch))
{
    auto* profile = QWebEngineProfile::defaultProfile();
    // Ownership: Qt requires the scheme handler to outlive the profile's
    // use of it. Parenting it to the profile ties its lifetime to the
    // (application-lifetime) default profile, mirroring how the legacy
    // LocalSketchServer lived for the lifetime of its SketchViewport.
    auto* handler = new SketchUrlSchemeHandler(
        [this]() { return CurrentSketchSource(); }, profile);
    profile->installUrlSchemeHandler(QByteArray(SketchUrlSchemeHandler::SchemeName), handler);

    m_view = new QWebEngineView(this);
    auto* layout = new QVBoxLayout(this);
    layout->setContentsMargins(0, 0, 0, 0);
    layout->addWidget(m_view);

    m_view->setUrl(QUrl("p5ccs://viewport/index.html"));
}

void SketchViewportWidget::SetSketchSource(const QString& source)
{
    m_sketchSource = source;
    m_view->reload();
}

QByteArray SketchViewportWidget::CurrentSketchSource() const
{
    return m_sketchSource.toUtf8();
}

} // namespace p5ccs::engine
