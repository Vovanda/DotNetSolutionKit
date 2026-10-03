using System.Reflection;
using NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;
using NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup.Swagger;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework.DataSeeding;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- Logging configuration ---
    builder.SetupLogging();
    
    // --- DI validation configuration ---
    builder.Host.UseDefaultServiceProvider((context, options) =>
    {
        var isLocal = context.HostingEnvironment.IsEnvironment("Local");
        options.ValidateScopes = isLocal;    // Scoped services don't resolve via root
        options.ValidateOnBuild = isLocal;   // Validate that all dependencies can be built
    });

    // --- Application configuration ---
    builder.Configuration.SetupAppConfiguration(builder.Environment);

    // --- Application services setup ---
    builder.SetupWebApi()
        .SetupAppServices()
        .SetupAppAuthentication()
        .SetupAppAuthorization()
        .SetupHealthChecks()
        .SetupSwaggerPage()
        .SetupValidation()
        .SetupCors();

    var app = builder.Build();
    
    // --- Database Initialization ---
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<ServiceNameOrCustomDbContext>();
        var logger = services.GetRequiredService<ILogger<MigrationRunner>>();
        
        // Run Migrations
        MigrationRunner.RunMigrations(dbContext, logger);
        
        // Data Seeding
        try
        {
            var dataSeeder = services.GetRequiredService<DataSeeder>();
            await dataSeeder.SeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    // --- Middleware ---
    app.UseLogging()
        .UseWebServer()
        .UseAppCors()
        .UseAppErrorHandling()
        .UseAppAuthentication()
        .UseAppAuthorization()
        .UseSwaggerPage()
//#if (Hangfire)
        .UseAppHangfire()
//#endif
        .UseWebApi();

    // --- Health checks ---
    app.MapHealthEndpoints();

    // --- Application startup ---
    app.Run();
}
catch (HostAbortedException ex)
{
    Log.Warning(ex, "Host was aborted. This may be expected in some environments.");
}
// Build-time tools such as dotnet-getdocument start this entry point and stop it with an internal
// exception once the host is built. Swallowing it below would make the run look like a clean exit,
// and the tool would report that no host was built.
catch (Exception ex) when (ex.GetType().Name == "StopTheHostException")
{
    throw;
}
catch (Exception ex)
{
    Log.Fatal(ex, "The {EntryAssemblyName} application startup failed", Assembly.GetEntryAssembly()?.GetName().Name);
    // A failure before logging is configured leaves Serilog silent, so the error also goes to stderr.
    // A non-zero exit code tells the orchestrator the service did not start.
    Console.Error.WriteLine(ex);
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}