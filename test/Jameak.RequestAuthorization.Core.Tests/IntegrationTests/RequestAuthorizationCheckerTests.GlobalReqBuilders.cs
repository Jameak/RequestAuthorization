using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Configuration;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Core.Tests.IntegrationTests;

public partial class RequestAuthorizationCheckerTests
{
    [Fact]
    public async Task GlobalBuilderOnlyAndBuilderProducesSuccessProducesAuthorized()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore(option => option.RequirementBuilderValidation = RequirementBuilderValidationKind.ZeroOrMoreBuilders)
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddGlobalRequirementBuilderType<GlobalRequirementBuilderProducingSuccessRequirement>();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.True(checkResult.IsAuthorized);
    }

    [Fact]
    public async Task GlobalBuilderOnlyAndBuilderProducesFailureProducesUnauthorized()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore(option => option.RequirementBuilderValidation = RequirementBuilderValidationKind.ZeroOrMoreBuilders)
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddGlobalRequirementBuilderType<GlobalRequirementBuilderProducingFailureRequirement>();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
    }

    [Fact]
    public async Task GlobalBuilderOnlyAndBuilderCrashesProducesUnauthorized()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore(option => option.RequirementBuilderValidation = RequirementBuilderValidationKind.ZeroOrMoreBuilders)
            .AddGlobalRequirementBuilderType<GlobalRequirementBuilderThatAlwaysCrashes>();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.False(checkResult.IsAuthorized);
        Assert.Equal(GlobalRequirementBuilderThatAlwaysCrashes.CrashMessage, checkResult.FailureException?.Message);
    }

    [Fact]
    public async Task MultipleGlobalBuildersAllContributeToAuthDecision()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore(option => option.RequirementBuilderValidation = RequirementBuilderValidationKind.ZeroOrMoreBuilders)
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>()
            .AddGlobalRequirementBuilderType<GlobalRequirementBuilderProducingSuccessRequirement>()
            .AddGlobalRequirementBuilderType<AnotherGlobalBuilderProducingSuccessRequirement>();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var checkResult = await RunCheck(service);

        // Assert
        Assert.True(checkResult.IsAuthorized);
        Assert.Equal(typeof(AllRequirement), checkResult.Requirement.GetType());
        Assert.Equal(2, checkResult.Diagnostic?.EvaluatedChildren?.Count);
    }

    internal class AnotherGlobalBuilderProducingSuccessRequirement : IGlobalRequestAuthorizationRequirementBuilder
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync<TRequest>(TRequest request, CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
        }
    }
}
