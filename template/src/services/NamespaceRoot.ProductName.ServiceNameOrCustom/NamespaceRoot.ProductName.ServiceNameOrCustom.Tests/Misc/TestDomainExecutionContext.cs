using NamespaceRoot.ProductName.Common.Contracts.Domain.Context;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Tests.Misc.Stabs;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Tests.Misc;

public class TestDomainExecutionContext : IDomainExecutionContext
{
    // Test user constants with valid GUID strings
    public const string DefaultTestUserId = "00000000-0000-0000-0000-000000000001";
    public const string DefaultApiKeyUserId = "00000000-0000-0000-0000-000000000002";
    
    public TestDomainExecutionContext(IUserContext actor, TimeProvider timeProvider)
    {
        Actor = actor;
        TimeProvider = timeProvider;
    }

    public IUserContext Actor { get; private set; }

    public TimeProvider TimeProvider { get; }

    /// <summary>
    /// Set current actor (login, roles, etc.)
    /// </summary>
    public void SetActor(IUserContext userContext)
    {
        Actor = userContext;
    }

    /// <summary>
    /// Set current actor with basic parameters
    /// </summary>
    public void SetActor(string userId, string login, string displayName = "Test User")
    {
        Actor = new UserContextMock(userId, login, displayName);
    }

    /// <summary>
    /// Set current actor as JWT user
    /// </summary>
    public void SetJwtActor(
        string userId = DefaultTestUserId,
        string login = "testuser@example.com",
        string displayName = "Test User")
    {
        Actor = UserContextMockFactory.CreateJwtUser(TimeProvider, userId, login, displayName);
    }

    /// <summary>
    /// Set current actor as API Key user
    /// </summary>
    public void SetApiKeyActor(Guid apiKeyId, string userId = DefaultApiKeyUserId)
    {
        Actor = UserContextMockFactory.CreateApiKeyUser(TimeProvider, apiKeyId, userId);
    }

    /// <summary>
    /// Set current actor as system user
    /// </summary>
    public void SetSystemActor()
    {
        Actor = UserContextMockFactory.CreateSystemUser();
    }
}