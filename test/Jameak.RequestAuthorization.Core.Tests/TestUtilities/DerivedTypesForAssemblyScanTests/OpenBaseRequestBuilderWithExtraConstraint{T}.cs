using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.DerivedTypesForAssemblyScanTests;

internal class OpenBaseRequestBuilderWithExtraConstraint<T> : IRequestAuthorizationRequirementBuilder<IBaseRequest1<T>> where T : UserDataRequest2
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(IBaseRequest1<T> request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
    }
}
