namespace Jameak.RequestAuthorization.Core.Abstractions;

/// <summary>
/// Builds an authorization requirement for a specific request type.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public interface IRequestAuthorizationRequirementBuilder<in TRequest>
{
    /// <summary>
    /// Builds a requirement for the specified request.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that produces a requirement.</returns>
    Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
        TRequest request,
        CancellationToken token);
}
