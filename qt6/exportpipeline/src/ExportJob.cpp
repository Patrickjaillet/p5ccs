#include "p5ccs/exportpipeline/ExportJob.h"

namespace p5ccs::exportpipeline {

QString ToString(ExportFormat format)
{
    switch (format) {
    case ExportFormat::Png:
        return QStringLiteral("PNG");
    case ExportFormat::Jpeg:
        return QStringLiteral("JPEG");
    case ExportFormat::Gif:
        return QStringLiteral("GIF");
    case ExportFormat::WebM:
        return QStringLiteral("WebM");
    case ExportFormat::Mp4:
        return QStringLiteral("MP4");
    }
    return QStringLiteral("Unknown");
}

} // namespace p5ccs::exportpipeline
