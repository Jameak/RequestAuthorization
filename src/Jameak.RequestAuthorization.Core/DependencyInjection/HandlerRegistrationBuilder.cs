using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Configuration;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Internal;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Jameak.RequestAuthorization.Core.DependencyInjection;

internal class HandlerRegistrationBuilder : IHandlerRegistrationBuilder
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
        Services.AddScoped<IRequestAuthorizationExecutor, AuthorizationExecutor>();
        Services.AddSingleton<AuthorizationHandlerRegistry>();
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
        Services.AddTransient(provider => options);
    }

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    public IHandlerRegistrationBuilder AddRequirementHandlerTypesFromAssembly(Assembly assembly)
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

            Services.AddScoped(tuple.handlerType);
        }

        if (invalidDeclarations.Count > 0)
        {
            throw new InvalidHandlerRegistrationException($"Assembly contains invalid handler types. These handler types implement {nameof(RequestAuthorizationHandlerBase<>)} with the generic type {nameof(IRequestAuthorizationRequirement)}. The generic type must inherit from this interface-type instead.\n{string.Join('\n', invalidDeclarations.Select(e => e.handlerType.FullName))}");
        }

        Services.AddSingleton(new AuthorizationHandlerRegistrar(handlerTuples!));
        return this;

        static IEnumerable<Type> GetBaseTypes(Type type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MA0015:Specify the parameter name in ArgumentException", Justification = "Argument exceptions related to generic params")]
    public IHandlerRegistrationBuilder AddRequirementHandlerType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler, TRequirement>() where THandler : RequestAuthorizationHandlerBase<TRequirement> where TRequirement : IRequestAuthorizationRequirement
    {
        if (typeof(TRequirement) == typeof(IRequestAuthorizationRequirement))
        {
            throw new ArgumentException($"Generic argument '{nameof(TRequirement)}' must inherit from {nameof(IRequestAuthorizationRequirement)}.", nameof(TRequirement));
        }

        ThrowIfAbstractOrInterface<THandler>(nameof(THandler));
        Services.AddScoped<THandler>();
        Services.AddSingleton(new AuthorizationHandlerRegistrar([(typeof(THandler), typeof(TRequirement))]));

        return this;
    }

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes() and type.GetInterfaces()")]
    public IHandlerRegistrationBuilder AddRequirementBuilderTypesFromAssembly(Assembly assembly)
    {
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
            }
        }

        return this;
    }

    public IHandlerRegistrationBuilder AddRequirementBuilderType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TBuilder, TRequest>() where TBuilder : class, IRequestAuthorizationRequirementBuilder<TRequest>
    {
        ThrowIfAbstractOrInterface<TBuilder>(nameof(TBuilder));
        RegisterWithLifetime<IRequestAuthorizationRequirementBuilder<TRequest>, TBuilder>();
        return this;
    }

    [RequiresUnreferencedCode("Calls System.Reflection.Assembly.GetTypes()")]
    public IHandlerRegistrationBuilder AddGlobalRequirementBuilderTypesFromAssembly(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes()
            .Where(t => !t.IsAbstract &&
                !t.IsInterface &&
                !t.IsGenericTypeDefinition &&
                s_globalBuilderInterfaceType.IsAssignableFrom(t)))
        {
            RegisterWithLifetime(s_globalBuilderInterfaceType, type);
        }

        return this;
    }

    public IHandlerRegistrationBuilder AddGlobalRequirementBuilderType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TGlobalHandler>() where TGlobalHandler : class, IGlobalRequestAuthorizationRequirementBuilder
    {
        ThrowIfAbstractOrInterface<TGlobalHandler>(nameof(TGlobalHandler));
        RegisterWithLifetime<IGlobalRequestAuthorizationRequirementBuilder, TGlobalHandler>();
        return this;
    }

    public IHandlerRegistrationBuilder WithUnauthorizedResultHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>() where THandler : class, IUnauthorizedResultHandler
    {
        ThrowIfAbstractOrInterface<THandler>(nameof(THandler));
        RegisterWithLifetime<IUnauthorizedResultHandler, THandler>();
        return this;
    }

    public IHandlerRegistrationBuilder WithAuthorizedResultHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler>() where THandler : class, IAuthorizedResultHandler
    {
        ThrowIfAbstractOrInterface<THandler>(nameof(THandler));
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "MA0015:Specify the parameter name in ArgumentException", Justification = "Throw helper related to generic params")]
    private static void ThrowIfAbstractOrInterface<T>(string genericParamName)
    {
        if (typeof(T).IsAbstract || typeof(T).IsInterface)
        {
            throw new ArgumentException($"Generic argument '{genericParamName}' cannot be abstract class or interface.", genericParamName);
        }
    }
}
