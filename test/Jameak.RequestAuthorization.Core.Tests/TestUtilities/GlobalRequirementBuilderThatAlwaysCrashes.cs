using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal class GlobalRequirementBuilderThatAlwaysCrashes : IGlobalRequestAuthorizationRequirementBuilder
{
    public const string CrashMessage = "I crashed while builder the global requirement";

    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync<TRequest>(TRequest request, CancellationToken token)
    {
        throw new Exception(CrashMessage);
    }
}
