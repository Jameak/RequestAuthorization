using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Exceptions;

/// <summary>
/// Represents an exception thrown when authorization fails.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    /// <summary>
    /// The authorization result associated with the failure.
    /// </summary>
    public RequestAuthorizationResult AuthResult { get; }

    /// <summary>
    /// Instantiates the exception
    /// </summary>
    public UnauthorizedException(RequestAuthorizationResult authResult) : base(authResult.FailureReason, authResult.FailureException)
    {
        AuthResult = authResult;
    }
}
