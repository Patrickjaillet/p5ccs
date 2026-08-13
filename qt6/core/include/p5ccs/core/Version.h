#pragma once

#include <QString>

namespace p5ccs::core {

// Single source of truth for the Qt6 rewrite's version, analogous to the
// legacy WPF app's Directory.Build.props. Kept separate from the legacy
// solution's own versioning (this rewrite starts its own v2.0.0 line).
struct Version {
    static constexpr int Major = 2;
    static constexpr int Minor = 0;
    static constexpr int Patch = 0;
    static constexpr const char* Suffix = "dev";

    static QString ToString();
};

} // namespace p5ccs::core
