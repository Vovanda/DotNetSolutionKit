using NamespaceRoot.ProductName.Common.Contracts.FeatureManagement;

namespace NamespaceRoot.ProductName.Common.Contracts.Common.Attributes;

/// <summary>
/// Attribute for marking elements associated with specific feature <see cref="FeatureFlags"/>.
/// </summary>
/// <param name="feature">Feature associated with element usage.</param>
/// <remarks>
/// Provides change control when removing feature flags: flag removal requires attribute removal, otherwise build will fail.  
/// Additionally can use TODO comments or <see cref="ObsoleteAttribute"/> for generating warnings.
/// </remarks>
[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class AssociatedFeatureAttribute(FeatureFlags feature) : Attribute
{
    /// <summary>
    /// Associated feature.
    /// </summary>
    public FeatureFlags Feature { get; } = feature;
}