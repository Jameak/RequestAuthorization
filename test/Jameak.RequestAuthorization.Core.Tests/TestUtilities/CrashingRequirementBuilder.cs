using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal class CrashingRequirementBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new CrashingRequirement());
    }
}
