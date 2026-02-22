using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;

internal abstract class AbstractRequestBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
{
    public abstract Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token);
}
