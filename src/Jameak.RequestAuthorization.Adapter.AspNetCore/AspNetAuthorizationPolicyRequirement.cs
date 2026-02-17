using Jameak.RequestAuthorization.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Jameak.RequestAuthorization.Adapter.AspNetCore;

/// <summary>
/// Represents an authorization requirement based on an ASP.NET Core authorization policy.
/// </summary>
public sealed class AspNetAuthorizationPolicyRequirement : IRequestAuthorizationRequirement
{
    /// <summary>
    /// Gets the name of the authorization policy.
    /// </summary>
    public string? PolicyName { get; }

    /// <summary>
    /// Gets the authorization policy instance.
    /// </summary>
    public AuthorizationPolicy? Policy { get; }

    /// <summary>
    /// Gets the resource passed to the ASP.NET Core authorization service.
    /// </summary>
    public object? Resource { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetAuthorizationPolicyRequirement"/> class using a policy name.
    /// </summary>
    /// <param name="policyName">The policy name.</param>
    /// <param name="resource">The authorization resource.</param>
    public AspNetAuthorizationPolicyRequirement(string policyName, object? resource)
    {
        PolicyName = policyName;
        Resource = resource;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetAuthorizationPolicyRequirement"/> class using a policy instance.
    /// </summary>
    /// <param name="policy">The authorization policy.</param>
    /// <param name="resource">The authorization resource.</param>
    public AspNetAuthorizationPolicyRequirement(AuthorizationPolicy policy, object? resource)
    {
        Policy = policy;
        Resource = resource;
    }
}
