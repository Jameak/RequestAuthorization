using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.HandlerTypesForAssemblyScanTests;

internal class HandlerTypeInheritingFromUserProvidedGenericAbstract : GenericAbstractRequestHandler<AlwaysSuccessRequirement>
{
    public override Task<RequestAuthorizationResult> CheckRequirementAsync(AlwaysSuccessRequirement requirement, CancellationToken token) => throw new NotImplementedException();
}
