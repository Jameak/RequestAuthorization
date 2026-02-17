namespace Jameak.RequestAuthorization.Core.Abstractions;

/// <summary>
/// Builds authorization requirements that apply to all requests.
/// </summary>
public interface IGlobalRequestAuthorizationRequirementBuilder
{
    /// <summary>
    /// Builds a requirement for the specified request.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="request">The request instance.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that produces a requirement.</returns>
    Task<IRequestAuthorizationRequirement> BuildRequirementAsync<TRequest>(
        TRequest request,
        CancellationToken token);
}
