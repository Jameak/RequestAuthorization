using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;

internal class BuilderInheritingFromUserProvidedAbstract : AbstractRequestBuilder
{
    public override Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token) => throw new NotImplementedException();
}
