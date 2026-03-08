using System.Runtime.CompilerServices;

namespace Jameak.RequestAuthorization.Core.Tests;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initializer()
    {
        Verifier.UseSourceFileRelativeDirectory("__snapshots__");
    }
}
