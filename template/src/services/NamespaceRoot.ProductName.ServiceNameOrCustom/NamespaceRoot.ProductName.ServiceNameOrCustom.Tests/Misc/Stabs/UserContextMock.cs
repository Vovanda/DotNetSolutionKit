using NamespaceRoot.ProductName.Common.Contracts.Domain.Context;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Tests.Misc.Stabs;

public class UserContextMock : IUserContext
{
    public UserContextMock(
        string userId = TestDomainExecutionContext.DefaultTestUserId,
        string login = "testuser@example.com", 
        string displayName = "Test User",
        Guid? apiKeyId = null,
        IAuthContext? authContext = null)
    {
        UserId = Guid.Parse(userId);
        Login = login;
        DisplayName = displayName;
        ApiKeyId = apiKeyId;
        AuthContext = authContext ?? AuthContextMock.CreateJwtAuth(TimeProvider.System);
    }

    public Guid UserId { get; set; }
    public Guid? PartnerId { get; set;}

    public string? Login { get; set;}

    public string? DisplayName { get; set; }

    public Guid? ApiKeyId { get; set; }

    public IAuthContext AuthContext { get; set; }
}

public class AuthContextMock : IAuthContext
{
    public AuthContextMock(
        AuthMethod type, 
        string id,
        DateTimeOffset expireAt)
    {
        Type = type;
        Id = id;
        ExpireAt = expireAt;
    }

    public AuthMethod Type { get; }

    public string Id { get; }

    public DateTimeOffset ExpireAt { get; }

    public static AuthContextMock CreateJwtAuth(
        TimeProvider timeProvider,
        string? jwtId = null,
        DateTimeOffset? expireAt = null)
    {
        return new AuthContextMock(
            AuthMethod.Jwt,
            id: jwtId ?? $"jwt-{Guid.NewGuid()}",
            expireAt: expireAt ?? timeProvider.GetUtcNow().AddDays(30));
    }

    public static AuthContextMock CreateApiKeyAuth(
        TimeProvider timeProvider,
        Guid apiKeyId,
        DateTimeOffset? expireAt = null)
    {
        return new AuthContextMock(
            AuthMethod.ApiKey,
            id: apiKeyId.ToString(),
            expireAt: expireAt ?? timeProvider.GetUtcNow().AddDays(30));
    }

    public static AuthContextMock CreateSystemAuth()
    {
        return new AuthContextMock(
            AuthMethod.System,
            id: "00000000-0000-0000-0000-000000000000",
            expireAt: DateTimeOffset.MaxValue);
    }

    public static AuthContextMock CreateExpiredAuth(
        TimeProvider timeProvider,
        AuthMethod type = AuthMethod.Jwt)
    {
        return new AuthContextMock(
            type,
            id: type == AuthMethod.ApiKey ? Guid.NewGuid().ToString() : $"expired-{Guid.NewGuid()}",
            expireAt: timeProvider.GetUtcNow().AddDays(-1));
    }
}

public static class UserContextMockFactory
{
    public static UserContextMock CreateJwtUser(
        TimeProvider timeProvider,
        string userId = TestDomainExecutionContext.DefaultTestUserId,
        string login = "testuser@example.com",
        string displayName = "Test User",
        string? jwtId = null,
        DateTimeOffset? jwtExpireAt = null)
    {
        return new UserContextMock(
            userId: userId,
            login: login,
            displayName: displayName,
            authContext: AuthContextMock.CreateJwtAuth(timeProvider, jwtId, jwtExpireAt));
    }

    public static UserContextMock CreateApiKeyUser(
        TimeProvider timeProvider,
        Guid apiKeyId,
        string userId = TestDomainExecutionContext.DefaultApiKeyUserId,
        DateTimeOffset? apiKeyExpireAt = null)
    {
        return new UserContextMock(
            userId: userId,
            login: "apikey@system",
            displayName: "API Key User", 
            apiKeyId: apiKeyId,
            authContext: AuthContextMock.CreateApiKeyAuth(timeProvider, apiKeyId, apiKeyExpireAt));
    }

    public static UserContextMock CreateSystemUser()
    {
        return new UserContextMock(
            userId: TestDomainExecutionContext.DefaultTestUserId,
            login: "system", 
            displayName: "System",
            authContext: AuthContextMock.CreateSystemAuth());
    }

    public static UserContextMock CreateExpiredJwtUser(TimeProvider timeProvider)
    {
        return new UserContextMock(
            userId: "00000000-0000-0000-0000-000000000003",
            login: "expired@example.com",
            displayName: "Expired User",
            authContext: AuthContextMock.CreateExpiredAuth(timeProvider, type:AuthMethod.Jwt));
    }

    public static UserContextMock CreateExpiredApiKeyUser(TimeProvider timeProvider, Guid apiKeyId)
    {
        return new UserContextMock(
            userId: "00000000-0000-0000-0000-000000000004",
            login: "expired-api@system",
            displayName: "Expired API User",
            apiKeyId: apiKeyId,
            authContext: AuthContextMock.CreateExpiredAuth(timeProvider, type:AuthMethod.ApiKey));
    }
}