using System.IO;
using System.Reflection;

namespace P5CCS.App.Export;

public static class ExportFileNaming
{
    public static string GenerateFileName(string sketchTitle, string extension)
    {
        var slug = Slugify(Path.GetFileNameWithoutExtension(sketchTitle));
        var version = GetProjectVersion();
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

        return $"{slug}_v{version}_{timestamp}.{extension.TrimStart('.')}";
    }

    private static string GetProjectVersion()
    {
        var informationalVersion = Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        return string.IsNullOrWhiteSpace(informationalVersion) ? "0.0.0" : informationalVersion;
    }

    private static string Slugify(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "sketch";
        }

        var chars = value.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = new string(chars);

        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }

        return slug.Trim('-').ToLowerInvariant() is { Length: > 0 } trimmed ? trimmed : "sketch";
    }
}
