using DotsKit.Io;

namespace DotsKit.Merging;

/// <summary>
/// The changes a solution needs to have what the template gives now, keeping what the solution changed: every
/// file of the base and the new tree goes through <see cref="MergeRule"/>, and a file both sides changed through
/// the text merger. Line endings are merged as LF and written back as the solution's copy has them.
/// </summary>
internal sealed class ThreeWayMerge(ITextMerger merger)
{
    /// <param name="baseDir">What the template gave before; null when the solution is new.</param>
    /// <param name="preferTemplate">--force: in a conflict take the template's side; the solution's lines stay in git history.</param>
    public async Task<List<Change>> ComputeAsync(string solution, string? baseDir, string newDir, bool preferTemplate, CancellationToken ct)
    {
        var paths = new SortedSet<string>(Folders.RelativeFiles(newDir), StringComparer.Ordinal);
        if (baseDir is not null)
            paths.UnionWith(Folders.RelativeFiles(baseDir));

        var changes = new List<Change>();
        foreach (var path in paths)
        {
            var decision = MergeRule.Decide(path, FileText.Read(baseDir, path), FileText.Read(newDir, path), FileText.Read(solution, path));
            var change = decision switch
            {
                MergeRule.Final final => final.Change,
                MergeRule.NeedsMerge merge => await MergeAsync(path, merge, preferTemplate, ct),
                _ => throw new InvalidOperationException($"Unknown decision for {path}"),
            };
            if (change is not null)
                changes.Add(change);
        }
        return changes;
    }

    private async Task<Change> MergeAsync(string path, MergeRule.NeedsMerge merge, bool preferTemplate, CancellationToken ct)
    {
        if (merge.Solution.IsBinary || merge.Template.IsBinary)
            return preferTemplate
                ? new(path, ChangeKind.Update, merge.Template, merge.Reason + "; binary, the template's taken")
                : new(path, ChangeKind.Kept, null, merge.Reason + "; binary, kept as it is");

        var result = await merger.MergeAsync(
            LineEndings.ToLf(merge.Solution.Text), LineEndings.ToLf(merge.Base.Text), LineEndings.ToLf(merge.Template.Text), preferTemplate, ct);
        if (result.Failure is not null)
            throw new InvalidOperationException($"git could not merge {path}: {result.Failure}. Nothing was written.");
        var content = merge.Solution with { Text = LineEndings.Apply(result.Text, LineEndings.Of(merge.Solution.Text)) };
        return result.Conflicts == 0
            ? new(path, ChangeKind.Merge, content, merge.Reason)
            : new(path, ChangeKind.Conflict, content, merge.Reason + $"; {result.Conflicts} conflict(s) marked");
    }
}
