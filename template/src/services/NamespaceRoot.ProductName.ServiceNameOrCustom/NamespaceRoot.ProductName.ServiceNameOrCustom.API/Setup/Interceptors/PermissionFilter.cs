using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using NamespaceRoot.ProductName.Common.Exceptions;
using NamespaceRoot.ProductName.Common.Web.Authorization;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Application.Services.Auth;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Interceptors;

/// <summary>
/// Authorization filter that checks for permissions specified via <see cref="RequiredPermissionsAttribute"/>.
/// Uses <see cref="IPermissionService"/> to verify that the current user has the required permissions.
/// </summary>
[UsedImplicitly]
internal sealed class PermissionAuthorizationFilter(IPermissionService permissionService) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var requiredPermissions = context.ActionDescriptor.EndpointMetadata
            .OfType<RequiredPermissionsAttribute>()
            .SelectMany(attr => attr.Permissions)
            .Distinct()
            .ToList();

        if (requiredPermissions.Count == 0) return;

        var hasAccess = await permissionService.UserHasAllPermissionsAsync(
            requiredPermissions, 
            context.HttpContext.RequestAborted);

        if (!hasAccess)
        {
            var missing = string.Join(", ", requiredPermissions);
    
            // Pass both the descriptive message and the specific error code
            throw new AccessDeniedException($"Missing permissions: {missing}", "INSUFFICIENT_PERMISSIONS");
        }
    }
}