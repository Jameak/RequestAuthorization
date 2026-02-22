using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Results;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;
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
        var service = serviceCollection.BuildServiceProvider();

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
        serviceCollection.AddRequestAuthorizationCore()
            .WithAuthorizedResultHandler<OverwrittenAuthBehavior>()
            .WithUnauthorizedResultHandler<OverwrittenUnauthBehavior>();
        var service = serviceCollection.BuildServiceProvider();

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
        serviceCollection.AddRequestAuthorizationCore()
            .WithAuthorizedResultHandler<OverwrittenAuthBehavior>()
            .WithUnauthorizedResultHandler<OverwrittenUnauthBehavior>();

        serviceCollection.AddRequestAuthorizationCore();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var authResultHandler = service.GetRequiredService<IAuthorizedResultHandler>();
        var unauthResultHandler = service.GetRequiredService<IUnauthorizedResultHandler>();

        // Assert
        Assert.Equal(typeof(OverwrittenAuthBehavior), authResultHandler.GetType());
        Assert.Equal(typeof(OverwrittenUnauthBehavior), unauthResultHandler.GetType());
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
    public void AddRequirementHandlerTypesFromAssembly_RegistersCorrectTypes()
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
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var registrars = serviceProvider.GetRequiredService<AuthorizationHandlerRegistrar>();

        Assert.Single(registrars.TypesToRegister, e => e.handlerType == typeof(AlwaysSuccessRequirementHandler) && e.requirementType == typeof(AlwaysSuccessRequirement));
        Assert.Single(registrars.TypesToRegister, e => e.handlerType == typeof(HandlerTypeInheritingFromUserProvidedAbstract) && e.requirementType == typeof(AlwaysSuccessRequirement));
        Assert.Single(registrars.TypesToRegister, e => e.handlerType == typeof(HandlerTypeInheritingFromUserProvidedGenericAbstract) && e.requirementType == typeof(AlwaysSuccessRequirement));
    }

    [Fact]
    public void AddRequirementBuilderTypesFromAssembly_RegistersCorrectTypes()
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
    }

    [Fact]
    public void AddGlobalRequirementBuilderTypesFromAssembly_RegistersCorrectTypes()
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
    }

    [Fact]
    public void AddRequirementHandlerType_ViaGenerics_WithValidAndInvalidTypes_ThrowsOnInvalid()
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
    }

    [Fact]
    public void AddRequirementHandlerType_ViaTypeArguments_WithValidAndInvalidTypes_ThrowsOnInvalid()
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

        // Valid types
        builder.AddRequirementHandlerType(typeof(AnotherGenericRequestHandler<object>), typeof(AlwaysSuccessRequirement));
        builder.AddRequirementHandlerType(typeof(GenericRequestHandler<AlwaysSuccessRequirement>), typeof(AlwaysSuccessRequirement));
        AssertExpectedHandlerRegistrations(serviceCollection);
    }

    private static void AssertExpectedHandlerRegistrations(ServiceCollection serviceCollection)
    {
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(AnotherGenericRequestHandler<object>));
        Assert.Single(serviceCollection, e => e.ServiceType == typeof(GenericRequestHandler<AlwaysSuccessRequirement>));

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var registrars = serviceProvider.GetServices<AuthorizationHandlerRegistrar>().ToList();

        Assert.Single(registrars, e => e.TypesToRegister.Any(e => e.handlerType == typeof(AnotherGenericRequestHandler<object>) && e.requirementType == typeof(AlwaysSuccessRequirement)));
        Assert.Single(registrars, e => e.TypesToRegister.Any(e => e.handlerType == typeof(GenericRequestHandler<AlwaysSuccessRequirement>) && e.requirementType == typeof(AlwaysSuccessRequirement)));
    }

    [Fact]
    public void AddRequirementBuilderType_ViaGenerics_WithValidAndInvalidTypes_ThrowsOnInvalid()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddRequestAuthorizationCore();

        // Act & Assert

        // Invalid types
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType<AbstractRequestBuilder, TestRequest>()); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType<GenericAbstractRequestBuilder<TestRequest>, TestRequest>()); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType<GenericInterfaceRequestBuilder<TestRequest>, TestRequest>()); // Interface & generic
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

        AssertExpectedBuilderRegistrations(serviceCollection);
    }

    [Fact]
    public void AddRequirementBuilderType_ViaTypeArguments_WithValidAndInvalidTypes_ThrowsOnInvalid()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var builder = serviceCollection.AddRequestAuthorizationCore();

        // Act & Assert

        // Invalid types
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(AbstractRequestBuilder), typeof(TestRequest))); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(GenericAbstractRequestBuilder<TestRequest>), typeof(TestRequest))); // Abstract
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(GenericInterfaceRequestBuilder<TestRequest>), typeof(TestRequest))); // Interface & generic
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(InterfaceRequestBuilder), typeof(TestRequest))); // Interface
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(GenericRequestBuilder<TestRequest>), typeof(TestRequest2))); // Wrong request-type
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(AnotherGenericRequestBuilder<object>), typeof(TestRequest2))); // Wrong request-type
        Assert.Throws<ArgumentException>(() => builder.AddRequirementBuilderType(typeof(GenericRequestBuilder<>), typeof(TestRequest))); // Open-generic

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

        AssertExpectedBuilderRegistrations(serviceCollection);
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
    }

    [Fact]
    public void AddGlobalRequirementBuilderType_ViaGenerics_WithValidAndInvalidTypes_ThrowsOnInvalid()
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
    }

    [Fact]
    public void AddGlobalRequirementBuilderType_ViaTypeArguments_WithValidAndInvalidTypes_ThrowsOnInvalid()
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
    }
}
