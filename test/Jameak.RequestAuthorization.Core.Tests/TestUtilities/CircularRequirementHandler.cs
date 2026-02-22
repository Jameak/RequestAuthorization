using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal class CircularRequirementHandler : RequestAuthorizationHandlerBase<CircularRequirement>
{
    private readonly IRequestAuthorizationExecutor _executor;

    public CircularRequirementHandler(IRequestAuthorizationExecutor executor)
    {
        _executor = executor;
    }

    public override async Task<RequestAuthorizationResult> CheckRequirementAsync(CircularRequirement requirement, CancellationToken token)
    {
        return await _executor.ExecuteAsync(requirement, token);
    }
}
