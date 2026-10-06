using DotsKit.Io;
using DotsKit.Planning;
using DotsKit.Solutions;

namespace DotsKit.Commands;

/// <summary>
/// The end every command that changes a solution shares: show the plan, ask, write it in one transaction, then
/// build - or stop at the conflicts a person resolves first.
/// </summary>
internal sealed class PlanExecution(PlanReport report, IPlanWriter writer, IBuilder builder, ITerminal terminal)
{
    public async Task<ExitCode> RunAsync(Plan plan, Options options, CancellationToken ct)
    {
        report.Show(plan);
        if (!options.Yes && !terminal.Confirm("Write these changes?"))
        {
            terminal.Info("Nothing written: answer y to write, or pass --yes where no one can answer.");
            return ExitCode.Failed;
        }
        await writer.WriteAsync(plan, ct);
        return plan.HasConflicts ? Conflicts() : await BuildAsync(plan.Root, options, ct);
    }

    private ExitCode Conflicts()
    {
        terminal.Info("Written. Resolve the conflicts marked <<<<<<< solution / >>>>>>> template, then build.");
        return ExitCode.Conflicts;
    }

    private async Task<ExitCode> BuildAsync(string root, Options options, CancellationToken ct)
    {
        if (options.NoBuild)
            return ExitCode.Done;
        ProcessResult build;
        try
        {
            build = await builder.BuildAsync(root, ct);
        }
        catch (OperationCanceledException)
        {
            terminal.Info("Written; the build was cancelled: run dotnet build to check the solution.");
            return ExitCode.Failed;
        }
        if (build.Code == 0)
        {
            terminal.Info("Written; the solution builds.");
            return ExitCode.Done;
        }
        terminal.Info("Written; the build fails, by project:");
        foreach (var line in BuildErrors.ByProject(build.Output))
            terminal.Info(line);
        return ExitCode.Failed;
    }
}
