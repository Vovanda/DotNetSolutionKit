using System.Reflection;
using NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- Настройка логирования ---
    builder.SetupLogging();
    
    // --- Конфигурация проверки DI ---
    builder.Host.UseDefaultServiceProvider((context, options) =>
    {
        var isLocal = context.HostingEnvironment.IsEnvironment("Local");
        options.ValidateScopes = isLocal;    // Scoped не резолвятся через root
        options.ValidateOnBuild = isLocal;   // Проверка, что все зависимости могут быть построены
    });

    // --- Конфигурация приложения ---
    builder.Configuration.SetupAppConfiguration(builder.Environment);

    // --- Подключение сервисов приложения ---
    builder.SetupWebApi()
        .SetupAppServices()
        .SetupApplicationAuthentication()
        .SetupApplicationAuthorization()
        .SetupHealthChecks()
        .SetupSwaggerPage()
        .SetupValidation();

    var app = builder.Build();

    // --- Middleware ---
    app.UseLogging()
        .UseWebServer()
        .UseAppAuthorization()
        .UseSwaggerPage()
        .UseWebApi();

    // --- Health checks ---
    app.MapHealthChecks("/health");

    // --- Запуск приложения ---
    app.Run();
}
catch (HostAbortedException ex)
{
    Log.Warning(ex, "Host was aborted. This may be expected in some environments.");
}
catch (Exception ex)
{
    Log.Fatal(ex, "The {EntryAssemblyName} application startup failed", Assembly.GetEntryAssembly()?.GetName().Name);
}
finally
{
    Log.Information("The {EntryAssemblyName} application was stopped", Assembly.GetEntryAssembly()?.GetName().Name);
    Log.CloseAndFlush();
}