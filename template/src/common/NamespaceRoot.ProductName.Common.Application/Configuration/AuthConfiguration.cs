using System.ComponentModel.DataAnnotations;

namespace NamespaceRoot.ProductName.Common.Application.Configuration;

public class JwtConfiguration : IJwtConfiguration
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT issuer is required")]
    public string Issuer { get; init; } = string.Empty;

    [Required(ErrorMessage = "JWT audience is required")]
    public string Audience { get; init; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "JWT lifetime must be between 1 and 1440 minutes (24 hours)")]
    public int LifetimeMinutes { get; init; } = 60;

    [Required(ErrorMessage = "Private key path is required")]
    public string PrivateKeyPath { get; init; } = string.Empty;

    [Required(ErrorMessage = "Public key path is required")]
    public string PublicKeyPath { get; init; } = string.Empty;
}

public class RefreshTokenConfiguration : IRefreshTokenConfiguration
{
    public const string SectionName = "RefreshTokenOptions";

    [Range(1, 365, ErrorMessage = "Refresh token expiration must be between 1 and 365 days")]
    public int ExpirationDays { get; init; } = 30;

    [Range(16, 64, ErrorMessage = "Refresh token size must be between 16 and 64 bytes")]
    public int Size { get; init; } = 32;
}

public class InternalApiConfiguration : IInternalApiConfiguration
{
    public const string SectionName = "InternalApi";

    [Required(ErrorMessage = "Internal API key is required")]
    [MinLength(16, ErrorMessage = "Internal API key must be at least 16 characters")]
    public string ApiKey { get; init; } = string.Empty;
}

public class AuthConfiguration : IAuthConfiguration
{
    public const string SectionName = "Auth";

    public bool RequireHttpsMetadata { get; init; } = true;
    public bool SaveToken { get; init; } = true;
    public bool ValidateIssuer { get; init; } = true;
    public bool ValidateAudience { get; init; } = true;
    public bool ValidateLifetime { get; init; } = true;
    public bool ValidateIssuerSigningKey { get; init; } = true;
}