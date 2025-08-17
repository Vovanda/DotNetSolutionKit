using Microsoft.Extensions.DependencyInjection;

namespace NamespaceRoot.ProductName.Common.DependencyInjection;

/// <summary>
/// Полезные расширения для DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// "Also Known As" — регистрирует сервис от имени другого типа на основе последней регистрации.
    /// </summary>
    public static IServiceCollection Aka<T>(this IServiceCollection services)
    {
        var lastDescriptor = services.LastOrDefault()
                             ?? throw new InvalidOperationException("Previous registration was not found");

        return services.AddAkaRegistration(lastDescriptor, typeof(T));
    }

    /// <summary>
    /// "Also Known As" — регистрирует сервис от имени другого типа напрямую через Type.
    /// </summary>
    public static IServiceCollection Aka(this IServiceCollection services, Type type)
    {
        var lastDescriptor = services.LastOrDefault()
                             ?? throw new InvalidOperationException("Previous registration was not found");

        return services.AddAkaRegistration(lastDescriptor, type);
    }

    /// <summary>
    /// "Also Known As" — регистрирует сервис от имени TResult, исходный тип TSource должен быть совместим.
    /// </summary>
    public static IServiceCollection Aka<TResult, TSource>(this IServiceCollection services) where TSource : TResult
    {
        var sourceDescriptor = services.LastOrDefault(d => d.ServiceType == typeof(TSource))
                               ?? throw new InvalidOperationException("Source registration was not found");

        return services.AddAkaRegistration(sourceDescriptor, typeof(TResult));
    }

    /// <summary>
    /// Реализация регистрации "Also Known As".
    /// </summary>
    private static IServiceCollection AddAkaRegistration(this IServiceCollection services,
        ServiceDescriptor sourceDescriptor, Type targetType)
    {
        ServiceDescriptor serviceDescriptor;

        if (sourceDescriptor.ImplementationInstance != null)
        {
            serviceDescriptor = new ServiceDescriptor(targetType, sourceDescriptor.ImplementationInstance);
        }
        else if (sourceDescriptor.ImplementationFactory != null)
        {
            serviceDescriptor = new ServiceDescriptor(targetType, sourceDescriptor.ImplementationFactory, sourceDescriptor.Lifetime);
        }
        else if (sourceDescriptor.ImplementationType != null)
        {
            serviceDescriptor = new ServiceDescriptor(targetType,
                isp => isp.GetRequiredService(sourceDescriptor.ImplementationType), sourceDescriptor.Lifetime);
        }
        else
        {
            throw new NotImplementedException($"Cannot create descriptor from source registration: {sourceDescriptor}");
        }

        services.Add(serviceDescriptor);
        return services;
    }
}