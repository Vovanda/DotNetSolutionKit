using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Swagger;

/// <summary>
/// Helper utility for discovering API versions from application controllers
/// </summary>
internal static partial class ApiVersionHelper
{
    /// <summary>
    /// Discovers all unique API versions from application controllers
    /// </summary>
    /// <returns>Collection of unique version strings</returns>
    public static IEnumerable<string> DiscoverAllVersions()
    {
        var versions = new HashSet<string>();
        
        // Get all controller types from current assembly
        var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsSubclassOf(typeof(ControllerBase)));
        
        foreach (var controllerType in controllerTypes)
        {
            var routeAttribute = controllerType.GetCustomAttribute<RouteAttribute>();
            if (routeAttribute?.Template == null) continue;
            var version = ExtractVersionFromRoute(routeAttribute.Template);
            versions.Add(version);
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