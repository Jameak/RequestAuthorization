using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Configuration;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jameak.RequestAuthorization.Core.DependencyInjection;

internal sealed class HandlerRegistrationBuilder : IHandlerRegistrationBuilder
{
    private static readonly Type s_handlerInterface = typeof(IRequestAuthorizationHandler);
    private static readonly Type s_builderInterfaceType = typeof(IRequestAuthorizationRequirementBuilder<>);
    private static readonly Type s_globalBuilderInterfaceType = typeof(IGlobalRequestAuthorizationRequirementBuilder);

    public IServiceCollection Services { get; }
    public ServiceLifetime ServiceLifetime { get; }

    public HandlerRegistrationBuilder(IServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
    {
        Services = serviceCollection;
        ServiceLifetime = serviceLifetime;
    }

    internal void RegisterInternalsAndDefaults()
    {
        RegisterWithLifetime<IRequestAuthorizationExecutor, RequestAuthorizationExecutor>();
        Services.AddSingleton<AuthorizationHandlerRegistry>();
        Services.AddSingleton<IRequestAuthorizationResultAccessor, RequestAuthorizationResultAccessor>();
        AddRequirementHandlerType<AllRequirementHandler, AllRequirement>();
        AddRequirementHandlerType<AnyRequirementHandler, AnyRequirement>();
        RegisterWithLifetime(typeof(IAuthorizationPipelineStep<,>), typeof(AuthorizationPipelineStep<,>));
        RegisterWithLifetime(typeof(IAuthorizationStreamPipelineStep<,>), typeof(AuthorizationStreamPipelineStep<,>));
        RegisterWithLifetime(typeof(IRequestAuthorizationChecker<>), typeof(RequestAuthorizationChecker<>));

        // These are intended to be overwrite-able by library users
        Services.TryAddSingleton<IUnauthorizedResultHandler, DefaultUnauthorizedResultHandler>();
        Services.TryAddSingleton<IAuthorizedResultHandler, DefaultAuthorizedResultHandler>();
    }

    internal void WithOptions(Action<RequestAuthorizationOptions>? configure)
    {
        var options = new RequestAuthorizationOptions();
        configure?.Invoke(options);
        Services.AddTransient(provider => options.Clone());
    }

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    public IHandlerRegistrationBuilder AddRequirementHandlerTypesFromAssembly(Assembly assembly, bool throwWhenNoValidTypesFound = true)
    {
        var handlerTuples = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                !t.IsInterface &&
                !t.IsGenericTypeDefinition &&
                s_handlerInterface.IsAssignableFrom(t))
            .Select(type => (
                handlerType: type,
                requirementType: GetBaseTypes(type)
                    .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(RequestAuthorizationHandlerBase<>))
                    .GetGenericArguments()[0]))
            .ToList();

        var invalidDeclarations = new List<(Type handlerType, Type requirementType)>();
        foreach (var tuple in handlerTuples)
        {
            if (tuple.requirementType == typeof(IRequestAuthorizationRequirement))
            {
                invalidDeclarations.Add(tuple);
            }

            RegisterWithLifetime(tuple.handlerType, tuple.handlerType);
        }

        if (invalidDeclarations.Count > 0)
        {
            throw new InvalidHandlerRegistrationException($"Assembly contains invalid handler types. These handler types implement {nameof(RequestAuthorizationHandlerBase<>)} with the generic type {nameof(IRequestAuthorizationRequirement)}. The generic type must inherit from this interface-type instead.\n{string.Join('\n', invalidDeclarations.Select(e => e.handlerType.FullName))}");
        }

        if (throwWhenNoValidTypesFound && handlerTuples.Count == 0)
        {
            throw new ArgumentException($"Assembly contains 0 non-abstract, non-interface, non-open types deriving from '{typeof(RequestAuthorizationHandlerBase<>)}<T>'", nameof(assembly));
        }

        Services.AddSingleton(new AuthorizationHandlerRegistrar(handlerTuples));
        return this;
    }

    [SuppressMessage("Usage", "MA0015:Specify the parameter name in ArgumentException", Justification = "Argument exceptions related to generic params")]
    [SuppressMessage("Usage", "S3928:Parameter names used into ArgumentException constructors should match an existing one", Justification = "Argument exceptions related to generic params")]
    public IHandlerRegistrationBuilder AddRequirementHandlerType<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler,
        TRequirement>()
        where THandler : RequestAuthorizationHandlerBase<TRequirement>
        where TRequirement : IRequestAuthorizationRequirement
    {
        if (typeof(TRequirement) == typeof(IRequestAuthorizationRequirement))
        {
            throw new ArgumentException($"Generic argument '{nameof(TRequirement)}' must inherit from {nameof(IRequestAuthorizationRequirement)}.", nameof(TRequirement));
        }

        ThrowIfAbstractOrInterfaceOrOpenGeneric(typeof(THandler), nameof(THandler));
        RegisterWithLifetime<THandler, THandler>();
        Services.AddSingleton(new AuthorizationHandlerRegistrar([(typeof(THandler), typeof(TRequirement))]));

        return this;
    }

    public IHandlerRegistrationBuilder AddRequirementHandlerType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType,
        Type requirementType)
    {
        if (requirementType == typeof(IRequestAuthorizationRequirement) || !requirementType.IsAssignableTo(typeof(IRequestAuthorizationRequirement)))
        {
            throw new ArgumentException($"Type-argument '{nameof(requirementType)}' type '{requirementType}' must inherit from {nameof(IRequestAuthorizationRequirement)}.", nameof(requirementType));
        }

        var handlerHandlesRequirement = GetBaseTypes(handlerType)
            .SingleOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(RequestAuthorizationHandlerBase<>))
            ?.GetGenericArguments()[0];

        if (handlerHandlesRequirement == null || handlerHandlesRequirement != requirementType)
        {
            throw new ArgumentException($"Type-argument '{nameof(handlerType)}' must inherit from {nameof(RequestAuthorizationHandlerBase<>)}<T> and T must match the type-argument '{nameof(requirementType)}'.", nameof(handlerType));
        }

        ThrowIfAbstractOrInterfaceOrOpenGeneric(handlerType, nameof(handlerType));
        RegisterWithLifetime(handlerType, handlerType);
        Services.AddSingleton(new AuthorizationHandlerRegistrar([(handlerType, requirementType)]));

        return this;
    }

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes() and type.GetInterfaces()")]
    public IHandlerRegistrationBuilder AddRequirementBuilderTypesFromAssembly(Assembly assembly, bool throwWhenNoValidTypesFound = true)
    {
        var foundAny = false;
        foreach (var type in assembly.GetTypes().Where(t =>
                !t.IsAbstract &&
                !t.IsInterface &&
                !t.IsGenericTypeDefinition))
        {
            var implementedBuilderInterfaces = type
                .GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == s_builderInterfaceType);

            foreach (var serviceType in implementedBuilderInterfaces)
            {
                RegisterWithLifetime(serviceType, type);
                foundAny = true;
            }
        }

        if (throwWhenNoValidTypesFound && !foundAny)
        {
            throw new ArgumentException($"Assembly contains 0 non-abstract, non-interface, non-open types deriving from '{s_builderInterfaceType}'", nameof(assembly));
        }

        return this;
    }

    public IHandlerRegistrationBuilder AddRequirementBuilderType<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TBuilder,
        TRequest>()
        where TBuilder : class, IRequestAuthorizationRequirementBuilder<TRequest>
    {
        ThrowIfAbstractOrInterfaceOrOpenGeneric(typeof(TBuilder), nameof(TBuilder));
        RegisterWithLifetime<IRequestAuthorizationRequirementBuilder<TRequest>, TBuilder>();
        return this;
    }

    [RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
    public IHandlerRegistrationBuilder AddRequirementBuilderType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.Interfaces)] Type builderType,
        Type requestType)
    {
        var builderHandlesRequestType = builderType.GetInterfaces()
            .Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == s_builderInterfaceType &&
                i.GetGenericArguments()[0].IsAssignableFrom(requestType))
            .ToList();

        if (builderHandlesRequestType.Count == 0)
        {
            throw new ArgumentException($"Type-argument '{nameof(builderType)}' type '{builderType}' must implement {nameof(IRequestAuthorizationRequirementBuilder<>)}<T> where T is assignable from the type-argument '{nameof(requestType)}' type '{requestType}'.", nameof(builderType));
        }

        // builderHandlesRequestType.Count may be > 1. That is ok, we simply register with exactly the request type the user specified.
        ThrowIfAbstractOrInterfaceOrOpenGeneric(builderType, nameof(builderType));
        RegisterWithLifetime(typeof(IRequestAuthorizationRequirementBuilder<>).MakeGenericType(requestType), builderType);
        return this;
    }

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    [RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
    public IHandlerRegistrationBuilder AddRequirementBuilderTypeForDerivedRequestsFromAssembly(
        Type builderType,
        Type requestBaseType,
        Assembly assembly,
        bool throwWhenNoValidTypesFound = true)
    {
        if (!builderType.IsGenericTypeDefinition && requestBaseType.IsGenericTypeDefinition)
        {
            throw new ArgumentException($"Closed builder '{builderType}' requires a closed request base type.", nameof(requestBaseType));
        }

        if (builderType.IsGenericTypeDefinition && !requestBaseType.IsGenericTypeDefinition)
        {
            throw new ArgumentException($"Open generic builder '{builderType}' requires an open generic request base type.", nameof(requestBaseType));
        }

        if (builderType.IsAbstract || builderType.IsInterface)
        {
            throw new ArgumentException($"Builder type '{builderType}' must be a concrete class.", nameof(builderType));
        }

        var builderInterfaces = builderType
            .GetInterfaces()
            .Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == s_builderInterfaceType)
            .ToList();

        if (builderInterfaces.Count == 0)
        {
            throw new ArgumentException($"Builder type '{builderType}' does not implement {nameof(IRequestAuthorizationRequirementBuilder<>)}<T>.", nameof(builderType));
        }

        var requestTypes = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                !t.IsInterface &&
                !t.IsGenericTypeDefinition &&
                IsAssignableToRequestBaseType(t, requestBaseType))
            .ToList();

        if (throwWhenNoValidTypesFound && requestTypes.Count == 0)
        {
            throw new ArgumentException($"Assembly contains 0 non-abstract, non-interface, non-open types deriving from request-type '{requestBaseType}'", nameof(requestBaseType));
        }

        foreach (var requestType in requestTypes)
        {
            var concreteBuilderType = CloseBuilderTypeIfNeeded(builderType, requestBaseType, requestType);

            ValidateBuilderForRequest(concreteBuilderType, requestType);
            AddRequirementBuilderType(concreteBuilderType, requestType);
        }

        return this;
    }

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    public IHandlerRegistrationBuilder AddGlobalRequirementBuilderTypesFromAssembly(Assembly assembly, bool throwWhenNoValidTypesFound = true)
    {
        var foundAny = false;
        foreach (var type in assembly.GetTypes()
            .Where(t => !t.IsAbstract &&
                !t.IsInterface &&
                !t.IsGenericTypeDefinition &&
                s_globalBuilderInterfaceType.IsAssignableFrom(t)))
        {
            RegisterWithLifetime(s_globalBuilderInterfaceType, type);
            foundAny = true;
        }

        if (throwWhenNoValidTypesFound && !foundAny)
        {
            throw new ArgumentException($"Assembly contains 0 non-abstract, non-interface, non-open types deriving from '{s_globalBuilderInterfaceType}'", nameof(assembly));
        }

        return this;
    }

    public IHandlerRegistrationBuilder AddGlobalRequirementBuilderType<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TGlobalHandler>()
        where TGlobalHandler : class, IGlobalRequestAuthorizationRequirementBuilder
    {
        ThrowIfAbstractOrInterfaceOrOpenGeneric(typeof(TGlobalHandler), nameof(TGlobalHandler));
        RegisterWithLifetime<IGlobalRequestAuthorizationRequirementBuilder, TGlobalHandler>();
        return this;
    }

    public IHandlerRegistrationBuilder AddGlobalRequirementBuilderType
        ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type globalHandlerType)
    {
        if (!globalHandlerType.IsAssignableTo(s_globalBuilderInterfaceType))
        {
            throw new ArgumentException($"Type-argument '{nameof(globalHandlerType)}' must implement {nameof(IGlobalRequestAuthorizationRequirementBuilder)}.", nameof(globalHandlerType));
        }

        ThrowIfAbstractOrInterfaceOrOpenGeneric(globalHandlerType, nameof(globalHandlerType));
        RegisterWithLifetime(typeof(IGlobalRequestAuthorizationRequirementBuilder), globalHandlerType);
        return this;
    }

    public IHandlerRegistrationBuilder WithUnauthorizedResultHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>() where THandler : class, IUnauthorizedResultHandler
    {
        ThrowIfAbstractOrInterfaceOrOpenGeneric(typeof(THandler), nameof(THandler));
        RegisterWithLifetime<IUnauthorizedResultHandler, THandler>();
        return this;
    }

    public IHandlerRegistrationBuilder WithAuthorizedResultHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>() where THandler : class, IAuthorizedResultHandler
    {
        ThrowIfAbstractOrInterfaceOrOpenGeneric(typeof(THandler), nameof(THandler));
        RegisterWithLifetime<IAuthorizedResultHandler, THandler>();
        return this;
    }

    private void RegisterWithLifetime(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
    {
        Services.Add(new ServiceDescriptor(serviceType, implementationType, ServiceLifetime));
    }

    private void RegisterWithLifetime<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>() where TService : class where TImplementation : class, TService
    {
        RegisterWithLifetime(typeof(TService), typeof(TImplementation));
    }

    private static void ThrowIfAbstractOrInterfaceOrOpenGeneric(Type type, string paramName)
    {

        if (type.IsInterface)
        {
            throw new ArgumentException($"Type-argument '{paramName}' cannot be interface.", paramName);
        }

        if (type.IsAbstract)
        {
            throw new ArgumentException($"Type-argument '{paramName}' cannot be abstract class.", paramName);
        }

        if (type.IsGenericTypeDefinition)
        {
            throw new ArgumentException($"Type-argument '{paramName}' cannot be open-generic class.", paramName);
        }
    }

    private static IEnumerable<Type> GetBaseTypes(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            yield return baseType;
            baseType = baseType.BaseType;
        }
    }

    private static bool IsAssignableToRequestBaseType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type candidate,
        Type requestBaseType)
    {
        if (requestBaseType.IsGenericTypeDefinition)
        {
            return candidate
                .GetInterfaces()
                .Concat(GetBaseTypes(candidate))
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == requestBaseType);
        }

        return requestBaseType.IsAssignableFrom(candidate);
    }

    [RequiresDynamicCode("Calls System.Type.MakeGenericType(params Type[])")]
    [RequiresUnreferencedCode("Calls System.Type.MakeGenericType(params Type[])")]
    private static Type CloseBuilderTypeIfNeeded(
        Type builderType,
        Type requestBaseType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type concreteRequestType)
    {
        if (!builderType.IsGenericTypeDefinition)
        {
            return builderType;
        }

        var matchingDerivedTypes = concreteRequestType
            .GetInterfaces()
            .Concat(GetBaseTypes(concreteRequestType))
            .Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == requestBaseType)
            .ToList();

        if (matchingDerivedTypes.Count == 0)
        {
            throw new AssemblyScanningRegistrationException($"Could not find a closed generic implementation of '{requestBaseType}' on '{concreteRequestType}'.");
        }

        if (matchingDerivedTypes.Count > 1)
        {
            throw new AssemblyScanningRegistrationException($"Found multiple closed generic implementations of '{requestBaseType}' on '{concreteRequestType}'. This is not supported.");
        }

        var genericArgs = matchingDerivedTypes.Single().GetGenericArguments();

        try
        {
            return builderType.MakeGenericType(genericArgs);
        }
        catch (Exception ex)
        {
            throw new AssemblyScanningRegistrationException($"Failed to close the open builder type '{builderType}' with generic arguments from '{concreteRequestType}' found via '{requestBaseType}'. All types deriving from '{requestBaseType}' must be valid for the given builder type.", ex);
        }
    }

    private static void ValidateBuilderForRequest(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type concreteBuilderType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type requestType)
    {
        var concreteBuilderInterfaces = concreteBuilderType
            .GetInterfaces()
            .Where(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == s_builderInterfaceType)
            .ToList();

        var matchesRequest = concreteBuilderInterfaces.Exists(i =>
        {
            var builderRequestType = i.GetGenericArguments()[0];
            return builderRequestType.IsAssignableFrom(requestType);
        });

        if (!matchesRequest)
        {
            throw new AssemblyScanningRegistrationException($"Concrete builder type '{concreteBuilderType}' is not assignable from {nameof(IRequestAuthorizationRequirementBuilder<>)}<{requestType.FullName ?? requestType.Name}>.");
        }
    }

    [SuppressMessage("Usage", "MA0015:Specify the parameter name in ArgumentException", Justification = "Argument exceptions related to generic params")]
    [SuppressMessage("Usage", "S3928:Parameter names used into ArgumentException constructors should match an existing one", Justification = "Argument exceptions related to generic params")]
    public IHandlerRegistrationBuilder AddRequirementHandlerDelegate<TRequirement>(Func<IServiceProvider, TRequirement, Task<RequestAuthorizationResult>> handler)
        where TRequirement : IRequestAuthorizationRequirement
    {
        if (typeof(TRequirement) == typeof(IRequestAuthorizationRequirement))
        {
            throw new ArgumentException($"Generic argument '{nameof(TRequirement)}' must inherit from {nameof(IRequestAuthorizationRequirement)}.", nameof(TRequirement));
        }

        FuncRequirementHandler<TRequirement> Factory(IServiceProvider sp) => new(sp, handler);
        Services.Add(new ServiceDescriptor(typeof(FuncRequirementHandler<TRequirement>), Factory, ServiceLifetime));
        Services.AddSingleton(new AuthorizationHandlerRegistrar([(typeof(FuncRequirementHandler<TRequirement>), typeof(TRequirement))]));

        return this;
    }

    public IHandlerRegistrationBuilder AddRequirementHandlerDelegate<TRequirement>(Func<TRequirement, Task<RequestAuthorizationResult>> handler)
        where TRequirement : IRequestAuthorizationRequirement
    {
        return AddRequirementHandlerDelegate<TRequirement>((_, requirement) => handler(requirement));
    }

    public IHandlerRegistrationBuilder AddRequirementBuilderDelegate<TRequest>(Func<IServiceProvider, TRequest, Task<IRequestAuthorizationRequirement>> builder)
    {
        FuncRequirementBuilder<TRequest> Factory(IServiceProvider sp) => new(sp, builder);
        Services.Add(new ServiceDescriptor(typeof(IRequestAuthorizationRequirementBuilder<TRequest>), Factory, ServiceLifetime));

        return this;
    }

    public IHandlerRegistrationBuilder AddRequirementBuilderDelegate<TRequest>(Func<TRequest, Task<IRequestAuthorizationRequirement>> builder)
    {
        return AddRequirementBuilderDelegate<TRequest>((_, request) => builder(request));
    }
}
