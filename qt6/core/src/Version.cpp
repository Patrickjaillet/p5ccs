#include "p5ccs/core/Version.h"

namespace p5ccs::core {

QString Version::ToString()
{
    QString base = QString("%1.%2.%3").arg(Major).arg(Minor).arg(Patch);
    if (Suffix && Suffix[0] != '\0') {
        base += QString("-%1").arg(Suffix);
    }
    return base;
}

} // namespace p5ccs::core
