using DotsKit.Io;

namespace DotsKit.Solutions;

/// <summary>
/// The projects of All.sln, through <c>dotnet sln</c>, never by editing its text: <c>dotnet sln add</c> gives a
/// project a new GUID each time, so two All.sln with the same projects differ as text. A service's projects go
/// into a solution folder named after the last segment of the service folder's name
/// (NamespaceRoot.ProductName.Sales.Orders -> Orders), Common's into Common - as the template's post-action does.
/// </summary>
internal interface ISolutionProjects
{
    Task<HashSet<string>> ListAsync(string solutionFile, CancellationToken ct);
    Task AddMissingAsync(string root, CancellationToken ct);
    Task RemoveMissingAsync(string root, CancellationToken ct);
    Task AddAsync(string solutionFile, string listedProject, CancellationToken ct);
    Task RemoveAsync(string solutionFile, string listedProject, CancellationToken ct);
}

internal sealed class SolutionProjects(IProcessRunner runner) : ISolutionProjects
{
    /// <summary>A service's projects sit one level down in its folder: src/services/&lt;service&gt;/&lt;project&gt;/*.csproj.</summary>
    private const int ProjectDepth = 1;

    private const string ProjectPattern = "*.csproj";

    public static string FolderOf(string serviceFolderName) => serviceFolderName[(serviceFolderName.LastIndexOf('.') + 1)..];

    /// <summary>The solution folder a project listed relative to All.sln belongs in.</summary>
    public static string FolderOfListed(string project)
    {
        var first = project.Split('/')[0];
        return first == ".." ? Layout.CommonSolutionFolder : FolderOf(first);
    }

    public async Task<HashSet<string>> ListAsync(string solutionFile, CancellationToken ct)
    {
        var listed = await runner.RunCheckedAsync("dotnet", ["sln", solutionFile, "list"], DirectoryOf(solutionFile), ct);
        return listed.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(l => l.EndsWith(Layout.ProjectExtension, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Replace('\\', '/'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Adds every service project of the solution All.sln does not list yet.</summary>
    public async Task AddMissingAsync(string root, CancellationToken ct)
    {
        var solutionFile = Layout.SolutionFileOf(root);
        var listed = await ListAsync(solutionFile, ct);
        foreach (var project in ServiceProjects(root).Where(p => !listed.Contains(Relative(solutionFile, p))))
            await AddAsync(solutionFile, Relative(solutionFile, project), ct);
    }

    /// <summary>Removes from All.sln every project whose file is gone, as when a service folder is generated again.</summary>
    public async Task RemoveMissingAsync(string root, CancellationToken ct)
    {
        var solutionFile = Layout.SolutionFileOf(root);
        foreach (var project in await ListAsync(solutionFile, ct))
            if (!File.Exists(Path.Combine(DirectoryOf(solutionFile), project)))
                await RemoveAsync(solutionFile, project, ct);
    }

    public Task AddAsync(string solutionFile, string listedProject, CancellationToken ct) =>
        runner.RunCheckedAsync("dotnet",
            ["sln", solutionFile, "add", listedProject, "--solution-folder", FolderOfListed(listedProject)], DirectoryOf(solutionFile), ct);

    public Task RemoveAsync(string solutionFile, string listedProject, CancellationToken ct) =>
        runner.RunCheckedAsync("dotnet", ["sln", solutionFile, "remove", listedProject], DirectoryOf(solutionFile), ct);

    private static IEnumerable<string> ServiceProjects(string root) =>
        Directory.GetDirectories(Layout.ServicesOf(root))
            .SelectMany(service => Directory.GetFiles(service, ProjectPattern,
                new EnumerationOptions { RecurseSubdirectories = true, MaxRecursionDepth = ProjectDepth }))
            .Order(StringComparer.Ordinal);

    private static string DirectoryOf(string solutionFile) => Path.GetDirectoryName(solutionFile)!;

    private static string Relative(string solutionFile, string project) =>
        Path.GetRelativePath(DirectoryOf(solutionFile), project).Replace('\\', '/');
}
