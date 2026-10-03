using System.Runtime.CompilerServices;
using NamespaceRoot.ProductName.Common.Tests;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Tests;

//#if (TestFramework == "xunit")
/// <summary>Tells the shared test infrastructure that a skip here is xUnit's skip.</summary>
internal static class TestSkipSetup
{
    [ModuleInitializer]
    internal static void UseXunit() => TestSkip.Handler = reason => Assert.Skip(reason);
}
//#else
/// <summary>Tells the shared test infrastructure that a skip here is NUnit's ignore.</summary>
internal static class TestSkipSetup
{
    [ModuleInitializer]
    internal static void UseNUnit() => TestSkip.Handler = Assert.Ignore;
}
//#endif
