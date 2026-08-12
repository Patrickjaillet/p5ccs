using System.IO;

namespace P5CCS.Engine;

public static class WebView2RuntimeLocator
{
    private const string FixedVersionFolderName = "WebView2Runtime";

    /// <summary>
    /// Path to a bundled WebView2 Fixed Version Runtime folder next to the app executable,
    /// if one is present (deployed by the installer). When absent, the app falls back to
    /// the system's Evergreen WebView2 Runtime (the WebView2 control's default behavior).
    /// </summary>
    public static string? FixedVersionRuntimePath
    {
        get
        {
            var candidate = Path.Combine(AppContext.BaseDirectory, FixedVersionFolderName);
            return Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "msedgewebview2.exe"))
                ? candidate
                : null;
        }
    }
}
