using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;

internal class BuilderImplementingMultipleInterfaces : IRequestAuthorizationRequirementBuilder<TestRequest>, IRequestAuthorizationRequirementBuilder<TestRequest2>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token) => throw new NotImplementedException();
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest2 request, CancellationToken token) => throw new NotImplementedException();
}
