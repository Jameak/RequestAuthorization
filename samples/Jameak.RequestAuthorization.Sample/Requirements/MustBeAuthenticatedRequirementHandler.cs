using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Sample.Requirements;

public sealed class MustBeAuthenticatedRequirementHandler : RequestAuthorizationHandlerBase<MustBeAuthenticatedRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MustBeAuthenticatedRequirementHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<RequestAuthorizationResult> CheckRequirementAsync(MustBeAuthenticatedRequirement requirement, CancellationToken token)
    {
        if (_httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false)
        {
            return Task.FromResult(RequestAuthorizationResult.Success(requirement));
        }

        return Task.FromResult(RequestAuthorizationResult.Fail(requirement));
    }
}
