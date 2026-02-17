using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Diagnostics;

/// <summary>
/// Provides diagnostic information produced during authorization evaluation.
/// </summary>
public sealed class AuthorizationDiagnostic
{
    /// <summary>
    /// Gets the results of child requirements that were evaluated.
    /// </summary>
    public IReadOnlyList<RequestAuthorizationResult>? EvaluatedChildren { get; init; }

    /// <summary>
    /// Gets the child requirements that were skipped due to short-circuit evaluation.
    /// </summary>
    public IReadOnlyList<IRequestAuthorizationRequirement>? SkippedChildren { get; init; }
}
