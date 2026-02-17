using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Requirements;

/// <summary>
/// A sentinel requirement used when requirement building fails due to an exception.
/// </summary>
/// <remarks>
/// This type is used internally to return builder failures through the authorization pipeline.
/// </remarks>
public sealed class RequirementBuildingMustNotThrowExceptionRequirement : IRequestAuthorizationRequirement
{
    internal RequirementBuildingMustNotThrowExceptionRequirement()
    {
    }
}
