using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal class RequirementHandlerThatCrashesOnInstantiation : RequestAuthorizationHandlerBase<AlwaysSuccessRequirement>
{
    public const string CrashMessage = "I crashed while being instantiated";

    public RequirementHandlerThatCrashesOnInstantiation()
    {
        throw new Exception(CrashMessage);
    }

    public override Task<RequestAuthorizationResult> CheckRequirementAsync(AlwaysSuccessRequirement requirement, CancellationToken token) => throw new NotImplementedException("Should never reach this.");
}
