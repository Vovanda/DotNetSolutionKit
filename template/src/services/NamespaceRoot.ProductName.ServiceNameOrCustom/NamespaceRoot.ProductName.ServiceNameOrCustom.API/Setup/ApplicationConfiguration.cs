using NamespaceRoot.ProductName.Common.Application.FeatureManagement;
namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class ApplicationConfiguration
{
    public static IConfigurationBuilder SetupAppConfiguration(this IConfigurationBuilder builder, IHostEnvironment env)
    {
        // Platform feature flags, shipped from Common so every service reads the same file and a
        // feature means one thing across the platform. First, so everything below can override it.
        builder.AddPlatformFeatures();

        // Always base config
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Environment-specific configuration
        if (!string.IsNullOrWhiteSpace(env.EnvironmentName))
        {
            var envConfigFile = $"appsettings.{env.EnvironmentName}.json";
            builder.AddJsonFile(envConfigFile, optional: true, reloadOnChange: true);
        }

        // Secret files only for Local
        if (env.IsEnvironment("Local"))
        {
            builder.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
        }

        // Always connect environment variables (secrets, dynamic parameters)
        builder.AddEnvironmentVariables();

        return builder;
    }
}