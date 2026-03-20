using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace NamespaceRoot.ProductName.Common.Web.Swagger;

/// <summary>
/// Helper utility for discovering API versions from application controllers.
/// </summary>
public static partial class ApiVersionHelper
{
    /// <summary>
    /// Discovers all unique API versions from controllers in the given assembly.
    /// </summary>
    public static IEnumerable<string> DiscoverAllVersions(Assembly assembly)
    {
        var versions = new HashSet<string>();

        var controllerTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(ControllerBase)));

        foreach (var controllerType in controllerTypes)
        {
            var routeAttribute = controllerType.GetCustomAttribute<RouteAttribute>();
            if (routeAttribute?.Template == null) continue;
            versions.Add(ExtractVersionFromRoute(routeAttribute.Template));
        }

        return versions.OrderBy(v => v);
    }

    public static string ExtractVersionFromRoute(string route)
    {
        if (string.IsNullOrEmpty(route))
            return "v1";

        var match = ApiVersionRouteRegex().Match(route);
        return match is { Success: true, Groups.Count: > 1 }
            ? $"v{match.Groups[1].Value}"
            : "v1";
    }

    [GeneratedRegex(@"/api/v(\d+)")]
    private static partial Regex ApiVersionRouteRegex();
}
