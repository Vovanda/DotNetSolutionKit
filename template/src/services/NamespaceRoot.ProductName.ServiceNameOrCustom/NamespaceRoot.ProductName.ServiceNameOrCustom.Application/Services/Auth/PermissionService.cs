namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Application.Services.Auth;

/// <summary>
/// Service for checking permissions of the currently authenticated user.
/// </summary>
public class PermissionService : IPermissionService
{
    public Task<bool> UserHasAllPermissionsAsync(IEnumerable<string> allRequiredPermissions, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}