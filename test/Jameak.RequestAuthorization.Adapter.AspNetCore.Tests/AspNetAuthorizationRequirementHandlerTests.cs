using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Jameak.RequestAuthorization.Adapter.AspNetCore.Tests;

public class AspNetAuthorizationRequirementHandlerTests
{
    private static readonly object s_resourceObj = new();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunCheckerWithAspNetRequirements_ReturnsAppropriateAuthResult(bool expectSuccess)
    {
        // Arrange
        var testFailureReason = "Test reason";
        var aspNetAuthResult = TestHelpers.CreateAspNetAuthResult(expectSuccess, testFailureReason);
        var httpContextAccessorMock = TestHelpers.CreateHttpContextAccessorMock(out var user);

        var aspNetAuthServiceMock = new Mock<IAuthorizationService>(MockBehavior.Strict);
        aspNetAuthServiceMock.Setup(e => e.AuthorizeAsync(user, s_resourceObj, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(aspNetAuthResult);

        var serviceCollection = new ServiceCollection()
            .AddSingleton<IAuthorizationService>(aspNetAuthServiceMock.Object)
            .AddSingleton<IHttpContextAccessor>(httpContextAccessorMock.Object);
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderType<SampleRequestAspNetRequirementsBuilder, TestHelpers.SampleRequest>()
            .AddAspNetAdapter();

        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var authChecker = scope.ServiceProvider.GetRequiredService<IRequestAuthorizationChecker<TestHelpers.SampleRequest>>();
        var requestObj = new TestHelpers.SampleRequest();

        // Act
        var result = await authChecker.CheckAuthorization(requestObj, CancellationToken.None);

        // Assert
        Assert.Equal(expectSuccess, result.IsAuthorized);
        Assert.Equal(typeof(AspNetAuthorizationRequirement), result.Requirement.GetType());

        if (!expectSuccess)
        {
            Assert.Equal(aspNetAuthResult, result.Context);
            Assert.Equal(testFailureReason, result.FailureReason);
        }
    }

    internal class SampleRequestAspNetRequirementsBuilder : IRequestAuthorizationRequirementBuilder<TestHelpers.SampleRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            TestHelpers.SampleRequest request,
            CancellationToken token)
        {
            var requirement = new AspNetAuthorizationRequirement([new TestHelpers.AspNetTestRequirement()], resource: s_resourceObj);
            return Task.FromResult<IRequestAuthorizationRequirement>(requirement);
        }
    }
}
