using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NamespaceRoot.ProductName.Common.Contracts.Domain.Context;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Security;

public static class DependencyInjection
{
    private static void AddUserContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext>(sp =>
        {
            var http = sp.GetRequiredService<IHttpContextAccessor>();
            return new HttpUserContext(http);
        });
    }
    
    public static IServiceCollection AddExecutionContext(this IServiceCollection services)
    {
        services.AddUserContext();
        services.AddScoped<IDomainExecutionContext>(sp =>
        {
            var actor = sp.GetRequiredService<IUserContext>();
            var timeProvider = sp.GetRequiredService<TimeProvider>();
            
            return new ApplicationExecutionContext(actor, timeProvider);
        });

        return services;
    }
}