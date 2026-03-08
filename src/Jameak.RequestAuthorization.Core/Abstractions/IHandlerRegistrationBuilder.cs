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
    /// Scans the specified assembly for concrete request types that
    /// derive from <paramref name="requestBaseType"/>, and registers the specified
    /// <paramref name="builderType"/> as the corresponding
    /// <see cref="IRequestAuthorizationRequirementBuilder{TRequest}"/>
    /// for each request type discovered in the assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method enables registering a single requirement builder for an entire
    /// request hierarchy. For example, a builder targeting ICustomerRequest&lt;TResponse&gt;
    /// can be automatically applied to all concrete request types implementing that interface.
    /// </para>
    /// <para>
    /// The <paramref name="builderType"/> may be:
    /// <list type="bullet">
    /// <item>
    /// An open generic type (e.g. CustomerRequestBuilder&lt;&gt;), in which case
    /// it will be closed using the generic arguments inferred from each discovered request.
    /// </item>
    /// <item>
    /// A closed concrete type, in which case the request must also be a closed type.
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Only non-abstract, non-interface, non-open derived request types from the assembly are considered.
    /// </para>
    /// </remarks>
    /// <param name="builderType">The builder type to register. Must implement
    /// <see cref="IRequestAuthorizationRequirementBuilder{TRequest}"/>. May be an open generic type or a closed type.
    /// </param>
    /// <param name="requestBaseType">
    /// The base request type that discovered request types must derive from.
    /// </param>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="throwWhenNoValidTypesFound">Controls whether an exception should be thrown when
    /// no valid types to register are found in the assembly. Defaults to true.</param>
    /// <returns>The builder for chaining further calls.</returns>
    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    [RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
    public IHandlerRegistrationBuilder AddRequirementBuilderTypeForDerivedRequestsFromAssembly(
        Type builderType,
        Type requestBaseType,
        Assembly assembly,
        bool throwWhenNoValidTypesFound = true);

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
    /// Only non-abstract, non-interface, non-open types deriving from
    /// <see cref="RequestAuthorizationHandlerBase{Object}"/> in the assembly are considered.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="throwWhenNoValidTypesFound">Controls whether an exception should be thrown when
    /// no valid types to register are found in the assembly. Defaults to true.</param>
    /// <returns>The builder for chaining further calls.</returns>
    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    IHandlerRegistrationBuilder AddRequirementHandlerTypesFromAssembly(
        Assembly assembly,
        bool throwWhenNoValidTypesFound = true);

    /// <summary>
    /// Registers requirement builder types discovered in the specified assembly.
    /// Only non-abstract, non-interface, non-open types deriving from
    /// <see cref="IRequestAuthorizationRequirementBuilder{Object}"/> in the assembly are considered.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="throwWhenNoValidTypesFound">Controls whether an exception should be thrown when
    /// no valid types to register are found in the assembly. Defaults to true.</param>
    /// <returns>The builder for chaining further calls.</returns>

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes() and type.GetInterfaces()")]
    IHandlerRegistrationBuilder AddRequirementBuilderTypesFromAssembly(
        Assembly assembly,
        bool throwWhenNoValidTypesFound = true);

    /// <summary>
    /// Registers global requirement builder types discovered in the specified assembly.
    /// Only non-abstract, non-interface, non-open types deriving from
    /// <see cref="IGlobalRequestAuthorizationRequirementBuilder"/> in the assembly are considered.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="throwWhenNoValidTypesFound">Controls whether an exception should be thrown when
    /// no valid types to register are found in the assembly. Defaults to true.</param>
    /// <returns>The builder for chaining further calls.</returns>

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    IHandlerRegistrationBuilder AddGlobalRequirementBuilderTypesFromAssembly(
        Assembly assembly,
        bool throwWhenNoValidTypesFound = true);

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
