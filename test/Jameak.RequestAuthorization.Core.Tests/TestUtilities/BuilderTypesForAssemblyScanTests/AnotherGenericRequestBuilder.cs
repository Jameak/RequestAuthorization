using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;

internal class AnotherGenericRequestBuilder<T> : IRequestAuthorizationRequirementBuilder<TestRequest>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token) => throw new NotImplementedException();
}
