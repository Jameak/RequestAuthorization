using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Requirements;

/// <summary>
/// A sentinel requirement used when requirement building completes without producing any requirements.
/// </summary>
/// <remarks>
/// This type is used internally to represent a successful requirement build phase
/// that produced no authorization constraints.
/// </remarks>
public sealed class RequirementBuildingProducedNoRequirements : IRequestAuthorizationRequirement
{
    internal RequirementBuildingProducedNoRequirements()
    {
    }
}
