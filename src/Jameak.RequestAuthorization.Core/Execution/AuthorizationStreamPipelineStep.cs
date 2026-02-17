using System.Runtime.CompilerServices;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Internal;

namespace Jameak.RequestAuthorization.Core.Execution;

internal sealed class AuthorizationStreamPipelineStep<TRequest, TResponse> : IAuthorizationStreamPipelineStep<TRequest, TResponse>
{
    private readonly IRequestAuthorizationChecker<TRequest> _requestAuthorizationChecker;
    private readonly IAuthorizedResultHandler _authorizedResultHandler;
    private readonly IUnauthorizedResultHandler _unauthorizedResultHandler;

    public AuthorizationStreamPipelineStep(
        IRequestAuthorizationChecker<TRequest> requestAuthorizationChecker,
        IAuthorizedResultHandler authorizedResultHandler,
        IUnauthorizedResultHandler unauthorizedResultHandler)
    {
        _requestAuthorizationChecker = requestAuthorizationChecker;
        _authorizedResultHandler = authorizedResultHandler;
        _unauthorizedResultHandler = unauthorizedResultHandler;
    }

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest message,
        Func<CancellationToken, IAsyncEnumerable<TResponse>> next,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var authResult = await _requestAuthorizationChecker.CheckAuthorization(message, cancellationToken);

        if (!authResult.IsAuthorized)
        {
            yield return await _unauthorizedResultHandler.OnUnauthorizedStream<TRequest, TResponse>(message, authResult);
            yield break;
        }

        _authorizedResultHandler.OnAuthorized(message, authResult);

        await foreach (var response in next(cancellationToken).WithCancellation(cancellationToken))
        {
            yield return response;
        }
    }
}
