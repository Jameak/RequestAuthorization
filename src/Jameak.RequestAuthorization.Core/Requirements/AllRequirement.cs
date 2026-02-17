using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Requirements;

/// <summary>
/// Represents a composite authorization requirement that succeeds when all of its child requirements succeeds.
/// </summary>
/// <remarks>
/// Evaluation uses short-circuit semantics. Evaluation stops as soon as a failing child requirement is encountered.
/// </remarks>
public sealed class AllRequirement : IRequestAuthorizationRequirement
{
    /// <summary>
    /// The child requirements that participate in the evaluation.
    /// </summary>
    public IReadOnlyList<IRequestAuthorizationRequirement> Requirements { get; init; }

    internal AllRequirement(IEnumerable<IRequestAuthorizationRequirement> requirements)
    {
        var reqArray = requirements.ToArray();
        if (reqArray.Length < 2)
        {
            throw new ArgumentException($"{nameof(AllRequirement)} requires at least two requirements.", nameof(requirements));
        }

        Requirements = reqArray;
    }
}

