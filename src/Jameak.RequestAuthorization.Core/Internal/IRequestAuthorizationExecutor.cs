using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Internal;

internal interface IRequestAuthorizationExecutor
{
    Task<RequestAuthorizationResult> ExecuteAsync(
        IRequestAuthorizationRequirement requirement,
        CancellationToken token);
}
