using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.HandlerTypesForAssemblyScanTests;

internal class HandlerTypeInheritingFromUserProvidedAbstract : AbstractRequestHandler
{
    public override Task<RequestAuthorizationResult> CheckRequirementAsync(AlwaysSuccessRequirement requirement, CancellationToken token) => throw new NotImplementedException();
}
