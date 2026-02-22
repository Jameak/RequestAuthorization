using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.GlobalBuilderTypesForAssemblyScanTests;

internal abstract class AbstractGlobalBuilder : IGlobalRequestAuthorizationRequirementBuilder
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync<TRequest>(TRequest request, CancellationToken token) => throw new NotImplementedException();
}
