namespace DotsKit.Io;

/// <summary>The git working tree of a solution.</summary>
internal interface IGit
{
    /// <summary>Whether the tree has no uncommitted change: what the tool writes can then be seen and undone in git.</summary>
    Task<bool> IsCleanAsync(string root, CancellationToken ct);
}

internal sealed class Git(IProcessRunner runner) : IGit
{
    public async Task<bool> IsCleanAsync(string root, CancellationToken ct)
    {
        var status = await runner.RunAsync("git", ["status", "--porcelain"], root, ct);
        // Not a repository: nothing to undo the tool's changes with, so the tree is not clean.
        return status.Code == 0 && status.Output.Trim().Length == 0;
    }
}
