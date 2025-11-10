using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Swagger.Filters;

/// <summary>
/// Custom document filter to properly separate endpoints by version
/// </summary>
[UsedImplicitly]
internal class VersionedDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Skip filtering for "all" document
        var version = swaggerDoc.Info.Version;
        if (version == "all") return;
        
        var pathsToRemove = new List<string>();

        foreach (var path in swaggerDoc.Paths)
        {
            var pathVersion = ApiVersionHelper.ExtractVersionFromRoute(path.Key);
            if (pathVersion != version)
            {
                pathsToRemove.Add(path.Key);
            }
        }

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }
    }
}