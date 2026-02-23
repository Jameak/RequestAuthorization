using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Configuration;
using Jameak.RequestAuthorization.Core.Execution;
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
    /// The lifetime used when registering
    /// <see cref="IRequestAuthorizationRequirementBuilder{TRequest}"/>,
    /// <see cref="IGlobalRequestAuthorizationRequirementBuilder"/>,
    /// <see cref="IAuthorizedResultHandler"/> and <see cref="IUnauthorizedResultHandler"/>,
    /// as well as some internal services. The default is <see cref="ServiceLifetime.Scoped"/>.
    /// </param>
    /// <returns>
    /// An <see cref="IHandlerRegistrationBuilder"/> used to configure handlers and integrations.
    /// </returns>
    /// <remarks>
    /// Note that request-handlers inheriting from <see cref="RequestAuthorizationHandlerBase{TRequirement}"/>
    /// and some internal services will be registered as scoped regardless of the <paramref name="serviceLifetime"/>,
    /// and wrapping the pipeline in a scope is therefore required no matter what <paramref name="serviceLifetime"/> is chosen.
    /// </remarks>
    public static IHandlerRegistrationBuilder AddRequestAuthorizationCore(
        this IServiceCollection serviceCollection,
        Action<RequestAuthorizationOptions>? configure = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        var coreRegistrationHasBeenCalledBeforeSentinel = serviceCollection.FirstOrDefault(e => e.ServiceType == typeof(IRequestAuthorizationExecutor));
        if (coreRegistrationHasBeenCalledBeforeSentinel != null && coreRegistrationHasBeenCalledBeforeSentinel.Lifetime != serviceLifetime)
        {
            throw new ArgumentException($"{nameof(AddRequestAuthorizationCore)} has been called multiple times with different '{nameof(serviceLifetime)}'-values. It must called with the same lifetime every time.", nameof(serviceLifetime));
        }

        var builder = new HandlerRegistrationBuilder(serviceCollection, serviceLifetime);
        builder.RegisterInternalsAndDefaults();
        builder.WithOptions(configure);
        return builder;
    }
}
