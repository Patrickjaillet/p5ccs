using P5CCS.Core.Projects;

namespace P5CCS.Core.Tests.Projects;

public class ProjectServiceTests : IDisposable
{
    private readonly string _filePath;

    public ProjectServiceTests()
    {
        _filePath = Path.Combine(Path.GetTempPath(), $"p5ccs-project-{Guid.NewGuid():N}.p5ccsproj");
    }

    [Fact]
    public void CreateNew_SetsCurrentProjectWithGivenName()
    {
        var sut = new ProjectService();

        var project = sut.CreateNew("My Sketch");

        Assert.Equal("My Sketch", project.Name);
        Assert.Same(project, sut.CurrentProject);
        Assert.Null(sut.CurrentProjectPath);
    }

    [Fact]
    public void Save_ThenOpen_RoundTripsProject()
    {
        var sut = new ProjectService();
        sut.CreateNew("Round Trip");
        sut.Save(_filePath);

        var reopened = new ProjectService();
        var project = reopened.Open(_filePath);

        Assert.Equal("Round Trip", project.Name);
        Assert.Equal(_filePath, reopened.CurrentProjectPath);
    }

    [Fact]
    public void SaveCurrent_WithoutPriorPath_Throws()
    {
        var sut = new ProjectService();
        sut.CreateNew("Untitled");

        Assert.Throws<InvalidOperationException>(sut.SaveCurrent);
    }

    public void Dispose()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }
}
