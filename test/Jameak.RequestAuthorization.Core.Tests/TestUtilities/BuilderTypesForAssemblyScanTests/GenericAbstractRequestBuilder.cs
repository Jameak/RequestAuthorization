using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;

internal abstract class GenericAbstractRequestBuilder<T> : IRequestAuthorizationRequirementBuilder<T>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(T request, CancellationToken token) => throw new NotImplementedException();
}
