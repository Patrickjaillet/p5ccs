using System.IO;
using System.Reflection;

namespace P5CCS.App.Services;

public static class EmbeddedTextResource
{
    public static string Read(string logicalName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(logicalName)
            ?? throw new FileNotFoundException($"Embedded resource '{logicalName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
