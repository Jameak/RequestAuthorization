using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Internal;

internal interface IRequestAuthorizationChecker<TRequest>
{
    Task<RequestAuthorizationResult> CheckAuthorization(TRequest request, CancellationToken token);
}
