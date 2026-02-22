using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.GlobalBuilderTypesForAssemblyScanTests;

internal class GlobalBuilderInheritingFromUserProvidedInterface : InterfaceGlobalBuilder
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync<TRequest>(TRequest request, CancellationToken token) => throw new NotImplementedException();
}
