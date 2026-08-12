using System.Text.Json;

namespace P5CCS.Core.Projects;

public sealed class ProjectService : IProjectService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    public P5ccsProject? CurrentProject { get; private set; }

    public string? CurrentProjectPath { get; private set; }

    public P5ccsProject CreateNew(string name)
    {
        CurrentProject = new P5ccsProject { Name = name };
        CurrentProjectPath = null;
        return CurrentProject;
    }

    public P5ccsProject Open(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var project = JsonSerializer.Deserialize<P5ccsProject>(json, SerializerOptions)
                      ?? throw new InvalidDataException($"Unable to parse project file '{filePath}'.");

        CurrentProject = project;
        CurrentProjectPath = filePath;
        return project;
    }

    public void Save(string filePath)
    {
        if (CurrentProject is null)
        {
            throw new InvalidOperationException("No project is currently loaded.");
        }

        CurrentProject.ModifiedUtc = DateTimeOffset.UtcNow;

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(CurrentProject, SerializerOptions);
        File.WriteAllText(filePath, json);

        CurrentProjectPath = filePath;
    }

    public void SaveCurrent()
    {
        if (CurrentProjectPath is null)
        {
            throw new InvalidOperationException("The current project has not been saved to a file yet.");
        }

        Save(CurrentProjectPath);
    }
}
