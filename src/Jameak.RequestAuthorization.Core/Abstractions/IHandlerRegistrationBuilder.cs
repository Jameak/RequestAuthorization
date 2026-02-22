using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Core.Abstractions;

/// <summary>
/// Defines a builder for registering request authorization handlers and builders.
/// </summary>
public interface IHandlerRegistrationBuilder
{
    /// <summary>
    /// Gets the underlying service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Gets the service lifetime used for registrations.
    /// </summary>
    ServiceLifetime ServiceLifetime { get; }

    /// <summary>
    /// Registers a requirement handler type.
    /// </summary>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <typeparam name="TRequirement">The requirement type.</typeparam>
    /// <returns>The builder for chaining further calls.</returns>
    IHandlerRegistrationBuilder AddRequirementHandlerType<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler, TRequirement>()
        where THandler : RequestAuthorizationHandlerBase<TRequirement>
        where TRequirement : IRequestAuthorizationRequirement;

    /// <summary>
    /// Registers a requirement handler type.
    /// </summary>
    /// <param name="handlerType">The handler type. Must inherit from <see cref="RequestAuthorizationHandlerBase{TRequirement}"/> where TRequirement matches '<paramref name="requirementType"/>'</param>
    /// <param name="requirementType">The requirement type. Must implement <see cref="IRequestAuthorizationRequirement"/></param>
    /// <returns>The builder for chaining further calls.</returns>
    IHandlerRegistrationBuilder AddRequirementHandlerType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType,
        Type requirementType);

    /// <summary>
    /// Registers a requirement builder type for a request type.
    /// </summary>
    /// <typeparam name="TBuilder">The builder type.</typeparam>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <returns>The builder for chaining further calls.</returns>
    IHandlerRegistrationBuilder AddRequirementBuilderType<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TBuilder, TRequest>()
        where TBuilder : class, IRequestAuthorizationRequirementBuilder<TRequest>;

    /// <summary>
    /// Registers a requirement builder type for a request type.
    /// </summary>
    /// <param name="builderType">The builder type. Must implement <see cref="IRequestAuthorizationRequirementBuilder{TRequest}"/> where TRequest matches '<paramref name="requestType"/>'</param>
    /// <param name="requestType">The request type.</param>
    /// <returns>The builder for chaining further calls.</returns>
    [RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
    IHandlerRegistrationBuilder AddRequirementBuilderType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces)] Type builderType,
        Type requestType);

    /// <summary>
    /// Registers a global requirement builder type.
    /// </summary>
    /// <typeparam name="TGlobalHandler">The global requirement builder type.</typeparam>
    /// <returns>The builder for chaining further calls.</returns>
    IHandlerRegistrationBuilder AddGlobalRequirementBuilderType<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TGlobalHandler>()
        where TGlobalHandler : class, IGlobalRequestAuthorizationRequirementBuilder;

    /// <summary>
    /// Registers a global requirement builder type.
    /// </summary>
    /// <param name="globalHandlerType">The global requirement builder type. Must implement <see cref="IGlobalRequestAuthorizationRequirementBuilder"/>.</param>
    /// <returns>The builder for chaining further calls.</returns>
    IHandlerRegistrationBuilder AddGlobalRequirementBuilderType
        ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type globalHandlerType);

    /// <summary>
    /// Registers requirement handler types discovered in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The builder for chaining further calls.</returns>
    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    IHandlerRegistrationBuilder AddRequirementHandlerTypesFromAssembly(Assembly assembly);

    /// <summary>
    /// Registers requirement builder types discovered in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The builder for chaining further calls.</returns>

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes() and type.GetInterfaces()")]
    IHandlerRegistrationBuilder AddRequirementBuilderTypesFromAssembly(Assembly assembly);

    /// <summary>
    /// Registers global requirement builder types discovered in the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The builder for chaining further calls.</returns>

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    IHandlerRegistrationBuilder AddGlobalRequirementBuilderTypesFromAssembly(Assembly assembly);

    /// <summary>
    /// Registers a custom global unauthorized result handler.
    /// </summary>
    /// <typeparam name="THandler">The handler type</typeparam>
    /// <returns>The builder for chaining further calls.</returns>
    /// <remarks>Only a single unauthorized result handler can be registered.</remarks>
    public IHandlerRegistrationBuilder WithUnauthorizedResultHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>()
        where THandler : class, IUnauthorizedResultHandler;

    /// <summary>
    /// Registers a custom global authorized result handler.
    /// </summary>
    /// <typeparam name="THandler">The handler type</typeparam>
    /// <returns>The builder for chaining further calls.</returns>
    /// <remarks>Only a single authorized result handler can be registered.</remarks>
    public IHandlerRegistrationBuilder WithAuthorizedResultHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>()
        where THandler : class, IAuthorizedResultHandler;
}
