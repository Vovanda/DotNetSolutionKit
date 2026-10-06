namespace DotsKit;

/// <summary>The tool's own options; every other argument goes to the template as it is.</summary>
internal sealed class Options
{
    public const string Usage = """
        dotskit - makes a solution with DotNetSolutionKit and adds services and modules to it, keeping your changes.

          dotskit new -N MyCompany -P MyProduct -S Orders   in a folder with no solution: make one with this service
          dotskit new -S Billing --Storage true             in a solution: add a service; it takes the solution's names
          dotskit new -S Orders --MongoDB true              in a solution: add a flag to a service it has
          dotskit init                                      in a solution made without dotskit: describe it in its manifest

        Every command shows what it will change and asks before writing.
          --yes                     write without asking (CI)
          --force                   in a conflict take the template's side; your lines stay in git history
          --no-build                do not build the solution after writing
          --allow-dirty             run on a working tree with uncommitted changes
          --template-version 2.x.y  init: the version a solution was made by, where it does not say
        """;

    private const string YesOption = "--yes";
    private const string ForceOption = "--force";
    private const string NoBuildOption = "--no-build";
    private const string AllowDirtyOption = "--allow-dirty";
    private const string TemplateVersionOption = "--template-version";

    public bool Yes { get; private init; }
    public bool Force { get; private init; }
    public bool NoBuild { get; private init; }
    public bool AllowDirty { get; private init; }
    public string? TemplateVersion { get; private init; }
    public List<string> TemplateArgs { get; private init; } = [];

    /// <summary>The options of <paramref name="command"/>, refusing one that belongs to another command.</summary>
    public static Options Parse(string command, IReadOnlyList<string> args)
    {
        var options = Parse(args);
        return options.TemplateVersion is null || command == "init"
            ? options
            : throw new ArgumentException($"{TemplateVersionOption} is an option of dotskit init.");
    }

    public static Options Parse(IReadOnlyList<string> args)
    {
        string[] flags = [YesOption, ForceOption, NoBuildOption, AllowDirtyOption];
        var at = args.ToList().FindIndex(a => Is(a, TemplateVersionOption));
        if (at >= 0 && at + 1 >= args.Count)
            throw new ArgumentException($"{TemplateVersionOption} needs a version: {TemplateVersionOption} 2.7.0.");
        var rest = at < 0 ? args : [.. args.Take(at), .. args.Skip(at + 2)];
        return new Options
        {
            Yes = rest.Any(a => Is(a, YesOption)),
            Force = rest.Any(a => Is(a, ForceOption)),
            NoBuild = rest.Any(a => Is(a, NoBuildOption)),
            AllowDirty = rest.Any(a => Is(a, AllowDirtyOption)),
            TemplateVersion = at < 0 ? null : args[at + 1],
            TemplateArgs = [.. rest.Where(a => !flags.Any(f => Is(a, f)))],
        };
    }

    private static bool Is(string arg, string option) => string.Equals(arg, option, StringComparison.OrdinalIgnoreCase);
}
