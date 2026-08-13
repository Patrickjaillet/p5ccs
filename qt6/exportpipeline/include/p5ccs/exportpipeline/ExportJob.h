#pragma once

#include <QString>

namespace p5ccs::exportpipeline {

enum class ExportFormat {
    Png,
    Jpeg,
    Gif,
    WebM,
    Mp4,
};

// Parameters for a single export job. This is data only — no FFmpeg
// invocation, no encoder selection/fallback logic, no frame capture yet.
// The legacy C# VideoExporter's GPU-first-then-CPU-fallback encoder chain
// and ExportJobRunner's queue processing are not ported in this
// foundation pass; see qt6/docs/QT6-MIGRATION.md.
struct ExportJob {
    ExportFormat format = ExportFormat::Png;
    int width = 800;
    int height = 450;
    double durationSeconds = 2.0;
    int frameRate = 30;
    QString outputPath;
};

QString ToString(ExportFormat format);

} // namespace p5ccs::exportpipeline
