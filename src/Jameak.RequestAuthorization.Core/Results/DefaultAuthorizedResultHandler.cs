using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Results;

internal sealed class DefaultAuthorizedResultHandler : IAuthorizedResultHandler
{
    public void OnAuthorized<TRequest>(TRequest message, RequestAuthorizationResult result)
    {
        // Intentionally left blank.
    }
}
