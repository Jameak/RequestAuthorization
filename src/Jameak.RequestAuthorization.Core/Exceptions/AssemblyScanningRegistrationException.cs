namespace Jameak.RequestAuthorization.Core.Exceptions;

/// <summary>
/// Represents an exception thrown during assembly scanning when invalid types are encountered.
/// This may be due to invalid types being declared in the assembly, or the given scanning input
/// types not being compatible with the declared types.
/// </summary>
public sealed class AssemblyScanningRegistrationException : Exception
{
    /// <summary>
    /// Instantiates the exception
    /// </summary>
    public AssemblyScanningRegistrationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Instantiates the exception
    /// </summary>
    public AssemblyScanningRegistrationException(string message, Exception inner) : base(message, inner)
    {
    }
}
