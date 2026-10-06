using System.Text.RegularExpressions;

namespace DotsKit.Solutions;

/// <summary>The errors of a dotnet build, each once, under the project it is in: what a person looks at after a change.</summary>
internal static partial class BuildErrors
{
    private const string ProjectExtension = ".csproj";

    /// <summary>
    /// MSBuild's line: <c>file(line,col): error CODE: message [project]</c>. NuGet's names the project itself as
    /// the file, and the solution in the brackets.
    /// </summary>
    [GeneratedRegex(@"^\s*(?<file>.+?)(\(\d+,\d+\))?\s*: error (?<code>[A-Z]+\d+): (?<message>.*?)(\s+\[(?<project>[^\]]+)\])?\s*$")]
    private static partial Regex Error();

    /// <summary>The output as it is when it has no error line MSBuild writes - a restore or a tool that failed otherwise.</summary>
    public static IEnumerable<string> ByProject(string output)
    {
        var errors = output.Split('\n')
            .Select(l => Error().Match(l) is { Success: true } m ? (Project: ProjectOf(m), Text: Describe(m)) : default)
            .Where(e => e.Project is not null)
            .Distinct()
            .GroupBy(e => e.Project, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();
        if (errors.Count == 0)
            return [output.Trim()];
        return errors.SelectMany(g => (IEnumerable<string>)[$"  {g.Key}", .. g.Select(e => $"    {e.Text}")]);
    }

    /// <summary>The project in the brackets, or the file itself where it is the project, as NuGet writes it.</summary>
    private static string ProjectOf(Match error)
    {
        var bracket = error.Groups["project"].Value;
        var project = bracket.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase) ? bracket : error.Groups["file"].Value;
        return Path.GetFileNameWithoutExtension(project.Replace('\\', '/'));
    }

    /// <summary>The file without its folders, then the code and the message.</summary>
    private static string Describe(Match error)
    {
        var file = error.Groups["file"].Value.Replace('\\', '/');
        return $"{file[(file.LastIndexOf('/') + 1)..]}: {error.Groups["code"].Value} {error.Groups["message"].Value}";
    }
}
