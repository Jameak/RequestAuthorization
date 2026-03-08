using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.DerivedTypesForAssemblyScanTests;

internal abstract class AbstractBaseRequestBuilder<T> : IRequestAuthorizationRequirementBuilder<IBaseRequest1<T>>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(IBaseRequest1<T> request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
    }
}
