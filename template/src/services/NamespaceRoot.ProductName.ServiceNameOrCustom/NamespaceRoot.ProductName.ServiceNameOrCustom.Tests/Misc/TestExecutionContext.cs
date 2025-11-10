using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Tests.Misc;

public class TestExecutionContext : IDisposable
{
    private readonly Dictionary<Type, Func<object>> _instanceFactories = new();
    private readonly Dictionary<Type, object> _cachedInstances = new();
    
    private ServiceProvider? _serviceProvider;
    private bool _disposed;

    public IServiceCollection Services { get; } = new ServiceCollection();
    
    private ServiceProvider GetServiceProvider()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TestExecutionContext));
            
        return _serviceProvider ??= Services.BuildServiceProvider();
    }

    // Instance registration methods
    public void Register<TService, TImplementation>(Func<TImplementation>? factory = null) 
        where TService : class
        where TImplementation : class, TService
    {
        if (factory != null)
        {
            _instanceFactories[typeof(TService)] = factory;
            _instanceFactories[typeof(TImplementation)] = factory;
            Services.AddScoped<TService>(_ => GetOrCreateInstance<TService>());
            Services.AddScoped<TImplementation>(_ => GetOrCreateInstance<TImplementation>());
        }
        else
        {
            // For DI-created instances, let DI handle the creation
            Services.AddScoped<TService, TImplementation>();
            Services.AddScoped<TImplementation>();
        }
    }

    public void Register<TImplementation>(Func<TImplementation>? factory = null) where TImplementation : class
    {
        if (factory != null)
        {
            _instanceFactories[typeof(TImplementation)] = factory;
            Services.AddScoped(_ => GetOrCreateInstance<TImplementation>());
        }
        else
        {
            Services.AddScoped<TImplementation>();
        }
    }

    public void Register<TService, TImplementation>(TImplementation instance) 
        where TService : class
        where TImplementation : class, TService
    {
        _cachedInstances[typeof(TService)] = instance;
        _cachedInstances[typeof(TImplementation)] = instance;
        
        Services.AddScoped<TService>(_ => instance);
        Services.AddScoped<TImplementation>(_ => instance);
    }
    
    public void Register<TImplementation>(TImplementation instance) where TImplementation : class
    {
        _cachedInstances[typeof(TImplementation)] = instance;
        Services.AddScoped(_ => instance);
    }

    private T GetOrCreateInstance<T>() where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(TestExecutionContext));
        
        var type = typeof(T);
        
        if (_cachedInstances.TryGetValue(type, out var instance))
            return (T)instance;
        
        if (_instanceFactories.TryGetValue(type, out var factory))
            return (T)factory();
        
        return GetServiceProvider().GetRequiredService<T>();
    }

    public T GetRegistered<T>() where T : class
    {
        return GetOrCreateInstance<T>();
    }

    // Async methods
    public async Task<TResult> ExecuteAsync<TService, TResult>(
        Func<TService, Task<TResult>> execute,
        Action<IServiceProvider>? configure = null) where TService : notnull
    {
        var serviceProvider = GetServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        configure?.Invoke(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return await execute(service);
    }
    
    public async Task ExecuteAsync<TService>(
        Func<TService, Task> execute,
        Action<IServiceProvider>? configure = null) where TService : notnull
    {
        var serviceProvider = GetServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        configure?.Invoke(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        await execute(service);
    }

    // Sync methods
    public TResult Execute<TService, TResult>(
        Func<TService, TResult> execute,
        Action<IServiceProvider>? configure = null) where TService : notnull
    {
        var serviceProvider = GetServiceProvider();
        using var scope = serviceProvider.CreateScope();

        configure?.Invoke(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return execute(service);
    }

    public void Execute<TService>(
        Action<TService> execute,
        Action<IServiceProvider>? configure = null) where TService : notnull
    {
        var serviceProvider = GetServiceProvider();
        using var scope = serviceProvider.CreateScope();

        configure?.Invoke(scope.ServiceProvider);
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        execute(service);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _serviceProvider?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Context with DbContext support and database operations.
/// </summary>
public class DbTestExecutionContext<TDbContext> : TestExecutionContext where TDbContext : DbContext
{
    public async Task ArrangeAsync(params object[] entities) =>
        await ExecuteAsync<TDbContext>(async db => { db.AddRange(entities); await db.SaveChangesAsync(); });

    public virtual async Task EnsureDatabaseCreatedAsync() =>
        await ExecuteAsync<TDbContext>(db => db.Database.EnsureCreatedAsync());

    public virtual async Task EnsureDatabaseDeletedAsync() =>
        await ExecuteAsync<TDbContext>(db => db.Database.EnsureDeletedAsync());

    public Task AssertAsync(Func<TDbContext, Task> assert) => ExecuteAsync(assert);
}

/// <summary>
/// Context with service but without DbContext.
/// </summary>
public class ServiceTestExecutionContext<TService> : TestExecutionContext where TService : class
{
    public ServiceTestExecutionContext() => Services.AddScoped<TService>();

    // Async methods
    public Task ActAsync(Func<TService, Task> act, Action<IServiceProvider>? configure = null) =>
        ExecuteAsync(act, configure);

    public Task<TResult> ActAsync<TResult>(Func<TService, Task<TResult>> act, Action<IServiceProvider>? configure = null) =>
        ExecuteAsync(act, configure);

    // Sync methods
    public TResult Act<TResult>(Func<TService, TResult> act, Action<IServiceProvider>? configure = null) =>
        Execute(act, configure);

    public void Act(Action<TService> act, Action<IServiceProvider>? configure = null) =>
        Execute(act, configure);
}

/// <summary>
/// Context with service and DbContext.
/// </summary>
public class ServiceDbTestExecutionContext<TService, TDbContext> : DbTestExecutionContext<TDbContext>
    where TService : class
    where TDbContext : DbContext
{
    public ServiceDbTestExecutionContext() => Services.AddScoped<TService>();

    // Async methods
    public Task ActAsync(Func<TService, Task> act, Action<IServiceProvider>? configure = null) =>
        ExecuteAsync(act, configure);

    public Task<TResult> ActAsync<TResult>(Func<TService, Task<TResult>> act, Action<IServiceProvider>? configure = null) =>
        ExecuteAsync(act, configure);

    // Sync methods
    public TResult Act<TResult>(Func<TService, TResult> act, Action<IServiceProvider>? configure = null) =>
        Execute(act, configure);

    public void Act(Action<TService> act, Action<IServiceProvider>? configure = null) =>
        Execute(act, configure);
}

/// <summary>
/// InMemory variant for unit tests.
/// </summary>
public class InMemoryTestExecutionContext<TService, TDbContext> : ServiceDbTestExecutionContext<TService, TDbContext>
    where TService : class
    where TDbContext : DbContext
{
    public InMemoryTestExecutionContext() =>
        Services.AddDbContext<TDbContext>(options => options
            .UseInMemoryDatabase($"TestDb_{TestContext.CurrentContext.Test.ID}")
            .ConfigureWarnings(warnings => warnings
                .Ignore(InMemoryEventId.TransactionIgnoredWarning)));

    public override Task EnsureDatabaseCreatedAsync() => Task.CompletedTask;
    public override Task EnsureDatabaseDeletedAsync() => Task.CompletedTask;
}