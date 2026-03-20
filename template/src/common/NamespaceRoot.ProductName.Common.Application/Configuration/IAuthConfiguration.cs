namespace NamespaceRoot.ProductName.Common.Application.Configuration;

/// <summary>
/// JWT configuration for services that only validate tokens (public key only).
/// </summary>
public interface IJwtPublicConfiguration
{
    string Issuer { get; }
    string Audience { get; }
    int LifetimeMinutes { get; }
    string PublicKeyPath { get; }
}

/// <summary>
/// Full JWT configuration for services that sign and validate tokens (Auth service).
/// </summary>
public interface IJwtConfiguration : IJwtPublicConfiguration
{
    string PrivateKeyPath { get; }
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