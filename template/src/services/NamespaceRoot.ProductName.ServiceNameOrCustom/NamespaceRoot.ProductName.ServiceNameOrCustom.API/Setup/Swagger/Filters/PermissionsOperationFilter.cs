using System.Reflection;
using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using NamespaceRoot.ProductName.Common.Web.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Swagger.Filters;

/// <summary>
/// Swagger-specific filter that adds permissions information from <see cref="RequiredPermissionsAttribute"/> to API documentation.
/// This filter is used ONLY for Swagger documentation generation and does not affect actual authorization.
/// </summary>
[UsedImplicitly]
internal class PermissionsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var permissionAttributes = context.MethodInfo
            .GetCustomAttributes<RequiredPermissionsAttribute>(true)
            .ToList();

        if (permissionAttributes.Count == 0) 
            return;

        var allPermissions = permissionAttributes
            .SelectMany(attr => attr.Permissions)
            .Distinct()
            .ToList();

        var permissionsText =
            $"\n\n---\n**Authorization Requirements** 🔐\n\n*Required permissions:* `{string.Join("`, `", allPermissions)}`\n";

        operation.Description = string.IsNullOrEmpty(operation.Description) 
            ? permissionsText 
            : operation.Description + permissionsText;
    }
}