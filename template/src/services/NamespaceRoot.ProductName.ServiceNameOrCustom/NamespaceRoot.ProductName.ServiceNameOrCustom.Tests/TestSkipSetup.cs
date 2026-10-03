using System.Runtime.CompilerServices;
using NamespaceRoot.ProductName.Common.Tests;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Tests;

/// <summary>Tells the shared test infrastructure that a skip here is NUnit's ignore.</summary>
internal static class TestSkipSetup
{
    [ModuleInitializer]
    internal static void UseNUnit() => TestSkip.Handler = Assert.Ignore;
}
