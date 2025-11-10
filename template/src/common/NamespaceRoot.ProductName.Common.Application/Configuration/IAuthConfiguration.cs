namespace NamespaceRoot.ProductName.Common.Application.Configuration;

public interface IJwtConfiguration
{
    string Issuer { get; }
    string Audience { get; }
    int LifetimeMinutes { get; }
    string PrivateKeyPath { get; }
    string PublicKeyPath { get; }
}

public interface IRefreshTokenConfiguration
{
    int ExpirationDays { get; }
    int Size { get; }
}

public interface IInternalApiConfiguration
{
    string ApiKey { get; }
}

public interface IAuthConfiguration
{
    bool RequireHttpsMetadata { get; }
    bool SaveToken { get; }
    bool ValidateIssuer { get; }
    bool ValidateAudience { get; }
    bool ValidateLifetime { get; }
    bool ValidateIssuerSigningKey { get; }
}