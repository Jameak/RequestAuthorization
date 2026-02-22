using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

public class AlwaysSuccessRequirementHandler : RequestAuthorizationHandlerBase<AlwaysSuccessRequirement>
{
    public override Task<RequestAuthorizationResult> CheckRequirementAsync(AlwaysSuccessRequirement requirement, CancellationToken token)
    {
        return Task.FromResult(RequestAuthorizationResult.Success(requirement));
    }
}
