using Jameak.RequestAuthorization.Core.Configuration;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Core.Tests.IntegrationTests;

public partial class RequestAuthorizationCheckerTests
{
    [Fact]
    public async Task RequestHasNoBuilderAndOptionsDontAllowItThrowsException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore();
        var service = serviceCollection.BuildServiceProvider();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidBuilderRegistrationException>(async () => await RunCheck(service));
    }

    [Theory]
    [InlineData(RequirementBuilderValidationKind.ZeroOrMoreBuilders)]
    [InlineData(RequirementBuilderValidationKind.ZeroOrOneBuilders)]
    public async Task RequestHasNoBuilderAndOptionsAllowItReturnsAuthorized(RequirementBuilderValidationKind validationOption)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore(options => options.RequirementBuilderValidation = validationOption);
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var result = await RunCheck(service);

        // Assert
        Assert.True(result.IsAuthorized);
        Assert.Equal(typeof(RequirementBuildingProducedNoRequirements), result.Requirement.GetType());
    }

    [Fact]
    public async Task RequestHasMultipleBuildersAndOptionsDontAllowItThrowsException()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderType<AlwaysSuccessRequirementBuilder, TestRequest>()
            .AddRequirementBuilderType<AlwaysFailureRequirementBuilder, TestRequest>();
        var service = serviceCollection.BuildServiceProvider();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidBuilderRegistrationException>(async () => await RunCheck(service));
    }

    [Theory]
    [InlineData(RequirementBuilderValidationKind.AtLeastOneBuilder)]
    [InlineData(RequirementBuilderValidationKind.ZeroOrMoreBuilders)]
    public async Task RequestHasMultipleBuildersAndOptionsAllowItAuthorizedOnlyIfAllSuccessful(RequirementBuilderValidationKind validationOption)
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddRequestAuthorizationCore(options => options.RequirementBuilderValidation = validationOption)
            .AddRequirementBuilderType<AlwaysSuccessRequirementBuilder, TestRequest>()
            .AddRequirementBuilderType<AlwaysFailureRequirementBuilder, TestRequest>()
            .AddRequirementHandlerType<AlwaysFailureRequirementHandler, AlwaysFailureRequirement>()
            .AddRequirementHandlerType<AlwaysSuccessRequirementHandler, AlwaysSuccessRequirement>();
        var service = serviceCollection.BuildServiceProvider();

        // Act
        var result = await RunCheck(service);

        // Assert
        Assert.False(result.IsAuthorized);
        Assert.Equal(typeof(AllRequirement), result.Requirement.GetType());
    }
}
