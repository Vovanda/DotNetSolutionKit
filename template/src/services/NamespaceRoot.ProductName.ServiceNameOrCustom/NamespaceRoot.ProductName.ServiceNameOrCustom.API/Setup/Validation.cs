namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Setup;

internal static class Validation
{
    public static WebApplicationBuilder SetupValidation(this WebApplicationBuilder builder)
    {
        // FluentValidation or other validation framework can be configured here
        // builder.Services.AddValidatorsFromAssemblyContaining<MyValidator>();
        return builder;
    }
}