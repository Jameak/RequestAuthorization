namespace Jameak.RequestAuthorization.Core.Exceptions;

/// <summary>
/// Represents an exception thrown when authorization handlers are registered incorrectly.
/// </summary>
public sealed class InvalidHandlerRegistrationException : Exception
{
    /// <summary>
    /// Instantiates the exception
    /// </summary>
    public InvalidHandlerRegistrationException(string message) : base(message)
    {
    }
}
