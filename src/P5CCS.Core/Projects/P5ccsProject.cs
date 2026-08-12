namespace P5CCS.Core.Projects;

public sealed class P5ccsProject
{
    public string Name { get; set; } = "Untitled Sketch";

    public string SchemaVersion { get; set; } = "1.0";

    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ModifiedUtc { get; set; } = DateTimeOffset.UtcNow;

    public string MainSketchFile { get; set; } = "sketch.js";

    public List<string> AssetFiles { get; set; } = new();
}
