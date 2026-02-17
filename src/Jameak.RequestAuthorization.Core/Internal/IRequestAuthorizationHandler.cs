using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Internal;

internal interface IRequestAuthorizationHandler
{
    public Task<RequestAuthorizationResult> CheckRequirementAsync(
        IRequestAuthorizationRequirement requirement,
        CancellationToken token);
}
