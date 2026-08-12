namespace P5CCS.Core.Versioning;

public interface IVersionService
{
    string InformationalVersion { get; }

    Version AssemblyVersion { get; }
}
