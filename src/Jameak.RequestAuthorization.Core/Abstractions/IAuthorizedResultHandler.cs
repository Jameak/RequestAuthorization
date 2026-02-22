using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Abstractions;

/// <summary>
/// Provides a callback invoked when a request is authorized.
/// </summary>
/// <remarks>
/// This handler allows global side-effects to occur after authorization succeeds,
/// such as logging or auditing where access to the successful
/// <see cref="RequestAuthorizationResult"/> is desired.
/// 
/// This handler does not influence the response and should not throw to
/// alter authorization outcomes.
/// </remarks>
public interface IAuthorizedResultHandler
{
    /// <summary>
    /// Called when authorization succeeds.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="message">The request instance.</param>
    /// <param name="result">The authorization result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task OnAuthorized<TRequest>(
        TRequest message,
        RequestAuthorizationResult result,
        CancellationToken cancellationToken);
}
