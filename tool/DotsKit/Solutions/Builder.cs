using DotsKit.Io;

namespace DotsKit.Solutions;

/// <summary>Builds a solution after the tool wrote it: a command ends with a solution that builds.</summary>
internal interface IBuilder
{
    Task<ProcessResult> BuildAsync(string root, CancellationToken ct);
}

internal sealed class DotnetBuilder(IProcessRunner runner) : IBuilder
{
    public Task<ProcessResult> BuildAsync(string root, CancellationToken ct) =>
        runner.RunAsync("dotnet", ["build", Layout.SolutionFileOf(root), "-nologo", "-v", "q", "--disable-build-servers"], root, ct);
}
