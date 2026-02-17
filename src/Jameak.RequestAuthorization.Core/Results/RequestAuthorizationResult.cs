using System.Diagnostics;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Diagnostics;

namespace Jameak.RequestAuthorization.Core.Results;

/// <summary>
/// Represents the result of evaluating an authorization requirement.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class RequestAuthorizationResult
{
    /// <summary>
    /// Indicates whether the requirement was authorized.
    /// </summary>
    public required bool IsAuthorized { get; init; }

    /// <summary>
    /// The requirement that was evaluated.
    /// </summary>
    public required IRequestAuthorizationRequirement Requirement { get; init; }

    /// <summary>
    /// The failure reason when authorization fails.
    /// </summary>
    public required string? FailureReason { get; init; }

    /// <summary>
    /// The exception associated with the failure, if any.
    /// </summary>
    public required Exception? FailureException { get; init; }

    /// <summary>
    /// Optional contextual data associated with the evaluation.
    /// </summary>
    public required object? Context { get; init; }

    /// <summary>
    /// Diagnostic information produced during evaluation.
    /// </summary>
    public required AuthorizationDiagnostic? Diagnostic { get; init; }

    /// <summary>
    /// Creates a successful authorization result.
    /// </summary>
    /// <param name="requirement">The evaluated requirement.</param>
    /// <param name="diagnostic">Optional diagnostic information.</param>
    /// <param name="context">Optional contextual data.</param>
    /// <returns>A successful authorization result.</returns>
    public static RequestAuthorizationResult Success(
        IRequestAuthorizationRequirement requirement,
        AuthorizationDiagnostic? diagnostic = default,
        object? context = default)
    {
        return new RequestAuthorizationResult
        {
            IsAuthorized = true,
            Diagnostic = diagnostic,
            Requirement = requirement,
            FailureReason = null,
            FailureException = null,
            Context = context,
        };
    }

    /// <summary>
    /// Creates a failed authorization result.
    /// </summary>
    /// <param name="requirement">The evaluated requirement.</param>
    /// <param name="failureReason">An optional failure reason.</param>
    /// <param name="failureException">An optional exception associated with the failure.</param>
    /// <param name="diagnostic">Optional diagnostic information.</param>
    /// <param name="context">Optional contextual data.</param>
    /// <returns>A failed <see cref="RequestAuthorizationResult"/>.</returns>
    public static RequestAuthorizationResult Fail(
        IRequestAuthorizationRequirement requirement,
        string? failureReason = default,
        Exception? failureException = default,
        AuthorizationDiagnostic? diagnostic = default,
        object? context = default)
    {
        return new RequestAuthorizationResult
        {
            IsAuthorized = false,
            Diagnostic = diagnostic,
            Requirement = requirement,
            FailureReason = failureReason,
            FailureException = failureException,
            Context = context,
        };
    }

    private string DebuggerDisplay
        => IsAuthorized
            ? $"Authorized ({Requirement?.GetType().Name})"
            : $"Denied ({Requirement?.GetType().Name}) - {FailureReason ?? FailureException?.Message}";

}
