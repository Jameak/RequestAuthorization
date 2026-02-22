using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Execution;

internal interface IRequestAuthorizationChecker<TRequest>
{
    Task<RequestAuthorizationResult> CheckAuthorization(TRequest request, CancellationToken token);
}
