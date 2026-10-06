using DotsKit.Generation;
using DotsKit.Io;
using DotsKit.Planning;
using DotsKit.Solutions;

namespace DotsKit.Commands;

/// <summary>
/// <c>dotskit new -S &lt;Service&gt; [flags]</c>: no solution around - makes one with this service; a solution -
/// adds the service, or joins the flags to a service it has. Whether to make or to add is read from the
/// folder; <c>--Solution</c> changes nothing.
/// </summary>
internal sealed class NewCommand(IPlanner planner, PlanReport report, IPlanWriter writer, IGit git, IBuilder builder, ITerminal terminal, string toolVersion)
{
    public async Task<ExitCode> RunAsync(Options options, string folder, CancellationToken ct)
    {
        var command = TemplateArgs.Parse(options.TemplateArgs);
        if (!command.HasService)
            throw new ArgumentException("Name the service with -S.");
        var root = SolutionRoot.Find(folder);
        if (root is null)
            await RefuseFilesOutsideGitAsync(folder, options, ct);
        var current = root is null ? null : await LoadCheckedAsync(root, options, ct);
        var next = Next(current, command);
        var plan = await planner.BuildAsync(root ?? folder, current, next, options.Force, ct);
        return await ConfirmAndWriteAsync(plan, options, ct);
    }

    /// <summary>
    /// A new solution goes into an empty folder, or beside files a clean git tree holds: what the tool writes over
    /// them must be possible to see and undo.
    /// </summary>
    private async Task RefuseFilesOutsideGitAsync(string folder, Options options, CancellationToken ct)
    {
        if (!SolutionRoot.IsEmpty(folder) && !options.AllowDirty && !await git.IsCleanAsync(folder, ct))
            throw new InvalidOperationException("The folder has files outside a clean git tree: make the solution in an empty folder, or commit what is here first (--allow-dirty to go on).");
    }

    /// <summary>The manifest of a solution to add to: there, of this version, and the tree clean.</summary>
    private async Task<Manifest> LoadCheckedAsync(string root, Options options, CancellationToken ct)
    {
        var manifest = Manifest.Load(root)
            ?? throw new InvalidOperationException($"{Layout.ManifestPath} is missing: dotskit works with a solution it made; one made with the template alone gets described by dotskit init, which comes in a later version.");
        var version = TemplateSource.Current(toolVersion);
        if (manifest.Template != version)
            throw new InvalidOperationException($"The solution is of the template {manifest.Template}, dotskit is {version}: use the dotskit of the solution, dotnet tool update -g SawKing.DotsKit.Tool --version {manifest.Template}.");
        if (!options.AllowDirty && !await git.IsCleanAsync(root, ct))
            throw new InvalidOperationException("The git working tree has uncommitted changes: commit or stash them, so what dotskit writes can be seen and undone (--allow-dirty to go on).");
        return manifest;
    }

    /// <summary>The manifest with the command's service: a new solution's first, or one more of the solution's.</summary>
    private Manifest Next(Manifest? current, TemplateArgs command)
    {
        var service = Ports.Assign(command.ServicePart(), current);
        if (current is null)
            return new Manifest { Template = TemplateSource.Current(toolVersion), Solution = command.SolutionPart().ToArgs(), Services = [] }.WithService(service);
        RefuseOtherSolutionValues(current, command);
        return current.WithService(service);
    }

    /// <summary>A service takes the names, the database and the rest from the solution; a different one would break it.</summary>
    private static void RefuseOtherSolutionValues(Manifest current, TemplateArgs command)
    {
        var different = current.OtherSolutionValues(command);
        if (different.Count > 0)
            throw new ArgumentException($"The solution has its own {string.Join(", ", different.Select(n => $"{n} ({current.SolutionArgs[n]})"))}: a service takes them from it, leave them out of the command.");
    }

    private async Task<ExitCode> ConfirmAndWriteAsync(Plan plan, Options options, CancellationToken ct)
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
        terminal.Info(build.Code == 0 ? "Written; the solution builds." : $"Written; the build fails:\n{build.Output}");
        return build.Code == 0 ? ExitCode.Done : ExitCode.Failed;
    }
}
