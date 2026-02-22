using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Execution;

internal interface IRequestAuthorizationHandler
{
    public Task<RequestAuthorizationResult> CheckRequirementAsync(
        IRequestAuthorizationRequirement requirement,
        CancellationToken token);
}
