using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Adapter.AspNetCore;

/// <summary>
/// Provides extensions for registering ASP.NET Core adapters.
/// </summary>
public static class HandlerRegistrationBuilderExtensions
{
    /// <summary>
    /// Registers handlers that integrate with ASP.NET Core authorization.
    /// </summary>
    /// <param name="builder">The registration builder.</param>
    /// <returns>The builder for chaining calls</returns>
    public static IHandlerRegistrationBuilder AddAspNetAuthorizationAdapter(this IHandlerRegistrationBuilder builder)
    {
        builder.AddRequirementHandlerType<AspNetAuthorizationRequirementHandler, AspNetAuthorizationRequirement>();
        builder.AddRequirementHandlerType<AspNetAuthorizationPolicyRequirementHandler, AspNetAuthorizationPolicyRequirement>();
        return builder;
    }
}
