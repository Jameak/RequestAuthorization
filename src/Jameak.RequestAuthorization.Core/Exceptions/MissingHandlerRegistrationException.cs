namespace Jameak.RequestAuthorization.Core.Exceptions;

/// <summary>
/// Represents an exception thrown when a requirement is checked for which no handlers are registered.
/// </summary>
public sealed class MissingHandlerRegistrationException : Exception
{
    /// <summary>
    /// Instantiates the exception
    /// </summary>
    public MissingHandlerRegistrationException(string message) : base(message)
    {
    }
}
