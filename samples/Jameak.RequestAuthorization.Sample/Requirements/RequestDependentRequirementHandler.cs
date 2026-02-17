using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Sample.Requirements;

public sealed class RequestDependentRequirementHandler : RequestAuthorizationHandlerBase<RequestDependentRequirement>
{
    private readonly IAuthService _authService;

    public RequestDependentRequirementHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public override Task<RequestAuthorizationResult> CheckRequirementAsync(RequestDependentRequirement requirement, CancellationToken token)
    {
        if (_authService.IsAllowed(requirement.Request))
        {
            return Task.FromResult(RequestAuthorizationResult.Success(requirement));
        }

        return Task.FromResult(RequestAuthorizationResult.Fail(requirement));
    }
}
