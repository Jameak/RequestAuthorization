using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Results;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Core.Tests.IntegrationTests;

public partial class RequestAuthorizationCheckerTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SuccessfulRequirementProducesAuthorized(bool useDelegateLogic)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceBuilder = serviceCollection.AddRequestAuthorizationCore();
        if (useDelegateLogic)
        {
            serviceBuilder
                .AddRequirementHandlerDelegate<AlwaysSuccessRequirement>((requirement) => Task.FromResult(RequestAuthorizationResult.Success(requirement)))
                .AddRequirementBuilderDelegate<TestRequest>((request) => Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement()));
        }
        else
        {
            serviceBuilder
                .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
                .AddRequirementBuilderType<AlwaysSuccessRequirementBuilder, TestRequest>();
        }

        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.True(checkResult.IsAuthorized);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FailingRequirementProducesUnauthorized(bool useDelegateLogic)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();

        var serviceBuilder = serviceCollection.AddRequestAuthorizationCore();
        if (useDelegateLogic)
        {
            serviceBuilder
                .AddRequirementHandlerDelegate<AlwaysFailureRequirement>((requirement) => Task.FromResult(RequestAuthorizationResult.Fail(requirement)))
                .AddRequirementBuilderDelegate<TestRequest>((request) => Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysFailureRequirement()));
        }
        else
        {
            serviceBuilder
                .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
                .AddRequirementBuilderType<AlwaysFailureRequirementBuilder, TestRequest>();
        }

        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RequirementBuilderThatCrashesProducesUnauthorizedWithDetails(bool useDelegateLogic)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceBuilder = serviceCollection.AddRequestAuthorizationCore();
        if (useDelegateLogic)
        {
            serviceBuilder.AddRequirementBuilderDelegate<TestRequest>((request) => throw new Exception(RequirementBuilderThatCrashesOnBuild.CrashMessage));
        }
        else
        {
            serviceBuilder.AddRequirementBuilderType<RequirementBuilderThatCrashesOnBuild, TestRequest>();
        }

        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
        Assert.Equal(RequirementBuilderThatCrashesOnBuild.CrashMessage, checkResult.FailureException?.Message);
        Assert.Equal(typeof(RequirementBuildingMustNotThrowExceptionRequirement), checkResult.Requirement.GetType());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CrashingRequirementHandlerProducesUnauthorizedWithDetails(bool useDelegateLogic)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceBuilder = serviceCollection.AddRequestAuthorizationCore();
        if (useDelegateLogic)
        {
            serviceBuilder
                .AddRequirementHandlerDelegate<CrashingRequirement>((requirement) => throw new Exception(CrashingRequirementHandler.CrashMessage))
                .AddRequirementBuilderDelegate<TestRequest>((request) => Task.FromResult<IRequestAuthorizationRequirement>(new CrashingRequirement()));
        }
        else
        {
            serviceBuilder
                .AddRequirementHandlerType<CrashingRequirementHandler, CrashingRequirement>()
                .AddRequirementBuilderType<CrashingRequirementBuilder, TestRequest>();
        }

        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
        Assert.Equal(CrashingRequirementHandler.CrashMessage, checkResult.FailureException?.Message);
        Assert.Equal(typeof(CrashingRequirement), checkResult.Requirement.GetType());
    }

    [Fact]
    public async Task CircularRequirementProducesUnauthorizedWithDetails()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerType<CircularRequirementHandler, CircularRequirement>()
            .AddRequirementBuilderType<CircularRequirementBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
        Assert.Equal(typeof(CircularRequirementException), checkResult.FailureException?.GetType());
        Assert.Equal(typeof(CircularRequirement), (checkResult.FailureException as CircularRequirementException)?.Requirement.GetType());
    }

    [Fact]
    public async Task RequirementWithoutHandlerProducesException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderType<AlwaysSuccessRequirementBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act & Assert
        await Assert.ThrowsAsync<MissingHandlerRegistrationException>(async () => await RunCheck(service));
    }

    [Fact]
    public async Task RequirementWithSameHandlerTypeRegisteredMultipleTimesDoesNotThrowException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderType<AlwaysSuccessRequirementBuilder, TestRequest>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.True(checkResult.IsAuthorized);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RequirementWithDifferentHandlerTypesForSameRequirementRegisteredMultipleTimesThrowsException(bool useDelegateLogic)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var serviceBuilder = serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderType<AlwaysSuccessRequirementBuilder, TestRequest>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>();
        if (useDelegateLogic)
        {
            serviceBuilder.AddRequirementHandlerDelegate<AlwaysSuccessRequirement>((requirement) => Task.FromResult(RequestAuthorizationResult.Success(requirement)));
        }
        else
        {
            serviceBuilder.AddRequirementHandlerType<AnotherHandlerAlsoProcessingAlwaysSuccessRequirements, AlwaysSuccessRequirement>();
        }

        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidHandlerRegistrationException>(async () => await RunCheck(service));
    }

    public class AnotherHandlerAlsoProcessingAlwaysSuccessRequirements : RequestAuthorizationHandlerBase<AlwaysSuccessRequirement>
    {
        public override Task<RequestAuthorizationResult> CheckRequirementAsync(AlwaysSuccessRequirement requirement, CancellationToken token)
        {
            return Task.FromResult(RequestAuthorizationResult.Success(requirement));
        }
    }

    [Fact]
    public async Task RequirementHandlerThatCrashesOnInstantiationProducesException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderType<AlwaysSuccessRequirementBuilder, TestRequest>()
            .AddRequirementHandlerType<RequirementHandlerThatCrashesOnInstantiation, AlwaysSuccessRequirement>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act & Assert
        var exceptionInfo = await Assert.ThrowsAsync<RegisteredHandlerInstantiationFailureException>(async () => await RunCheck(service));
        Assert.NotNull(exceptionInfo.InnerException);
        Assert.Equal(RequirementHandlerThatCrashesOnInstantiation.CrashMessage, exceptionInfo.InnerException.Message);
    }

    private static async Task<RequestAuthorizationResult> RunCheck(IServiceProvider toTest)
    {
        var request = new TestRequest();
        await using var scope = toTest.CreateAsyncScope();
        var checker = scope.ServiceProvider.GetRequiredService<IRequestAuthorizationChecker<TestRequest>>();
        return await checker.CheckAuthorization(request, CancellationToken.None);
    }
}
