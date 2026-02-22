using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal class CrashingRequirementHandler : RequestAuthorizationHandlerBase<CrashingRequirement>
{
    public const string CrashMessage = "I crashed while checking the requirement";

    public override async Task<RequestAuthorizationResult> CheckRequirementAsync(CrashingRequirement requirement, CancellationToken token)
    {
        throw new Exception(CrashMessage);
    }
}
