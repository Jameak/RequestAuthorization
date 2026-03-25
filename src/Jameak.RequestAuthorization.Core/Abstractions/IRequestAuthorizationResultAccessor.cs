using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Abstractions;

/// <summary>
/// Provides access to the <see cref="RequestAuthorizationResult"/> from the authorization evaluation pipeline, if one is available.
/// </summary>
public interface IRequestAuthorizationResultAccessor
{
    /// <summary>
    /// Gets or sets the current <see cref="RequestAuthorizationResult"/>.
    /// Returns <see langword="null" /> if there is no active <see cref="RequestAuthorizationResult"/>.
    /// </summary>
    public RequestAuthorizationResult? AuthorizationResult { get; set; }
}
