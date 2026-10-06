namespace DotsKit;

/// <summary>The tool's own options; every other argument goes to the template as it is.</summary>
internal sealed class Options
{
    public const string Usage = """
        dotskit - makes a solution with DotNetSolutionKit and adds services and modules to it, keeping your changes.

          dotskit new -N MyCompany -P MyProduct -S Orders   in a folder with no solution: make one with this service
          dotskit new -S Billing --Storage true             in a solution: add a service; it takes the solution's names
          dotskit new -S Orders --MongoDB true              in a solution: add a flag to a service it has

        Every command shows what it will change and asks before writing.
          --yes                     write without asking (CI)
          --force                   in a conflict take the template's side; your lines stay in git history
          --no-build                do not build the solution after writing
          --allow-dirty             run on a working tree with uncommitted changes
        """;

    private const string YesOption = "--yes";
    private const string ForceOption = "--force";
    private const string NoBuildOption = "--no-build";
    private const string AllowDirtyOption = "--allow-dirty";

    public bool Yes { get; private init; }
    public bool Force { get; private init; }
    public bool NoBuild { get; private init; }
    public bool AllowDirty { get; private init; }
    public List<string> TemplateArgs { get; private init; } = [];

    public static Options Parse(IReadOnlyList<string> args)
    {
        string[] flags = [YesOption, ForceOption, NoBuildOption, AllowDirtyOption];
        return new Options
        {
            Yes = args.Any(a => Is(a, YesOption)),
            Force = args.Any(a => Is(a, ForceOption)),
            NoBuild = args.Any(a => Is(a, NoBuildOption)),
            AllowDirty = args.Any(a => Is(a, AllowDirtyOption)),
            TemplateArgs = [.. args.Where(a => !flags.Any(f => Is(a, f)))],
        };
    }

    private static bool Is(string arg, string option) => string.Equals(arg, option, StringComparison.OrdinalIgnoreCase);
}
