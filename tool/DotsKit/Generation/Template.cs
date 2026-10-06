using DotsKit.Io;

namespace DotsKit.Generation;

/// <summary>Generates with one version of the template.</summary>
internal interface ITemplate
{
    /// <summary>Runs the template in <paramref name="root"/>, its post-actions with it.</summary>
    Task GenerateAsync(TemplateArgs args, string root, CancellationToken ct);
}

/// <summary>Makes the template of a version; the version a manifest names may be older than the tool.</summary>
internal interface ITemplateSource
{
    ITemplate Of(string version);
}

/// <summary>
/// The template installed in a hive of the tool's own, so the templates the user installed stay as they are.
/// The package comes from nuget.org by version; a folder of the template's sources stands in for it when the
/// tool runs against the template being developed (<see cref="TemplateSource.SourcesFolder"/>).
/// </summary>
internal sealed class DotnetNewTemplate(string package, IProcessRunner runner) : ITemplate
{
    private const string ShortName = "DotNetSolutionKit";

    private readonly string _hive = Folders.NewTemp("hive");
    private bool _installed;

    public async Task GenerateAsync(TemplateArgs args, string root, CancellationToken ct)
    {
        await InstallOnceAsync(ct);
        Directory.CreateDirectory(root);
        // --allow-scripts yes: the post-action that adds a service to All.sln runs a command.
        List<string> command = ["new", ShortName, .. args.ToArgs(), "--output", root, "--allow-scripts", "yes", "--debug:custom-hive", _hive];
        await runner.RunCheckedAsync("dotnet", command, root, ct);
    }

    private async Task InstallOnceAsync(CancellationToken ct)
    {
        if (_installed)
            return;
        await runner.RunCheckedAsync("dotnet", ["new", "install", package, "--force", "--debug:custom-hive", _hive], Path.GetTempPath(), ct);
        _installed = true;
    }
}

internal sealed class TemplateSource(IProcessRunner runner) : ITemplateSource
{
    private const string PackageId = "SawKing.DotNetSolutionKit";
    private const string SourcesVariable = "DOTSKIT_TEMPLATE_SOURCE";

    /// <summary>The version a manifest records when the tool ran against the template's sources.</summary>
    public const string SourcesVersion = "source";

    /// <summary>The folder of the template's sources, when DOTSKIT_TEMPLATE_SOURCE names one.</summary>
    public static string? SourcesFolder => Environment.GetEnvironmentVariable(SourcesVariable) is { Length: > 0 } folder ? folder : null;

    /// <summary>The version this run generates with: the tool's own, or the sources'.</summary>
    public static string Current(string toolVersion) => SourcesFolder is null ? toolVersion : SourcesVersion;

    private readonly Dictionary<string, ITemplate> _templates = new(StringComparer.Ordinal);

    public ITemplate Of(string version)
    {
        if (!_templates.TryGetValue(version, out var template))
            _templates[version] = template = new DotnetNewTemplate(PackageOf(version), runner);
        return template;
    }

    private static string PackageOf(string version) =>
        version == SourcesVersion
            ? SourcesFolder ?? throw new InvalidOperationException($"The solution was generated from the template's sources: set {SourcesVariable} to their folder.")
            : $"{PackageId}::{version}";
}
