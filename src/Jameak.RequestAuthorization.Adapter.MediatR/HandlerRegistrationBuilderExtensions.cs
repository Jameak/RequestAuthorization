using Jameak.RequestAuthorization.Core.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Adapter.MediatR;

/// <summary>
/// Provides extensions for registering MediatR adapters.
/// </summary>
public static class HandlerRegistrationBuilderExtensions
{
    /// <summary>
    /// Registers authorization pipeline behaviors for MediatR.
    /// </summary>
    /// <param name="builder">The registration builder.</param>
    /// <returns>The builder for chaining calls</returns>
    public static IHandlerRegistrationBuilder AddMediatRPipelineAdapter(this IHandlerRegistrationBuilder builder)
    {
        builder.Services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestAuthorizationPipelineBehavior<,>), builder.ServiceLifetime));
        builder.Services.Add(new ServiceDescriptor(typeof(IStreamPipelineBehavior<,>), typeof(RequestAuthorizationStreamPipelineBehavior<,>), builder.ServiceLifetime));
        return builder;
    }
}
