namespace Jameak.RequestAuthorization.Core.Exceptions;

/// <summary>
/// Represents an exception thrown when requirement builders are registered incorrectly.
/// </summary>
public sealed class InvalidBuilderRegistrationException : Exception
{
    /// <summary>
    /// Instantiates the exception
    /// </summary>
    public InvalidBuilderRegistrationException(string message) : base(message)
    {
    }
}
