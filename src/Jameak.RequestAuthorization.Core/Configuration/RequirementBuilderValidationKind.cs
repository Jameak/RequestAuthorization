namespace Jameak.RequestAuthorization.Core.Configuration;

/// <summary>
/// Specifies how requirement builder registrations are validated.
/// </summary>
public enum RequirementBuilderValidationKind
{
    /// <summary>
    /// Allows zero or more builders to be registered.
    /// </summary>
    ZeroOrMoreBuilders,
    /// <summary>
    /// Requires at least one builder to be registered.
    /// </summary>
    AtLeastOneBuilder,
    /// <summary>
    /// Requires exactly one builder to be registered.
    /// </summary>
    ExactlyOneBuilder,
}
