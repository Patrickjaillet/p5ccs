using System.Reflection;
using P5CCS.Core.Versioning;

namespace P5CCS.Core.Tests.Versioning;

public class VersionServiceTests
{
    [Fact]
    public void InformationalVersion_ReadsFromAssemblyAttribute()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var expected = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        var sut = new VersionService(assembly);

        Assert.Equal(expected, sut.InformationalVersion);
    }

    [Fact]
    public void AssemblyVersion_IsNotNull()
    {
        var sut = new VersionService(Assembly.GetExecutingAssembly());

        Assert.NotNull(sut.AssemblyVersion);
    }
}
