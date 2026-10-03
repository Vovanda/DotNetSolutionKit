namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Tests.Misc.Stabs;

public class TimeProviderMock : TimeProvider
{
    private DateTimeOffset? _fixedUtcNow;

    public TimeProviderMock(DateTimeOffset? fixedUtcNow = null)
    {
        _fixedUtcNow = fixedUtcNow;
    }

    /// <summary>
    /// Get current fixed time or real UTC time.
    /// </summary>
    public override DateTimeOffset GetUtcNow() => _fixedUtcNow ?? DateTimeOffset.UtcNow;

    /// <summary>
    /// Set fixed time.
    /// </summary>
    /// <param name="utcNow">UTC time that will be returned.</param>
    public void SetUtcNow(DateTimeOffset utcNow) => _fixedUtcNow = utcNow;

    /// <summary>
    /// Reset fixed time (return to real UTC time).
    /// </summary>
    public void Reset() => _fixedUtcNow = null;
}