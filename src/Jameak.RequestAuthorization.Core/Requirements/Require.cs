using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Requirements;

/// <summary>
/// Provides a fluent composable DSL for building authorization requirement graphs.
/// </summary>
/// <remarks>
/// Requirements created via this API are evaluated using short-circuit semantics:
/// <list type="bullet">
///   <item><see cref="Any"/> succeeds when any child requirement succeeds.</item>
///   <item><see cref="All"/> succeeds only when all child requirements succeed.</item>
/// </list>
/// </remarks>
public static class Require
{
    /// <summary>
    /// Creates a composite requirement that succeeds when any of the supplied requirements succeeds.
    /// </summary>
    /// <param name="requirements"> The child requirements to evaluate. At least two requirements must be supplied.</param>
    /// <returns> An authorization requirement representing logical OR semantics.</returns>
    /// <remarks>
    /// Example:
    /// <code>
    /// var requirement =
    ///     Require.Any(
    ///         new RoleRequirement("Admin"),
    ///         new RoleRequirement("Manager"));
    /// </code>
    /// </remarks>
    public static IRequestAuthorizationRequirement Any(params IRequestAuthorizationRequirement[] requirements)
        => new AnyRequirement(requirements);

    /// <summary>
    /// Creates a composite requirement that succeeds only when all of the supplied requirements succeed.
    /// </summary>
    /// <param name="requirements">The child requirements to evaluate. At least two requirements must be supplied.</param>
    /// <returns>An authorization requirement representing logical AND semantics.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when fewer than two requirements are supplied.
    /// </exception>
    /// <remarks>
    /// Example:
    /// <code>
    /// var requirement =
    ///     Require.All(
    ///         new AuthenticatedRequirement(),
    ///         new ScopeRequirement("orders:write"));
    /// </code>
    /// </remarks>
    public static IRequestAuthorizationRequirement All(params IRequestAuthorizationRequirement[] requirements)
        => new AllRequirement(requirements);
}
