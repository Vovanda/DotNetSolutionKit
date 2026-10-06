using DotsKit.Io;
using DotsKit.Merging;
using DotsKit.Solutions;

namespace DotsKit.Planning;

/// <summary>
/// Writes a plan in one transaction: the files, the manifest and the projects of All.sln all land, or none does.
/// </summary>
internal interface IPlanWriter
{
    Task WriteAsync(Plan plan, CancellationToken ct);
}

internal sealed class PlanWriter(ISolutionProjects projects) : IPlanWriter
{
    public Task WriteAsync(Plan plan, CancellationToken ct)
    {
        var transaction = new FileTransaction(plan.Root);
        foreach (var change in plan.Changes)
            Add(transaction, change);
        transaction.Write(Layout.ManifestPath, System.Text.Encoding.UTF8.GetBytes(plan.Next.Serialize()));
        if (!plan.Membership.IsEmpty)
            transaction.Guard(Path.GetRelativePath(plan.Root, Layout.SolutionFileOf(plan.Root)));
        return transaction.CommitAsync(() => ApplyMembershipAsync(plan, ct));
    }

    /// <summary>A conflict with no content - a file one side removed and the other changed - stays as it is.</summary>
    private static void Add(FileTransaction transaction, Change change)
    {
        if (change.Kind == ChangeKind.Delete)
            transaction.Delete(change.Path);
        else if (change.Content is not null)
            transaction.Write(change.Path, change.Content.ToBytes());
    }

    private async Task ApplyMembershipAsync(Plan plan, CancellationToken ct)
    {
        if (plan.Membership.IsEmpty)
            return;
        var solutionFile = Layout.SolutionFileOf(plan.Root);
        foreach (var project in plan.Membership.Add)
            await projects.AddAsync(solutionFile, project, ct);
        foreach (var project in plan.Membership.Remove)
            await projects.RemoveAsync(solutionFile, project, ct);
    }
}
