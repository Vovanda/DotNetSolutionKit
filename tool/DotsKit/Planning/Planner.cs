using DotsKit.Generation;
using DotsKit.Io;
using DotsKit.Merging;
using DotsKit.Solutions;

namespace DotsKit.Planning;

/// <summary>
/// Builds a plan: what the template gave the solution (the base, from the current manifest), what it gives now
/// (from the next one), and the three-way merge of the two with the solution's own files.
/// </summary>
internal interface IPlanner
{
    Task<Plan> BuildAsync(string root, Manifest? current, Manifest next, bool preferTemplate, CancellationToken ct);
}

internal sealed class Planner(IStaging staging, ThreeWayMerge merge, ISolutionProjects projects) : IPlanner
{
    /// <param name="current">The solution's manifest; null when there is no solution yet, and the base is empty.</param>
    public async Task<Plan> BuildAsync(string root, Manifest? current, Manifest next, bool preferTemplate, CancellationToken ct)
    {
        var baseDir = current is null ? null : await staging.BuildAsync(current, ct);
        var nextDir = await staging.BuildAsync(next, ct);
        var changes = await merge.ComputeAsync(root, baseDir, nextDir, preferTemplate, ct);
        if (current is null)
            return new Plan(root, next, changes, new MembershipChange([], []));
        var membership = await MembershipAsync(root, baseDir!, nextDir, ct);
        return new Plan(root, next, [.. changes.Where(c => !IsSolutionFile(c.Path))], membership);
    }

    private async Task<MembershipChange> MembershipAsync(string root, string baseDir, string nextDir, CancellationToken ct) =>
        MembershipChange.Of(
            await projects.ListAsync(Layout.SolutionFileOf(root), ct),
            await projects.ListAsync(Layout.SolutionFileOf(baseDir), ct),
            await projects.ListAsync(Layout.SolutionFileOf(nextDir), ct));

    /// <summary>All.sln of an existing solution changes by its projects, never by its text.</summary>
    private static bool IsSolutionFile(string path) =>
        path.StartsWith(Layout.ServicesFolder + "/", StringComparison.Ordinal) && path.EndsWith(Layout.SolutionSuffix, StringComparison.Ordinal)
        && !path[(Layout.ServicesFolder.Length + 1)..].Contains('/');
}
