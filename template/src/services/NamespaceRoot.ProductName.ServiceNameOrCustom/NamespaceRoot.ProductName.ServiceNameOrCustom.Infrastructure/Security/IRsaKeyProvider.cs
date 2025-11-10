using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.Security;

public interface IRsaKeyProvider
{
    /// <summary>
    /// RSA instance with private key for signing tokens
    /// </summary>
    RSA PrivateRsa { get; }
    
    /// <summary>
    /// RSA instance with public key for token validation
    /// </summary>
    RSA PublicRsa { get; }
    
    /// <summary>
    /// Security key for signing (private key)
    /// </summary>
    RsaSecurityKey SigningKey { get; }
    
    /// <summary>
    /// Security key for validation (public key)
    /// </summary>
    RsaSecurityKey ValidationKey { get; }
    
    /// <summary>
    /// Gets the signing credentials for JWT token generation
    /// </summary>
    SigningCredentials GetSigningCredentials();
}