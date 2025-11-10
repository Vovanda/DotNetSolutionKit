namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Application.Services.Auth;

/// <summary>
/// Service for checking permissions of the currently authenticated user.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Checks if the current user has all of the specified permissions.
    /// </summary>
    /// <param name="allRequiredPermissions">List of permission codes to check.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> UserHasAllPermissionsAsync(IEnumerable<string> allRequiredPermissions, CancellationToken cancellationToken);
}