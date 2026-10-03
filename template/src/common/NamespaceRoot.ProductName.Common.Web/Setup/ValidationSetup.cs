using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

namespace NamespaceRoot.ProductName.Common.Web.Setup;

/// <summary>
/// FluentValidation for controllers: every validator in the given assemblies runs before the action,
/// and an invalid request is answered with 400 and <c>ValidationProblemDetails</c>.
/// </summary>
public static class ValidationSetup
{
    /// <summary>
    /// Registers the validators found in <paramref name="assemblies"/> and validates action arguments
    /// with them automatically.
    /// </summary>
    /// <remarks>
    /// Validators are found by scanning rather than listed by hand: a validator that exists but is not
    /// registered fails silently, and the endpoint it guards simply stops rejecting bad input.
    /// </remarks>
    public static IServiceCollection AddValidation(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Stop each rule chain at its first failure. FluentValidation defaults to Continue, which keeps
        // running the remaining conditions of the same RuleFor after an earlier one failed, so a chain
        // like NotNull().Must(list => list.Count > 0) dereferences the null it just rejected and throws
        // out of the validator itself: a 500 instead of a validation error.
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

        services.AddValidatorsFromAssemblies(assemblies);
        services.AddFluentValidationAutoValidation();
        return services;
    }
}
