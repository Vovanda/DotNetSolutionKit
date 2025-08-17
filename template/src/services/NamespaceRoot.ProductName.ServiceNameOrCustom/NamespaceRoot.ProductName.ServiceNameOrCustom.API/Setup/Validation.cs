namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class Validation
{
    public static WebApplicationBuilder SetupValidation(this WebApplicationBuilder builder)
    {
        // Здесь можно подключить FluentValidation или другой валидатор
        // builder.Services.AddValidatorsFromAssemblyContaining<MyValidator>();
        return builder;
    }
}