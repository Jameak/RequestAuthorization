using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Exceptions;

/// <summary>
/// Represents an exception thrown when a circular requirement graph is encountered.
/// </summary>
public sealed class CircularRequirementException : Exception
{
    /// <summary>
    /// The requirement that caused the circular graph
    /// </summary>
    public IRequestAuthorizationRequirement Requirement { get; }

    /// <summary>
    /// Instantiates the exception
    /// </summary>
    public CircularRequirementException(IRequestAuthorizationRequirement requirement) : base("Circular requirement detected")
    {
        Requirement = requirement;
    }
}
