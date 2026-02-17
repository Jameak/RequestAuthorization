using Jameak.RequestAuthorization.Core.Abstractions;
using MediatR;

namespace Jameak.RequestAuthorization.Adapter.MediatR;

/// <summary>
/// A MediatR pipeline behavior that executes request authorization before invoking the next handler.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class RequestAuthorizationPipelineBehavior<TRequest, TResponse> :
    IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
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
    /// <param name="request">The request instance.</param>
    /// <param name="next">The next handler delegate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The handler response.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return await _corePipelineStep.Handle(request, async token => await next(token), cancellationToken);
    }
}
