using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal class GlobalRequirementBuilderProducingFailureRequirement : IGlobalRequestAuthorizationRequirementBuilder
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync<TRequest>(TRequest request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysFailureRequirement());
    }
}
