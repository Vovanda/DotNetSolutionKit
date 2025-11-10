using JetBrains.Annotations;

namespace NamespaceRoot.ProductName.Common.Contracts.FeatureManagement;

/// <summary>
/// Feature flag names
/// </summary>
[PublicAPI]
public enum FeatureFlags
{
    /// <summary>
    /// Test flag
    /// Add PBI number with description for each feature flag:
    /// PBI xxxxxx: PBI title
    /// </summary>
    TestFlag,

    /// <summary>
    /// Makes test service controller visible
    /// </summary>
    AllowTestController
}