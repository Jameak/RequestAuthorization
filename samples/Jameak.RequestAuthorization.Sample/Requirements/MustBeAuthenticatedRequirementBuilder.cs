using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Sample.Requirements;

public class MustBeAuthenticatedRequirementBuilder : IGlobalRequestAuthorizationRequirementBuilder
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync<TRequest>(
        TRequest request,
        CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new MustBeAuthenticatedRequirement());
    }
}
