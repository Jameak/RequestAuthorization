using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Requirements;

/// <summary>
/// Provides fluent composition operators for authorization requirements.
/// </summary>
/// <remarks>
/// Fluent composition produces the same requirement graph as the <see cref="Require"/> API.
/// </remarks>
public static class RequireExtensions
{
    /// <summary>
    /// Creates a composite requirement that succeeds when either this requirement or the specified requirement succeeds.
    /// </summary>
    public static IRequestAuthorizationRequirement Or(
        this IRequestAuthorizationRequirement left,
        IRequestAuthorizationRequirement right)
        => new AnyRequirement([left, right]);

    /// <summary>
    /// Creates a composite requirement that succeeds when any requirement in the enumerable succeeds.
    /// </summary>
    public static IRequestAuthorizationRequirement Any(
        this IEnumerable<IRequestAuthorizationRequirement> requirements)
        => new AnyRequirement(requirements);

    /// <summary>
    /// Creates a composite requirement that succeeds only when both this requirement and the specified requirement succeed.
    /// </summary>
    public static IRequestAuthorizationRequirement And(
        this IRequestAuthorizationRequirement left,
        IRequestAuthorizationRequirement right)
        => new AllRequirement([left, right]);

    /// <summary>
    /// Creates a composite requirement that succeeds only when all requirements in the enumerable succeed.
    /// </summary>
    public static IRequestAuthorizationRequirement All(
        this IEnumerable<IRequestAuthorizationRequirement> requirements)
        => new AllRequirement(requirements);
}
