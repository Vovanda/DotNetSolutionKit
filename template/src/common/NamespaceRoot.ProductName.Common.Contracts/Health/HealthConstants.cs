namespace NamespaceRoot.ProductName.Common.Contracts.Health;

public static class HealthConstants
{
    public const string Healthz = "/healthz";
    public const string Readyz = "/readyz";
    
    public static readonly string[] AllPaths = [Healthz, Readyz];
}