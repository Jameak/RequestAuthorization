using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Core.DependencyInjection;

/// <summary>
/// Provides extension methods for registering request authorization services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core request authorization services to the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <param name="configure">
    /// An optional delegate to configure <see cref="RequestAuthorizationOptions"/>.
    /// </param>
    /// <param name="serviceLifetime">
    /// The lifetime used when registering authorization services.
    /// The default is <see cref="ServiceLifetime.Scoped"/>.
    /// </param>
    /// <returns>
    /// An <see cref="IHandlerRegistrationBuilder"/> used to configure handlers and integrations.
    /// </returns>
    public static IHandlerRegistrationBuilder AddRequestAuthorizationCore(
        this IServiceCollection serviceCollection,
        Action<RequestAuthorizationOptions>? configure = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var builder = new HandlerRegistrationBuilder(serviceCollection, serviceLifetime);
        builder.RegisterInternalsAndDefaults();
        builder.WithOptions(configure);
        return builder;
    }
}
