using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using NamespaceRoot.ProductName.Common.Application.Configuration;
using NamespaceRoot.ProductName.Common.Exceptions;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Infrastructure.Security;

public class RsaKeyProvider : IRsaKeyProvider
{
    private readonly ILogger<RsaKeyProvider> _logger;
    
    public RSA PrivateRsa { get; }
    public RSA PublicRsa { get; }
    public RsaSecurityKey SigningKey { get; }
    public RsaSecurityKey ValidationKey { get; }

    public RsaKeyProvider(IJwtConfiguration jwtConfig, ILogger<RsaKeyProvider> logger)
    {
        _logger = logger;
        
        try
        {
            _logger.LogInformation("Loading RSA keys...");
            _logger.LogDebug("Private key path: {PrivateKeyPath}", jwtConfig.PrivateKeyPath);
            _logger.LogDebug("Public key path: {PublicKeyPath}", jwtConfig.PublicKeyPath);
            
            PrivateRsa = LoadKeyFromFile(jwtConfig.PrivateKeyPath, isPrivate: true);
            SigningKey = new RsaSecurityKey(PrivateRsa);
            
            PublicRsa = LoadKeyFromFile(jwtConfig.PublicKeyPath, isPrivate: false);
            ValidationKey = new RsaSecurityKey(PublicRsa);
            
            LogKeyInfo();
            _logger.LogInformation("RSA keys loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load RSA keys");
            throw new ConfigurationException($"Failed to initialize RSA keys: {ex.Message}", ex);
        }
    }

    private RSA LoadKeyFromFile(string filePath, bool isPrivate)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{GetKeyType(isPrivate)} key file not found", filePath);
            }
            
            var keyContent = File.ReadAllText(filePath);
            
            if (string.IsNullOrWhiteSpace(keyContent))
                throw new ConfigurationException($"{GetKeyType(isPrivate)} key file is empty: {filePath}");
            
            var rsa = RSA.Create();
            rsa.ImportFromPem(keyContent);
            
            _logger.LogDebug("Successfully loaded {KeyType} key from {FilePath}", 
                GetKeyType(isPrivate), filePath);
            
            return rsa;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Key file not found");
            throw new ConfigurationException($"{GetKeyType(isPrivate)} key file not found: {filePath}", ex);
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "Invalid key format");
            throw new ConfigurationException($"Invalid {GetKeyType(isPrivate)} key format in file: {filePath}", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Access denied to key file");
            throw new ConfigurationException($"Access denied to {GetKeyType(isPrivate)} key file: {filePath}", ex);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "IO error reading key file");
            throw new ConfigurationException($"IO error reading {GetKeyType(isPrivate)} key file: {filePath}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading key");
            throw new ConfigurationException($"Unexpected error loading {GetKeyType(isPrivate)} key: {ex.Message}", ex);
        }
    }

    private string GetKeyType(bool isPrivate) => isPrivate ? "Private" : "Public";

    private void LogKeyInfo()
    {
        _logger.LogDebug("RSA keys loaded - Private: {PrivateKeySize} bits, Public: {PublicKeySize} bits", 
            PrivateRsa.KeySize, PublicRsa.KeySize);
        
        if (PrivateRsa.KeySize < 2048 || PublicRsa.KeySize < 2048)
        {
            _logger.LogWarning("RSA key size less than 2048 bits (Private: {PrivateKeySize}, Public: {PublicKeySize}) - consider upgrading for better security",
                PrivateRsa.KeySize, PublicRsa.KeySize);
        }
    }

    public SigningCredentials GetSigningCredentials()
    {
        return new SigningCredentials(SigningKey, SecurityAlgorithms.RsaSha256);
    }
}