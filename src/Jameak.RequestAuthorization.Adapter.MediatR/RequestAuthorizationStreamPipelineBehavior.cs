using Jameak.RequestAuthorization.Core.Abstractions;
using MediatR;

namespace Jameak.RequestAuthorization.Adapter.MediatR;

/// <summary>
/// A MediatR stream pipeline behavior that executes request authorization before invoking the next handler.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class RequestAuthorizationStreamPipelineBehavior<TRequest, TResponse>
    : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuthorizationStreamPipelineStep<TRequest, TResponse> _corePipelineStep;

    /// <summary>
    /// Initializes a new instance of the stream pipeline behavior.
    /// </summary>
    /// <param name="corePipelineStep">The authorization pipeline step.</param>
    public RequestAuthorizationStreamPipelineBehavior(
        IAuthorizationStreamPipelineStep<TRequest, TResponse> corePipelineStep)
    {
        _corePipelineStep = corePipelineStep;
    }

    /// <summary>
    /// Applies authorization before invoking the next delegate.
    /// </summary>
    /// <param name="request">The request instance.</param>
    /// <param name="next">The next handler delegate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous stream of responses.</returns>
    public IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return _corePipelineStep.Handle(request, token => next(), cancellationToken);
    }
}
