using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Core.Tests.IntegrationTests;

public partial class RequestAuthorizationCheckerTests
{
    [Fact]
    public async Task OrRequirementWithAllFailureProducesFailureAndDiagnostics()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddRequirementBuilderType<OrRequirementWithOnlyFailureBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
        Assert.Equal(0, checkResult.Diagnostic?.SkippedChildren?.Count);
        Assert.Equal(2, checkResult.Diagnostic?.EvaluatedChildren?.Count);
    }

    public class OrRequirementWithOnlyFailureBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token)
        {
            return Task.FromResult(Require.Any(new AlwaysFailureRequirement(), new AlwaysFailureRequirement()));
        }
    }

    [Fact]
    public async Task OrRequirementWithOneSuccessAndOneFailureProducesSuccessAndDiagnostics_SuccessFirst()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddRequirementBuilderType<OrRequirementWithBothSuccessAndFailureSuccessFirstBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.True(checkResult.IsAuthorized);
        Assert.Equal(1, checkResult.Diagnostic?.SkippedChildren?.Count);
        Assert.Equal(1, checkResult.Diagnostic?.EvaluatedChildren?.Count);
    }

    public class OrRequirementWithBothSuccessAndFailureSuccessFirstBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token)
        {
            return Task.FromResult(Require.Any(new AlwaysSuccessRequirement(), new AlwaysFailureRequirement()));
        }
    }

    [Fact]
    public async Task OrRequirementWithOneSuccessAndOnefailureProducesSuccessAndDiagnostics_SuccessLast()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddRequirementBuilderType<OrRequirementWithBothSuccessAndFailureSuccessLastBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.True(checkResult.IsAuthorized);
        Assert.Equal(0, checkResult.Diagnostic?.SkippedChildren?.Count);
        Assert.Equal(2, checkResult.Diagnostic?.EvaluatedChildren?.Count);
    }

    public class OrRequirementWithBothSuccessAndFailureSuccessLastBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token)
        {
            return Task.FromResult(Require.Any(new AlwaysFailureRequirement(), new AlwaysSuccessRequirement()));
        }
    }

    [Fact]
    public async Task OrRequirementWithFirstCrashingRequirementAndLastSuccessProducesSuccessAndDiagnostics()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddRequirementHandlerType<CrashingRequirementHandler, CrashingRequirement>()
            .AddRequirementBuilderType<OrRequirementWithCrashingRequirementAndSuccessLastBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.True(checkResult.IsAuthorized);
        Assert.Equal(0, checkResult.Diagnostic?.SkippedChildren?.Count);
        Assert.Equal(2, checkResult.Diagnostic?.EvaluatedChildren?.Count);
        Assert.Equal(CrashingRequirementHandler.CrashMessage, checkResult.Diagnostic?.EvaluatedChildren?[0].FailureException?.Message);
    }

    public class OrRequirementWithCrashingRequirementAndSuccessLastBuilder : IRequestAuthorizationRequirementBuilder<TestRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TestRequest request, CancellationToken token)
        {
            return Task.FromResult(Require.Any(new CrashingRequirement(), new AlwaysSuccessRequirement()));
        }
    }
}
