using Jameak.RequestAuthorization.Core.Abstractions;
using Mediator;

namespace Jameak.RequestAuthorization.Adapter.Mediator;

/// <summary>
/// A Mediator pipeline behavior that executes request authorization before invoking the next handler.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class RequestAuthorizationPipelineBehavior<TRequest, TResponse> :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    private readonly IAuthorizationPipelineStep<TRequest, TResponse> _corePipelineStep;

    /// <summary>
    /// Initializes a new instance of the pipeline behavior.
    /// </summary>
    /// <param name="corePipelineStep">The authorization pipeline step.</param>
    public RequestAuthorizationPipelineBehavior(
        IAuthorizationPipelineStep<TRequest, TResponse> corePipelineStep)
    {
        _corePipelineStep = corePipelineStep;
    }

    /// <summary>
    /// Applies authorization before invoking the next delegate.
    /// </summary>
    /// <param name="message">The request instance.</param>
    /// <param name="next">The next handler delegate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The handler response.</returns>
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        return await _corePipelineStep.Handle(message, async token => await next(message, token), cancellationToken);
    }
}
