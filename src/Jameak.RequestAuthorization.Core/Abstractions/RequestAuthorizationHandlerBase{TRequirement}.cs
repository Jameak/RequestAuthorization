using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Abstractions;

/// <summary>
/// Provides a base class for authorization handlers.
/// </summary>
/// <typeparam name="TRequirement">The requirement type handled.</typeparam>
public abstract class RequestAuthorizationHandlerBase<TRequirement> : IRequestAuthorizationHandler where TRequirement : IRequestAuthorizationRequirement
{
    /// <summary>
    /// Dispatches the requirement to the strongly typed <see cref="CheckRequirementAsync(TRequirement, CancellationToken)"/>.
    /// </summary>
    /// <param name="requirement">The requirement instance.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The authorization result.</returns>
    public Task<RequestAuthorizationResult> CheckRequirementAsync(
        IRequestAuthorizationRequirement requirement,
        CancellationToken token)
        => CheckRequirementAsync((TRequirement)requirement, token);

    /// <summary>
    /// Evaluates the requirement.
    /// </summary>
    /// <param name="requirement">The requirement instance.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The authorization result.</returns>
    public abstract Task<RequestAuthorizationResult> CheckRequirementAsync(
        TRequirement requirement,
        CancellationToken token);
}
