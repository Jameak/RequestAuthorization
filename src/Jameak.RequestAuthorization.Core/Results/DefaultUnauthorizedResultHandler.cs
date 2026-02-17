using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Exceptions;

namespace Jameak.RequestAuthorization.Core.Results;

internal sealed class DefaultUnauthorizedResultHandler : IUnauthorizedResultHandler
{
    public Task<TResponse> OnUnauthorized<TRequest, TResponse>(
        TRequest message,
        RequestAuthorizationResult authResult)
        => throw new UnauthorizedException(authResult);

    public Task<TResponse> OnUnauthorizedStream<TRequest, TResponse>(
        TRequest message,
        RequestAuthorizationResult authResult)
        => throw new UnauthorizedException(authResult);
}
