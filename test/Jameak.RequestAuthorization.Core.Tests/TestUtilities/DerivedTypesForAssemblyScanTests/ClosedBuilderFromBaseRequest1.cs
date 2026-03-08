using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.DerivedTypesForAssemblyScanTests;

internal class ClosedBuilderFromBaseRequest1 : IRequestAuthorizationRequirementBuilder<IBaseRequest1<UserDataRequest1>>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(IBaseRequest1<UserDataRequest1> request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
    }
}
