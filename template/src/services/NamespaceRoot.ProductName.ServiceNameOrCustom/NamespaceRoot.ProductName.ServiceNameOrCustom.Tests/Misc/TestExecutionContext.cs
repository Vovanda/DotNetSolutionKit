using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Tests.Misc;

public class TestExecutionContext : IDisposable
{
    private ServiceProvider? _serviceProvider;

    public IServiceCollection Services { get; } = new ServiceCollection();
    
    public async Task<TResult> ExecuteAsync<TService, TResult>(Func<TService, Task<TResult>> execute) where TService : notnull
    {
        TResult result = default!;
        await ExecuteAsync<TService>(async service => result = await execute(service));
        return result;
    }

    public async Task ExecuteAsync<TService>(Func<TService, Task> execute) where TService : notnull
    {
        _serviceProvider ??= Services.BuildServiceProvider();
        await using var scope = _serviceProvider.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        await execute(service);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Контекст с поддержкой DbContext и операций над БД.
/// </summary>
public class TestExecutionContext<TDbContext> : TestExecutionContext where TDbContext : DbContext
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
/// Контекст с сервисом и DbContext.
/// </summary>
public class TestExecutionContext<TService, TDbContext> : TestExecutionContext<TDbContext>
    where TService : class
    where TDbContext : DbContext
{
    public TestExecutionContext() => Services.AddScoped<TService>();

    public Task ActAsync(Func<TService, Task> act) => ExecuteAsync(act);
    public Task<TResult> ActAsync<TResult>(Func<TService, Task<TResult>> act) => ExecuteAsync(act);
}

/// <summary>
/// InMemory вариант для юнит-тестов.
/// </summary>
public class InMemoryTestExecutionContext<TService, TDbContext> : TestExecutionContext<TService, TDbContext>
    where TService : class
    where TDbContext : DbContext
{
    public InMemoryTestExecutionContext() =>
        Services.AddDbContext<TDbContext>(options => options.UseInMemoryDatabase("TestDb" + Guid.NewGuid()));

    public override Task EnsureDatabaseCreatedAsync() => Task.CompletedTask;
    public override Task EnsureDatabaseDeletedAsync() => Task.CompletedTask;
}