using System.Diagnostics;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Jameak.RequestAuthorization.Adapter.AspNetCore;

/// <summary>
/// Handles <see cref="AspNetAuthorizationPolicyRequirement"/> using <see cref="IAuthorizationService"/>.
/// </summary>
public sealed class AspNetAuthorizationPolicyRequirementHandler : RequestAuthorizationHandlerBase<AspNetAuthorizationPolicyRequirement>
{
    private readonly IAuthorizationService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetAuthorizationPolicyRequirementHandler"/> class.
    /// </summary>
    /// <param name="authService">The ASP.NET Core authorization service.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public AspNetAuthorizationPolicyRequirementHandler(
        IAuthorizationService authService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authService = authService;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Evaluates the requirement using ASP.NET Core authorization.
    /// </summary>
    /// <param name="requirement">The requirement instance.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The authorization result.</returns>
    public override async Task<RequestAuthorizationResult> CheckRequirementAsync(AspNetAuthorizationPolicyRequirement requirement, CancellationToken token)
    {
        AuthorizationResult aspNetAuthResult;

        if (requirement.PolicyName != null)
        {
            aspNetAuthResult = await _authService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, requirement.Resource, requirement.PolicyName);
        }
        else
        {
            Debug.Assert(requirement.Policy != null);
            aspNetAuthResult = await _authService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, requirement.Resource, requirement.Policy);
        }

        if (aspNetAuthResult.Succeeded)
        {
            return RequestAuthorizationResult.Success(requirement, context: aspNetAuthResult);
        }

        return RequestAuthorizationResult.Fail(
            requirement,
            context: aspNetAuthResult,
            failureReason: string.Join('\n', aspNetAuthResult.Failure.FailureReasons.Select(e => e.Message)));
    }
}
