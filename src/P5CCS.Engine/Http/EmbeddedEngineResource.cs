using System.IO;
using System.Reflection;

namespace P5CCS.Engine.Http;

internal static class EmbeddedEngineResource
{
    public static byte[] ReadBytes(string logicalName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(logicalName)
            ?? throw new FileNotFoundException($"Embedded resource '{logicalName}' was not found.");
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    public static string ReadText(string logicalName)
        => System.Text.Encoding.UTF8.GetString(ReadBytes(logicalName));
}
