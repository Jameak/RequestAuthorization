using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Execution;

internal interface IRequestAuthorizationExecutor
{
    Task<RequestAuthorizationResult> ExecuteAsync(
        IRequestAuthorizationRequirement requirement,
        CancellationToken token);
}
