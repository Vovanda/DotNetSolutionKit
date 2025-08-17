namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class ApplicationConfiguration
{
    public static IConfigurationBuilder SetupAppConfiguration(this IConfigurationBuilder builder, IHostEnvironment env)
    {
        // Всегда базовый конфиг
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Конфигурация для конкретного окружения
        if (!string.IsNullOrWhiteSpace(env.EnvironmentName))
        {
            var envConfigFile = $"appsettings.{env.EnvironmentName}.json";
            builder.AddJsonFile(envConfigFile, optional: true, reloadOnChange: true);
        }

        // секретные файлы только для Local
        if (env.IsEnvironment("Local"))
        {
            builder.AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);
        }

        // Всегда подключаем переменные окружения (секреты, динамические параметры)
        builder.AddEnvironmentVariables();

        return builder;
    }
}