using Jameak.RequestAuthorization.Core.Abstractions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Adapter.Mediator;

/// <summary>
/// Provides extensions for registering Mediator adapters.
/// </summary>
public static class HandlerRegistrationBuilderExtensions
{
    /// <summary>
    /// Registers authorization pipeline behaviors for Mediator.
    /// </summary>
    /// <remarks>
    /// Note that for NativeAOT usage, you instead need to reference the pipeline behavior types directly in the AddMediator call like so:
    /// <code>
    /// builder.Services.AddMediator(options =>
    /// {
    ///     options.PipelineBehaviors = [typeof(RequestAuthorizationPipelineBehavior&lt;,&gt;)];
    ///     options.StreamPipelineBehaviors = [typeof(RequestAuthorizationStreamPipelineBehavior&lt;,&gt;)];
    /// });
    /// </code>
    /// </remarks>
    public static IHandlerRegistrationBuilder AddMediatorPipeline(this IHandlerRegistrationBuilder builder)
    {
        builder.Services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestAuthorizationPipelineBehavior<,>), builder.ServiceLifetime));
        builder.Services.Add(new ServiceDescriptor(typeof(IStreamPipelineBehavior<,>), typeof(RequestAuthorizationStreamPipelineBehavior<,>), builder.ServiceLifetime));
        return builder;
    }
}
