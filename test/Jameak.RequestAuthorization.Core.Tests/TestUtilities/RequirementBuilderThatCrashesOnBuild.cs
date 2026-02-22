using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal class RequirementBuilderThatCrashesOnBuild : IRequestAuthorizationRequirementBuilder<TestRequest>
{
    public const string CrashMessage = "I crashed while building the requirements";

    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token) => throw new Exception(CrashMessage);
}
