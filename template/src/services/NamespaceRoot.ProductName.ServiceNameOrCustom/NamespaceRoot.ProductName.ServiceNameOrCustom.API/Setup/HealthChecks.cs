using NamespaceRoot.ProductName.Common.Contracts.Health;
using NamespaceRoot.ProductName.Common.Web.Health;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class HealthChecks
{
    /// <summary>
    /// Registers the dependencies <c>/ready</c> checks. Another dependency joins readiness by being
    /// registered here with <see cref="HealthConstants.ReadyTag"/>.
    /// </summary>
    public static WebApplicationBuilder SetupHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
//#if (Hangfire)
            .AddHangfire(options => options.MinimumAvailableServers = 1, name: "hangfire", tags: [HealthConstants.ReadyTag])
//#endif
            .AddDbContextCheck<ServiceIdentifierDbContext>(name: "postgres", tags: [HealthConstants.ReadyTag]);
        return builder;
    }

    public static WebApplication MapHealthEndpoints(this WebApplication app) => app.MapPlatformHealth();
}
