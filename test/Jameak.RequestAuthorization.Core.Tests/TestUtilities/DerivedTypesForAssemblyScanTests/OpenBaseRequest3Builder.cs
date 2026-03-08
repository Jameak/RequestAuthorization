using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.DerivedTypesForAssemblyScanTests;

internal class OpenBaseRequest3Builder<T> : IRequestAuthorizationRequirementBuilder<IBaseRequest3<T>>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(IBaseRequest3<T> request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
    }
}
