using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.DerivedTypesForAssemblyScanTests;

internal class NotBaseRequestBuilder<T> : IRequestAuthorizationRequirementBuilder<IRequest<T>>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(IRequest<T> request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
    }
}
