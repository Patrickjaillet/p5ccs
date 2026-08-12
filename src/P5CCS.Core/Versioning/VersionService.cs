using System.Reflection;

namespace P5CCS.Core.Versioning;

public sealed class VersionService : IVersionService
{
    public VersionService()
        : this(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
    {
    }

    public VersionService(Assembly assembly)
    {
        InformationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0";

        AssemblyVersion = assembly.GetName().Version ?? new Version(0, 0, 0, 0);
    }

    public string InformationalVersion { get; }

    public Version AssemblyVersion { get; }
}
