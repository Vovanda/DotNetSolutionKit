using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.EntityFramework;

[UsedImplicitly]
public class ServiceNameOrCustomDbContextFactory : IDesignTimeDbContextFactory<ServiceNameOrCustomDbContext>
{
    private static DbContextOptions<ServiceNameOrCustomDbContext> GetSqlServerOptions(string connectionString)
    {
        return new DbContextOptionsBuilder<ServiceNameOrCustomDbContext>()
            .UseNpgsql(connectionString,
                x => { x.MigrationsHistoryTable("__EFMigrationsHistory", ServiceNameOrCustomDbContext.DefaultSchemaName); })
            .Options;
    }

    private static string GetConnectionString()
    {
        // First try environment variables (works everywhere)
        var envConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrEmpty(envConnectionString))
        {
            Console.WriteLine("Using connection string from environment variables");
            return envConnectionString;
        }

        var currentDir = Directory.GetCurrentDirectory();
        Console.WriteLine($"Current directory: {currentDir}");
    
        // If we're in Infrastructure, go up one level to API project
        var apiProjectPath = Path.Combine(currentDir, "..", "NamespaceRoot.ProductName.ServiceNameOrCustom.API");
        var apiProjectFullPath = Path.GetFullPath(apiProjectPath);
    
        Console.WriteLine($"Looking for configs in: {apiProjectFullPath}");
    
        if (!Directory.Exists(apiProjectFullPath))
        {
            throw new Exception($"API project directory not found: {apiProjectFullPath}");
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectFullPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Local.json", optional: true)
            .AddJsonFile("appsettings.Secrets.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
    
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Connection string 'DefaultConnection' not found in config files");
        }

        Console.WriteLine("Successfully found connection string");
        return connectionString;
    }

    public ServiceNameOrCustomDbContext CreateDbContext(string[] args)
    {
        var connectionString = GetConnectionString();
        Console.WriteLine($"ConnectionString: {connectionString}");
        
        return new ServiceNameOrCustomDbContext(GetSqlServerOptions(connectionString));
    }
}