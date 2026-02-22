using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Results;

internal sealed class DefaultAuthorizedResultHandler : IAuthorizedResultHandler
{
    public Task OnAuthorized<TRequest>(
        TRequest message,
        RequestAuthorizationResult result,
        CancellationToken cancellationToken)
    {
        // Intentionally left blank.
        return Task.CompletedTask;
    }
}
