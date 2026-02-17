namespace Jameak.RequestAuthorization.Core.Abstractions;

/// <summary>
/// Defines an authorization step in a request/response pipeline.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IAuthorizationPipelineStep<TRequest, TResponse>
{
    /// <summary>
    /// Handles the request.
    /// </summary>
    /// <param name="message">The request instance.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the response.</returns>
    ValueTask<TResponse> Handle(
        TRequest message,
        Func<CancellationToken, Task<TResponse>> next,
        CancellationToken cancellationToken);
}
