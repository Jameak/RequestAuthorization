using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.MediatRSample.Requirements;

public class MustBeAuthenticatedGlobalRequirementBuilder : IGlobalRequestAuthorizationRequirementBuilder
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync<TRequest>(
        TRequest request,
        CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new MustBeAuthenticatedRequirement());
    }
}
