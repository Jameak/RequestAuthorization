using System.Runtime.CompilerServices;

#if DEBUG || IS_CI_TEST_BUILD
[assembly: InternalsVisibleTo("Jameak.RequestAuthorization.Adapter.AspNetCore.Tests")]
[assembly: InternalsVisibleTo("Jameak.RequestAuthorization.Adapter.AspNetCore.Nuget.Tests")]
[assembly: InternalsVisibleTo("Jameak.RequestAuthorization.Core.Tests")]
[assembly: InternalsVisibleTo("Jameak.RequestAuthorization.Core.Nuget.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
#endif
