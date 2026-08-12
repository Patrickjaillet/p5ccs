namespace P5CCS.Core.Projects;

public interface IProjectService
{
    const string ProjectFileExtension = ".p5ccsproj";

    P5ccsProject? CurrentProject { get; }

    string? CurrentProjectPath { get; }

    P5ccsProject CreateNew(string name);

    P5ccsProject Open(string filePath);

    void Save(string filePath);

    void SaveCurrent();
}
