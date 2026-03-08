using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;

internal class BuilderHandlingMultipleAssignableRequests : IRequestAuthorizationRequirementBuilder<TestBaseRequest>, IRequestAuthorizationRequirementBuilder<TestBaseRequest2>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestBaseRequest2 request, CancellationToken token) => throw new NotImplementedException();
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestBaseRequest request, CancellationToken token) => throw new NotImplementedException();
}
