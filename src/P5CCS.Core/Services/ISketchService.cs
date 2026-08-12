namespace P5CCS.Core.Services;

public interface ISketchService
{
    string LoadSource(string sketchFilePath);

    void SaveSource(string sketchFilePath, string source);

    event EventHandler<string>? SourceChanged;
}
