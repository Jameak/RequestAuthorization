using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.DerivedTypesForAssemblyScanTests;

internal class ClosedNonGenericBaseRequestBuilder : IRequestAuthorizationRequirementBuilder<INonGenericBaseRequest>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(INonGenericBaseRequest request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
    }
}
