#include <QTest>

#include "p5ccs/core/Version.h"

using p5ccs::core::Version;

class VersionTest : public QObject {
    Q_OBJECT

private slots:
    void toString_includesSuffixWhenPresent()
    {
        QString result = Version::ToString();
        QVERIFY(result.startsWith("2.0.0"));
        if (Version::Suffix && Version::Suffix[0] != '\0') {
            QVERIFY(result.contains(QString("-%1").arg(Version::Suffix)));
        }
    }
};

QTEST_MAIN(VersionTest)
#include "test_Version.moc"
