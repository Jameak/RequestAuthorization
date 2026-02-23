using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Core.Tests.IntegrationTests;

public partial class RequestAuthorizationCheckerTests
{
    [Fact]
    public async Task AllRequirementWithAllSuccessProducesSuccessAndDiagnostics()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddRequirementBuilderType<AllRequirementWithOnlySuccessBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.True(checkResult.IsAuthorized);
        Assert.Equal(0, checkResult.Diagnostic?.SkippedChildren?.Count);
        Assert.Equal(2, checkResult.Diagnostic?.EvaluatedChildren?.Count);
    }

    public class AllRequirementWithOnlySuccessBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token)
        {
            return Task.FromResult(Require.All(new AlwaysSuccessRequirement(), new AlwaysSuccessRequirement()));
        }
    }

    [Fact]
    public async Task AllRequirementWithOneFailureProducesFailureAndDiagnostics_FailFirst()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddRequirementBuilderType<AllRequirementWithBothSuccessAndFailureFailFirstBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
        Assert.Equal(1, checkResult.Diagnostic?.SkippedChildren?.Count);
        Assert.Equal(1, checkResult.Diagnostic?.EvaluatedChildren?.Count);
    }

    public class AllRequirementWithBothSuccessAndFailureFailFirstBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token)
        {
            return Task.FromResult(Require.All(new AlwaysFailureRequirement(), new AlwaysSuccessRequirement()));
        }
    }

    [Fact]
    public async Task AllRequirementWithOneFailureProducesFailureAndDiagnostics_FailLast()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddRequirementBuilderType<AllRequirementWithBothSuccessAndFailureFailLastBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
        Assert.Equal(0, checkResult.Diagnostic?.SkippedChildren?.Count);
        Assert.Equal(2, checkResult.Diagnostic?.EvaluatedChildren?.Count);
    }

    public class AllRequirementWithBothSuccessAndFailureFailLastBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token)
        {
            return Task.FromResult(Require.All(new AlwaysSuccessRequirement(), new AlwaysFailureRequirement()));
        }
    }

    [Fact]
    public async Task AllRequirementWithCrashingRequirementProducesFailureAndDiagnostics()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddRequirementHandlerType<CrashingRequirementHandler, CrashingRequirement>()
            .AddRequirementBuilderType<AllRequirementWithSuccessAndCrashingRequirementBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
        Assert.Equal(0, checkResult.Diagnostic?.SkippedChildren?.Count);
        Assert.Equal(2, checkResult.Diagnostic?.EvaluatedChildren?.Count);
        Assert.Equal(CrashingRequirementHandler.CrashMessage, checkResult.Diagnostic?.EvaluatedChildren?[1].FailureException?.Message);
    }

    public class AllRequirementWithSuccessAndCrashingRequirementBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token)
        {
            return Task.FromResult(Require.All(new AlwaysSuccessRequirement(), new CrashingRequirement()));
        }
    }

    [Fact]
    public async Task AllRequirementWithNestedRequirementWithoutHandlerProducesException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderType<AllRequirementWithOnlySuccessBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });

        // Act & Assert
        await Assert.ThrowsAsync<MissingHandlerRegistrationException>(async () => await RunCheck(service));
    }
}
