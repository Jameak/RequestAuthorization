using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;

internal class GenericRequestBuilder<T> : IRequestAuthorizationRequirementBuilder<T>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(T request, CancellationToken token) => throw new NotImplementedException();
}
