using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Abstractions;

/// <summary>
/// Handles unauthorized authorization results.
/// </summary>
public interface IUnauthorizedResultHandler
{
    /// <summary>
    /// Called when authorization fails for a non-streaming request.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="message">The request instance.</param>
    /// <param name="authResult">The authorization result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that produces the response.</returns>
    Task<TResponse> OnUnauthorized<TRequest, TResponse>(
        TRequest message,
        RequestAuthorizationResult authResult,
        CancellationToken cancellationToken);

    /// <summary>
    /// Called when authorization fails for a streaming request.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The streamed response element type.</typeparam>
    /// <param name="message">The request instance.</param>
    /// <param name="authResult">The authorization result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that produces the response.</returns>
    Task<TResponse> OnUnauthorizedStream<TRequest, TResponse>(
        TRequest message,
        RequestAuthorizationResult authResult,
        CancellationToken cancellationToken);
}
