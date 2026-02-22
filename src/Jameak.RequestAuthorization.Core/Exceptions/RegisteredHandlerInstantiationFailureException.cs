namespace Jameak.RequestAuthorization.Core.Exceptions;

/// <summary>
/// Represents an exception thrown when a requirement handler cannot be instantiated from the service provider.
/// </summary>
public sealed class RegisteredHandlerInstantiationFailureException : Exception
{
    /// <summary>
    /// Instantiates the exception
    /// </summary>
    public RegisteredHandlerInstantiationFailureException(string message, Exception inner) : base(message, inner)
    {
    }
}
