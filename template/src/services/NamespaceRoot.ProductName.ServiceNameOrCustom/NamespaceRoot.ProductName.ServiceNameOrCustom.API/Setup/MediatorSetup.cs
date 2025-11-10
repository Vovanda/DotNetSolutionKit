using NamespaceRoot.ProductName.ServiceNameOrCustom.Application;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class MediatorSetup
{
    public static IServiceCollection AddAuthMediator(this IServiceCollection services)
    {
        // 1. Register MediatR
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationMarker).Assembly));
        /*

        //TODO (v,savkin):  Uncomment when implementing domain events
        // Requires: dotnet add package Scrutor
        // Also need to implement:
        // 1. IDomainEventDispatcher
        // 2. DomainEventDispatcher
        // 3. DbContextBase with event handling
        // 4. Handler classes implementing IDomainPreSaveHandler, IDomainPostCommitHandler, etc.

        // 2. Register domain event handlers via Scrutor
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(ApplicationMarker))
            .AddClasses(classes => classes.AssignableToAny(
                typeof(IDomainPreSaveHandler<>),
                typeof(IDomainPostCommitHandler<>),
                typeof(IDomainRollbackHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // 3. Register domain event dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        */
        return services;
    }
}