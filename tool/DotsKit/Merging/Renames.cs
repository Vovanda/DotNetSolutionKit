using DotsKit.Io;

namespace DotsKit.Merging;

/// <summary>The files the template moved between two of its outputs, by git's rename detection.</summary>
internal interface IRenames
{
    /// <summary>Each moved file, its new path to its old one, relative to the trees.</summary>
    Task<IReadOnlyDictionary<string, string>> FindAsync(string baseDir, string newDir, CancellationToken ct);
}

/// <summary>
/// <c>git diff --no-index -M</c> between the two trees: a file removed from one path and added at another with
/// mostly the same text is a move, so the team's changes to it follow it to the new path.
/// </summary>
internal sealed class GitRenames(IProcessRunner runner) : IRenames
{
    private const char Separator = '\0';
    private const char RenameStatus = 'R';

    /// <summary>git diff exits with 1 when the trees differ, which is the expected case.</summary>
    private const int Differ = 1;

    public async Task<IReadOnlyDictionary<string, string>> FindAsync(string baseDir, string newDir, CancellationToken ct)
    {
        var result = await runner.RunAsync("git", ["diff", "--no-index", "-M", "--name-status", "-z", baseDir, newDir], Path.GetTempPath(), ct);
        if (result.Code is not (0 or Differ))
            throw new InvalidOperationException($"git could not compare the template's two outputs: {result.Error.Trim()}");
        return Parse(result.Output, baseDir, newDir);
    }

    /// <summary>The renames of git's -z output: a status, then two paths for a rename, one for anything else.</summary>
    public static IReadOnlyDictionary<string, string> Parse(string output, string baseDir, string newDir)
    {
        var fields = output.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        var renames = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i < fields.Length; i++)
        {
            if (fields[i][0] != RenameStatus)
            {
                i++;
                continue;
            }
            renames[Relative(fields[i + 2], newDir)] = Relative(fields[i + 1], baseDir);
            i += 2;
        }
        return renames;
    }

    private static string Relative(string path, string root)
    {
        var normal = path.Replace('\\', '/');
        var prefix = root.Replace('\\', '/').TrimEnd('/') + "/";
        var at = normal.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        return at < 0 ? normal : normal[(at + prefix.Length)..];
    }
}
