using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Internal;

namespace Jameak.RequestAuthorization.Core.Execution;

internal sealed class AuthorizationPipelineStep<TRequest, TResponse> : IAuthorizationPipelineStep<TRequest, TResponse>
{
    private readonly IRequestAuthorizationChecker<TRequest> _requestAuthorizationChecker;
    private readonly IAuthorizedResultHandler _authorizedResultHandler;
    private readonly IUnauthorizedResultHandler _unauthorizedResultHandler;

    public AuthorizationPipelineStep(
        IRequestAuthorizationChecker<TRequest> requestAuthorizationChecker,
        IAuthorizedResultHandler authorizedResultHandler,
        IUnauthorizedResultHandler unauthorizedResultHandler)
    {
        _requestAuthorizationChecker = requestAuthorizationChecker;
        _authorizedResultHandler = authorizedResultHandler;
        _unauthorizedResultHandler = unauthorizedResultHandler;
    }

    public async ValueTask<TResponse> Handle(
        TRequest message,
        Func<CancellationToken, Task<TResponse>> next,
        CancellationToken cancellationToken)
    {
        var authResult = await _requestAuthorizationChecker.CheckAuthorization(message, cancellationToken);

        if (!authResult.IsAuthorized)
        {
            return await _unauthorizedResultHandler.OnUnauthorized<TRequest, TResponse>(message, authResult);
        }

        _authorizedResultHandler.OnAuthorized(message, authResult);

        return await next(cancellationToken);
    }
}
