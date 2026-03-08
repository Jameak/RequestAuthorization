using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.DerivedTypesForAssemblyScanTests;

internal class OpenGenericBuilderWithoutRequestConstraint<T> : IRequestAuthorizationRequirementBuilder<T>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(T request, CancellationToken token) => throw new NotImplementedException();
}
