using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Requirements;

/// <summary>
/// Represents a composite authorization requirement that succeeds when at least one of its child requirements succeeds.
/// </summary>
/// <remarks>
/// Evaluation uses short-circuit semantics. Evaluation stops as soon as a successful child requirement is encountered.
/// </remarks>
public sealed class AnyRequirement : IRequestAuthorizationRequirement
{
    /// <summary>
    /// The child requirements that participate in the evaluation.
    /// </summary>
    public IReadOnlyList<IRequestAuthorizationRequirement> Requirements { get; init; }

    internal AnyRequirement(IEnumerable<IRequestAuthorizationRequirement> requirements)
    {
        var reqArray = requirements.ToArray();
        if (reqArray.Length < 2)
        {
            throw new ArgumentException($"{nameof(AnyRequirement)} requires at least two requirements.", nameof(requirements));
        }

        Requirements = reqArray;
    }
}
