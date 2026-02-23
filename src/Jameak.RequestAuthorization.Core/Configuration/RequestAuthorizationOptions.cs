using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Configuration;

/// <summary>
/// Provides configuration options for request authorization.
/// </summary>
public sealed class RequestAuthorizationOptions
{
    /// <summary>
    /// Gets or sets the validation behavior applied to registered
    /// <see cref="IRequestAuthorizationRequirementBuilder{TRequest}"/> implementations.
    /// </summary>
    public RequirementBuilderValidationKind RequirementBuilderValidation { get; set; } = RequirementBuilderValidationKind.ExactlyOneBuilder;

    internal RequestAuthorizationOptions Clone()
    {
        return (RequestAuthorizationOptions)MemberwiseClone();
    }
}
