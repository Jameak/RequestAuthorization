using System.Diagnostics.CodeAnalysis;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Results;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities.DerivedTypesForAssemblyScanTests;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities.GlobalBuilderTypesForAssemblyScanTests;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities.HandlerTypesForAssemblyScanTests;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Core.Tests.DependencyInjection;

public class HandlerRegistrationBuilderTests
{
    [Fact]
    public void AuthBehaviorsAreRegisteredByDefault()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var authResultHandler = service.GetRequiredService<IAuthorizedResultHandler>();
        var unauthResultHandler = service.GetRequiredService<IUnauthorizedResultHandler>();

        // Assert
        Assert.Equal(typeof(DefaultAuthorizedResultHandler), authResultHandler.GetType());
        Assert.Equal(typeof(DefaultUnauthorizedResultHandler), unauthResultHandler.GetType());
    }

    [Fact]
    public void AuthBehaviorCanBeOverwritten()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: ServiceLifetime.Singleton)
            .WithAuthorizedResultHandler<OverwrittenAuthBehavior>()
            .WithUnauthorizedResultHandler<OverwrittenUnauthBehavior>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var authResultHandler = service.GetRequiredService<IAuthorizedResultHandler>();
        var unauthResultHandler = service.GetRequiredService<IUnauthorizedResultHandler>();

        // Assert
        Assert.Equal(typeof(OverwrittenAuthBehavior), authResultHandler.GetType());
        Assert.Equal(typeof(OverwrittenUnauthBehavior), unauthResultHandler.GetType());
    }

    [Fact]
    public void MultipleCoreRegistrationsDoesNotOverwriteCustomAuthBehaviors()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: ServiceLifetime.Singleton)
            .WithAuthorizedResultHandler<OverwrittenAuthBehavior>()
            .WithUnauthorizedResultHandler<OverwrittenUnauthBehavior>();

        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: ServiceLifetime.Singleton);
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var authResultHandler = service.GetRequiredService<IAuthorizedResultHandler>();
        var unauthResultHandler = service.GetRequiredService<IUnauthorizedResultHandler>();

        // Assert
        Assert.Equal(typeof(OverwrittenAuthBehavior), authResultHandler.GetType());
        Assert.Equal(typeof(OverwrittenUnauthBehavior), unauthResultHandler.GetType());
    }

    [Fact]
    public void MultipleCoreRegistrationsWithDifferentLifetimesThrowsException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore(serviceLifetime: ServiceLifetime.Singleton)
            .WithAuthorizedResultHandler<OverwrittenAuthBehavior>()
            .WithUnauthorizedResultHandler<OverwrittenUnauthBehavior>();

        Assert.Throws<ArgumentException>(() => serviceCollection.AddRequestAuthorizationCore(serviceLifetime: ServiceLifetime.Scoped));
    }

    public class OverwrittenAuthBehavior : IAuthorizedResultHandler
    {
        public Task OnAuthorized<TRequest>(TRequest message, RequestAuthorizationResult result, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    public class OverwrittenUnauthBehavior : IUnauthorizedResultHandler
    {
        public Task<TResponse> OnUnauthorized<TRequest, TResponse>(TRequest message, RequestAuthorizationResult authResult, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<TResponse> OnUnauthorizedStream<TRequest, TResponse>(TRequest message, RequestAuthorizationResult authResult, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    [Fact]
    public async Task AddRequirementHandlerTypesFromAssembly_RegistersCorrectTypes()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act & Assert
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerTypesFromAssembly(typeof(HandlerRegistrationBuilderTests).Assembly);

        Assert.Single(serviceCollection, e => e.ServiceType == typeof(AlwaysSuccessRequirementHandler));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(HandlerTypeInheritingFromUserProvidedAbstract));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(HandlerTypeInheritingFromUserProvidedGenericAbstract));
        Assert.DoesNotContain(serviceCollection, e => e.ServiceType == typeof(AbstractRequestHandler));
        Assert.DoesNotContain(serviceCollection, e => e.ServiceType == typeof(AnotherGenericRequestHandler<>));
        Assert.DoesNotContain(serviceCollection, e => e.ServiceType == typeof(GenericAbstractRequestHandler<>));
        Assert.DoesNotContain(serviceCollection, e => e.ServiceType == typeof(GenericRequestHandler<>));
        Assert.DoesNotContain(serviceCollection, e => e.ServiceType.IsGenericType && e.ServiceType.GetGenericTypeDefinition() == typeof(AnotherGenericRequestHandler<>));
        Assert.DoesNotContain(serviceCollection, e => e.ServiceType.IsGenericType && e.ServiceType.GetGenericTypeDefinition() == typeof(GenericAbstractRequestHandler<>));
        Assert.DoesNotContain(serviceCollection, e => e.ServiceType.IsGenericType && e.ServiceType.GetGenericTypeDefinition() == typeof(GenericRequestHandler<>));
        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        var registrars = serviceProvider.GetRequiredService<AuthorizationHandlerRegistrar>();

        Assert.Single(registrars.TypesToRegister, e => e.handlerType == typeof(AlwaysSuccessRequirementHandler) && e.requirementType == typeof(AlwaysSuccessRequirement));
        Assert.Single(registrars.TypesToRegister, e => e.handlerType == typeof(HandlerTypeInheritingFromUserProvidedAbstract) && e.requirementType == typeof(AlwaysSuccessRequirement));
        Assert.Single(registrars.TypesToRegister, e => e.handlerType == typeof(HandlerTypeInheritingFromUserProvidedGenericAbstract) && e.requirementType == typeof(AlwaysSuccessRequirement));

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);
    }

    [Fact]
    public async Task AddRequirementBuilderTypesFromAssembly_RegistersCorrectTypes()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act & Assert
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderTypesFromAssembly(typeof(HandlerRegistrationBuilderTests).Assembly);

        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType == typeof(BuilderInheritingFromUserProvidedAbstract));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType == typeof(BuilderInheritingFromUserProvidedGenericAbstract));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType == typeof(BuilderInheritingFromUserProvidedGenericInterface));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType == typeof(BuilderInheritingFromUserProvidedInterface));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType == typeof(BuilderInheritingFromUserProvidedAbstract));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType == typeof(AlwaysSuccessRequirementBuilder));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType == typeof(BuilderImplementingMultipleInterfaces));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest2>) && e.ImplementationType == typeof(BuilderImplementingMultipleInterfaces));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType == typeof(AbstractRequestBuilder));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType == typeof(AnotherGenericRequestBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType != null && e.ImplementationType.IsGenericType && e.ImplementationType.GetGenericTypeDefinition() == typeof(AnotherGenericRequestBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType == typeof(GenericAbstractRequestBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType != null && e.ImplementationType.IsGenericType && e.ImplementationType.GetGenericTypeDefinition() == typeof(GenericAbstractRequestBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType == typeof(GenericInterfaceRequestBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType != null && e.ImplementationType.IsGenericType && e.ImplementationType.GetGenericTypeDefinition() == typeof(GenericInterfaceRequestBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType == typeof(GenericRequestBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType != null && e.ImplementationType.IsGenericType && e.ImplementationType.GetGenericTypeDefinition() == typeof(GenericRequestBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType == typeof(InterfaceRequestBuilder));

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);

        var provider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        AssertServiceProviderCanInstantiate<IRequestAuthorizationRequirementBuilder<TestRequest>>(provider);
        AssertServiceProviderCanInstantiate<IRequestAuthorizationRequirementBuilder<TestRequest2>>(provider);
    }

    [Fact]
    public async Task AddGlobalRequirementBuilderTypesFromAssembly_RegistersCorrectTypes()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        // Act & Assert
        serviceCollection.AddRequestAuthorizationCore()
            .AddGlobalRequirementBuilderTypesFromAssembly(typeof(HandlerRegistrationBuilderTests).Assembly);

        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IGlobalRequestAuthorizationRequirementBuilder) && e.ImplementationType == typeof(GlobalRequirementBuilderProducingSuccessRequirement));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IGlobalRequestAuthorizationRequirementBuilder) && e.ImplementationType == typeof(GlobalBuilderInheritingFromUserProvidedAbstract));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IGlobalRequestAuthorizationRequirementBuilder) && e.ImplementationType == typeof(GlobalBuilderInheritingFromUserProvidedGeneric));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IGlobalRequestAuthorizationRequirementBuilder) && e.ImplementationType == typeof(GlobalBuilderInheritingFromUserProvidedInterface));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType == typeof(AbstractGlobalBuilder));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType == typeof(GenericGlobalBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType != null && e.ImplementationType.IsGenericType && e.ImplementationType.GetGenericTypeDefinition() == typeof(GenericGlobalBuilder<>));
        Assert.DoesNotContain(serviceCollection, e => e.ImplementationType == typeof(InterfaceGlobalBuilder));

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);

        var provider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        AssertServiceProviderCanInstantiate<IGlobalRequestAuthorizationRequirementBuilder>(provider);
    }

    [Fact]
    public async Task AddRequirementHandlerType_ViaGenerics_ThrowsOnInvalid()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddRequestAuthorizationCore();

        // Act & Assert

        // Invalid types
        Assert.Throws<ArgumentException>(() => builder.AddRequirementHandlerType<AbstractRequestHandler, AlwaysSuccessRequirement>()); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementHandlerType<GenericAbstractRequestHandler<AlwaysSuccessRequirement>, AlwaysSuccessRequirement>()); // Abstract

        // Valid types
        builder.AddRequirementHandlerType<AnotherGenericRequestHandler<object>, AlwaysSuccessRequirement>();
        builder.AddRequirementHandlerType<GenericRequestHandler<AlwaysSuccessRequirement>, AlwaysSuccessRequirement>();
        AssertExpectedHandlerRegistrations(serviceCollection);

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);
    }

    [Fact]
    [SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Test needs to specifically test the non-generic overload.")]
    public async Task AddRequirementHandlerType_ViaTypeArguments_ThrowsOnInvalid()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddRequestAuthorizationCore();

        // Act & Assert

        // Invalid types
        Assert.Throws<ArgumentException>(() => builder.AddRequirementHandlerType(typeof(AbstractRequestHandler), typeof(AlwaysSuccessRequirement))); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementHandlerType(typeof(GenericAbstractRequestHandler<AlwaysSuccessRequirement>), typeof(AlwaysSuccessRequirement))); // Generic abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementHandlerType(typeof(GenericAbstractRequestHandler<>), typeof(AlwaysSuccessRequirement))); // Open-generic
        Assert.Throws<ArgumentException>(() => builder.AddRequirementHandlerType(typeof(AnotherGenericRequestHandler<object>), typeof(AlwaysFailureRequirement))); // Wrong request-type
        Assert.Throws<ArgumentException>(() => builder.AddRequirementHandlerType(typeof(GenericRequestHandler<AlwaysSuccessRequirement>), typeof(AlwaysFailureRequirement))); // Wrong request-type
        Assert.Throws<ArgumentException>(() => builder.AddRequirementHandlerType(typeof(GenericRequestHandler<AlwaysSuccessRequirement>), typeof(object))); // Requirement type does not inherit from IRequestAuthorizationRequirement

        // Valid types
        builder.AddRequirementHandlerType(typeof(AnotherGenericRequestHandler<object>), typeof(AlwaysSuccessRequirement));
        builder.AddRequirementHandlerType(typeof(GenericRequestHandler<AlwaysSuccessRequirement>), typeof(AlwaysSuccessRequirement));
        AssertExpectedHandlerRegistrations(serviceCollection);

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);
    }

    private static void AssertExpectedHandlerRegistrations(ServiceCollection serviceCollection)
    {
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(AnotherGenericRequestHandler<object>));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(GenericRequestHandler<AlwaysSuccessRequirement>));

        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        var registrars = serviceProvider.GetServices<AuthorizationHandlerRegistrar>().ToList();

        Assert.Single(registrars, e => e.TypesToRegister.Any(e => e.handlerType == typeof(AnotherGenericRequestHandler<object>) && e.requirementType == typeof(AlwaysSuccessRequirement)));
        Assert.Single(registrars, e => e.TypesToRegister.Any(e => e.handlerType == typeof(GenericRequestHandler<AlwaysSuccessRequirement>) && e.requirementType == typeof(AlwaysSuccessRequirement)));
    }

    [Fact]
    public async Task AddRequirementBuilderType_ViaGenerics_ThrowsOnInvalid()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddRequestAuthorizationCore();

        // Act & Assert

        // Invalid types
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType<AbstractRequestBuilder, TestRequest>()); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType<GenericAbstractRequestBuilder<TestRequest>, TestRequest>()); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType<GenericInterfaceRequestBuilder<TestRequest>, TestRequest>()); // Interface
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType<InterfaceRequestBuilder, TestRequest>()); // Interface

        // Valid types
        builder.AddRequirementBuilderType<GenericRequestBuilder<TestRequest>, TestRequest>();
        builder.AddRequirementBuilderType<AnotherGenericRequestBuilder<object>, TestRequest>();
        builder.AddRequirementBuilderType<BuilderInheritingFromUserProvidedAbstract, TestRequest>();
        builder.AddRequirementBuilderType<BuilderInheritingFromUserProvidedGenericAbstract, TestRequest>();
        builder.AddRequirementBuilderType<BuilderInheritingFromUserProvidedGenericInterface, TestRequest>();
        builder.AddRequirementBuilderType<BuilderInheritingFromUserProvidedInterface, TestRequest>();
        builder.AddRequirementBuilderType<AlwaysSuccessRequirementBuilder, TestRequest>();
        builder.AddRequirementBuilderType<BuilderImplementingMultipleInterfaces, TestRequest>();
        builder.AddRequirementBuilderType<BuilderImplementingMultipleInterfaces, TestRequest2>();
        builder.AddRequirementBuilderType<BuilderHandlingMultipleAssignableRequests, TestMultiBaseRequest>();

        AssertExpectedBuilderRegistrations(serviceCollection);

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);
    }

    [Fact]
    [SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Test needs to specifically test the non-generic overload.")]
    public async Task AddRequirementBuilderType_ViaTypeArguments_ThrowsOnInvalid()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddRequestAuthorizationCore();

        // Act & Assert

        // Invalid types
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(AbstractRequestBuilder), typeof(TestRequest))); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(GenericAbstractRequestBuilder<TestRequest>), typeof(TestRequest))); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(GenericInterfaceRequestBuilder<TestRequest>), typeof(TestRequest))); // Interface
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(InterfaceRequestBuilder), typeof(TestRequest))); // Interface
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(GenericRequestBuilder<TestRequest>), typeof(TestRequest2))); // Wrong request-type
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(AnotherGenericRequestBuilder<object>), typeof(TestRequest2))); // Wrong request-type
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(GenericRequestBuilder<>), typeof(TestRequest))); // Open-generic
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(BuilderImplementingMultipleInterfaces), typeof(TestBaseRequest))); // Builder-type implements builder-interface with T derived from request-type multiple times

        // Valid types
        builder.AddRequirementBuilderType(typeof(GenericRequestBuilder<TestRequest>), typeof(TestRequest));
        builder.AddRequirementBuilderType(typeof(AnotherGenericRequestBuilder<object>), typeof(TestRequest));
        builder.AddRequirementBuilderType(typeof(BuilderInheritingFromUserProvidedAbstract), typeof(TestRequest));
        builder.AddRequirementBuilderType(typeof(BuilderInheritingFromUserProvidedGenericAbstract), typeof(TestRequest));
        builder.AddRequirementBuilderType(typeof(BuilderInheritingFromUserProvidedGenericInterface), typeof(TestRequest));
        builder.AddRequirementBuilderType(typeof(BuilderInheritingFromUserProvidedInterface), typeof(TestRequest));
        builder.AddRequirementBuilderType(typeof(AlwaysSuccessRequirementBuilder), typeof(TestRequest));
        builder.AddRequirementBuilderType(typeof(BuilderImplementingMultipleInterfaces), typeof(TestRequest));
        builder.AddRequirementBuilderType(typeof(BuilderImplementingMultipleInterfaces), typeof(TestRequest2));
        builder.AddRequirementBuilderType(typeof(BuilderHandlingMultipleAssignableRequests), typeof(TestMultiBaseRequest));

        AssertExpectedBuilderRegistrations(serviceCollection);

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);
    }

    private static void AssertExpectedBuilderRegistrations(ServiceCollection serviceCollection)
    {
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType != null && e.ImplementationType == typeof(GenericRequestBuilder<TestRequest>));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType != null && e.ImplementationType == typeof(AnotherGenericRequestBuilder<object>));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType != null && e.ImplementationType == typeof(BuilderInheritingFromUserProvidedAbstract));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType != null && e.ImplementationType == typeof(BuilderInheritingFromUserProvidedGenericAbstract));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType != null && e.ImplementationType == typeof(BuilderInheritingFromUserProvidedGenericInterface));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType != null && e.ImplementationType == typeof(BuilderInheritingFromUserProvidedInterface));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType != null && e.ImplementationType == typeof(AlwaysSuccessRequirementBuilder));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest>) && e.ImplementationType != null && e.ImplementationType == typeof(BuilderImplementingMultipleInterfaces));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<TestRequest2>) && e.ImplementationType != null && e.ImplementationType == typeof(BuilderImplementingMultipleInterfaces));

        var provider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        AssertServiceProviderCanInstantiate<IRequestAuthorizationRequirementBuilder<TestRequest>>(provider);
        AssertServiceProviderCanInstantiate<IRequestAuthorizationRequirementBuilder<TestRequest2>>(provider);
    }

    [Fact]
    public async Task AddGlobalRequirementBuilderType_ViaGenerics_ThrowsOnInvalid()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddRequestAuthorizationCore();

        // Act & Assert

        // Invalid types
        Assert.Throws<ArgumentException>(() => builder.AddGlobalRequirementBuilderType<AbstractGlobalBuilder>()); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddGlobalRequirementBuilderType<InterfaceGlobalBuilder>()); // Interface

        // Valid types
        builder.AddGlobalRequirementBuilderType<GlobalRequirementBuilderProducingSuccessRequirement>();
        builder.AddGlobalRequirementBuilderType<GlobalBuilderInheritingFromUserProvidedAbstract>();
        builder.AddGlobalRequirementBuilderType<GlobalBuilderInheritingFromUserProvidedGeneric>();
        builder.AddGlobalRequirementBuilderType<GlobalBuilderInheritingFromUserProvidedInterface>();
        builder.AddGlobalRequirementBuilderType<GenericGlobalBuilder<object>>();

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        AssertServiceProviderCanInstantiate<IGlobalRequestAuthorizationRequirementBuilder>(provider);
    }

    [Fact]
    [SuppressMessage("Usage", "CA2263:Prefer generic overload when type is known", Justification = "Test needs to specifically test the non-generic overload.")]
    public async Task AddGlobalRequirementBuilderType_ViaTypeArguments_ThrowsOnInvalid()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddRequestAuthorizationCore();

        // Act & Assert

        // Invalid types
        Assert.Throws<ArgumentException>(() => builder.AddGlobalRequirementBuilderType(typeof(AbstractGlobalBuilder))); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddGlobalRequirementBuilderType(typeof(InterfaceGlobalBuilder))); // Interface
        Assert.Throws<ArgumentException>(() => builder.AddGlobalRequirementBuilderType(typeof(GenericGlobalBuilder<>))); // Open generic
        Assert.Throws<ArgumentException>(() => builder.AddGlobalRequirementBuilderType(typeof(HandlerRegistrationBuilderTests))); // Does not inherit from global interface

        // Valid types
        builder.AddGlobalRequirementBuilderType(typeof(GlobalRequirementBuilderProducingSuccessRequirement));
        builder.AddGlobalRequirementBuilderType(typeof(GlobalBuilderInheritingFromUserProvidedAbstract));
        builder.AddGlobalRequirementBuilderType(typeof(GlobalBuilderInheritingFromUserProvidedGeneric));
        builder.AddGlobalRequirementBuilderType(typeof(GlobalBuilderInheritingFromUserProvidedInterface));
        builder.AddGlobalRequirementBuilderType(typeof(GenericGlobalBuilder<object>));

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        AssertServiceProviderCanInstantiate<IGlobalRequestAuthorizationRequirementBuilder>(provider);
    }

    [Fact]
    public async Task AddRequirementBuildersForDerivedRequestsFromAssembly_BehavesCorrectly()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddRequestAuthorizationCore();
        var assembly = typeof(DerivedRequest1).Assembly;

        // Invalid builder types
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(AbstractBaseRequestBuilder<>), typeof(IBaseRequest1<>), assembly)); // Builder is abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(InterfaceBaseRequestBuilder<>), typeof(IBaseRequest1<>), assembly)); // Builder is interface
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(OpenGenericClass<>), typeof(IBaseRequest1<>), assembly)); // 'Builder' type does not inherit from builder interface.
        Assert.Throws<AssemblyScanningRegistrationException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(OpenGenericBuilderWithoutRequestConstraint<>), typeof(IBaseRequest1<>), assembly)); // Builder is not constrained to IBaseRequest.
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(OpenBaseRequestBuilder<>), typeof(IBaseRequest2<>), assembly)); // Builder is constrained to different request hierarchy
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(OpenBaseRequestBuilder<>), typeof(ClosedRequestFromBaseRequest1), assembly)); // Builder is open but request is closed
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(ClosedBuilderFromBaseRequest1), typeof(IBaseRequest1<>), assembly)); // Builder is closed but request is open
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(OpenBaseRequest3Builder<>), typeof(IBaseRequest3<>), assembly, throwWhenNoValidTypesFound: true)); // Builder is valid but request type has no derived types
        Assert.Throws<AssemblyScanningRegistrationException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(ClosedNonGenericBaseRequestBuilder), typeof(IRequest<UserDataRequest1>), assembly)); // Closed builder has request in hierarchy, but is not assignable from it.
        Assert.Throws<AssemblyScanningRegistrationException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(OpenBaseRequestBuilderWithExtraConstraint<>), typeof(IBaseRequest1<>), assembly)); // Assembly contains types derived from IBaseRequest1<> that are not valid for the generic constraints the exist on OpenBaseRequestBuilderWithExtraConstraint<>
        Assert.Throws<AssemblyScanningRegistrationException>(() => builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(NotBaseRequestBuilder<>), typeof(IBaseRequest5<>), assembly)); // Assembly contains types that derive from IBaseRequest5<> multiple times, which is not supported

        // Valid open generic builder
        builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(OpenBaseRequest3Builder<>), typeof(IBaseRequest3<>), assembly, throwWhenNoValidTypesFound: false); // Finding no types is ok when throwing is disabled.
        builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(OpenBaseRequestBuilder<>), typeof(IBaseRequest1<>), assembly); // Builder is constrainted to T : IBaseRequest
        builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(NotBaseRequestBuilder<>), typeof(IBaseRequest1<>), assembly); // Builder is constrained to parent type of IBaseRequest
        builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(ClosedNonGenericBaseRequestBuilder), typeof(INonGenericBaseRequest), assembly); // Both builder and request are closed
        builder.AddRequirementBuilderTypeForDerivedRequestsFromAssembly(typeof(NotBaseRequestBuilder<>), typeof(ClassBaseRequest4<>), assembly); // Request-base is a class, not an interface type

        var builderDescriptors = serviceCollection
            .Where(s => s.ServiceType.IsGenericType &&
                        s.ServiceType.GetGenericTypeDefinition() ==
                        typeof(IRequestAuthorizationRequirementBuilder<>))
            .ToList();

        Assert.Contains(builderDescriptors, d => d.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<ClosedRequestFromBaseRequest1>) && d.ImplementationType == typeof(OpenBaseRequestBuilder<UserDataRequest1>));
        Assert.Contains(builderDescriptors, d => d.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<ClosedRequestFromBaseRequest1>) && d.ImplementationType == typeof(NotBaseRequestBuilder<UserDataRequest1>));
        Assert.Contains(builderDescriptors, d => d.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<DerivedRequest1>) && d.ImplementationType == typeof(OpenBaseRequestBuilder<UserDataRequest1>));
        Assert.Contains(builderDescriptors, d => d.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<DerivedRequest1>) && d.ImplementationType == typeof(NotBaseRequestBuilder<UserDataRequest1>));
        Assert.Contains(builderDescriptors, d => d.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<DerivedRequest2>) && d.ImplementationType == typeof(OpenBaseRequestBuilder<UserDataRequest2>));
        Assert.Contains(builderDescriptors, d => d.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<DerivedRequest2>) && d.ImplementationType == typeof(NotBaseRequestBuilder<UserDataRequest2>));
        Assert.Contains(builderDescriptors, d => d.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<NonGenericDerivedRequest>) && d.ImplementationType == typeof(ClosedNonGenericBaseRequestBuilder));
        Assert.Contains(builderDescriptors, d => d.ServiceType == typeof(IRequestAuthorizationRequirementBuilder<DerivedRequestFromBaseRequest4>) && d.ImplementationType == typeof(NotBaseRequestBuilder<UserDataRequest1>));

        Assert.DoesNotContain(builderDescriptors, d => d.ServiceType.GenericTypeArguments[0] == typeof(AbstractDerivedRequest));
        Assert.DoesNotContain(builderDescriptors, d => d.ServiceType.GenericTypeArguments[0] == typeof(IInterfaceDerivedRequest));
        Assert.DoesNotContain(builderDescriptors, d => d.ServiceType.GenericTypeArguments[0] == typeof(OpenGenericDerivedRequest<>));

        await AssertServiceCollectionMatchesSnapshot(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        AssertServiceProviderCanInstantiate<IRequestAuthorizationRequirementBuilder<ClosedRequestFromBaseRequest1>>(provider);
        AssertServiceProviderCanInstantiate<IRequestAuthorizationRequirementBuilder<DerivedRequest1>>(provider);
        AssertServiceProviderCanInstantiate<IRequestAuthorizationRequirementBuilder<DerivedRequest2>>(provider);
        AssertServiceProviderCanInstantiate<IRequestAuthorizationRequirementBuilder<NonGenericDerivedRequest>>(provider);
        AssertServiceProviderCanInstantiate<IRequestAuthorizationRequirementBuilder<DerivedRequestFromBaseRequest4>>(provider);
    }

    private static void AssertServiceProviderCanInstantiate<TServiceType>(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var services = scope.ServiceProvider.GetServices<TServiceType>();
        Assert.NotEmpty(services);
    }

    private static async Task AssertServiceCollectionMatchesSnapshot(ServiceCollection serviceCollection)
    {
        var services = serviceCollection
            .Select(e => new DescriptorForVerify(e.Lifetime, e.ServiceType, e.ImplementationType))
            .OrderBy(e => e.ToString(), StringComparer.InvariantCulture);
        await Verify(services);
    }

    internal sealed record DescriptorForVerify(ServiceLifetime Lifetime, Type ServiceType, Type? ImplementationType);
}
