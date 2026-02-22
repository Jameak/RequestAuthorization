using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

public class AlwaysFailureRequirementHandler : RequestAuthorizationHandlerBase<AlwaysFailureRequirement>
{
    public override Task<RequestAuthorizationResult> CheckRequirementAsync(AlwaysFailureRequirement requirement, CancellationToken token)
    {
        return Task.FromResult(RequestAuthorizationResult.Fail(requirement));
    }
}
