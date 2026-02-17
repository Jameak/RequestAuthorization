using Jameak.RequestAuthorization.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Jameak.RequestAuthorization.Adapter.AspNetCore;

/// <summary>
/// Represents an authorization requirement composed of
/// ASP.NET Core <see cref="IAuthorizationRequirement"/> instances.
/// </summary>
public sealed class AspNetAuthorizationRequirement : IRequestAuthorizationRequirement
{
    /// <summary>
    /// Gets the collection of ASP.NET Core authorization requirements.
    /// </summary>
    public IReadOnlyList<IAuthorizationRequirement> Requirements { get; }

    /// <summary>
    /// Gets the resource passed to the ASP.NET Core authorization service.
    /// </summary>
    public object? Resource { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetAuthorizationRequirement"/> class.
    /// </summary>
    /// <param name="requirements">The ASP.NET Core authorization requirements.</param>
    /// <param name="resource">The authorization resource.</param>
    public AspNetAuthorizationRequirement(IReadOnlyList<IAuthorizationRequirement> requirements, object? resource)
    {
        Requirements = requirements;
        Resource = resource;
    }
}
