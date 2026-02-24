using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Execution;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Jameak.RequestAuthorization.Adapter.AspNetCore.Tests;

public class AspNetAuthorizationPolicyRequirementHandlerTests
{
    private static readonly object s_resourceObj = new();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunCheckerWithAspNetPolicy_ByName_ReturnsAppropriateAuthResult(bool expectSuccess)
    {
        // Arrange
        var testFailureReason = "Test reason";
        var aspNetAuthResult = TestHelpers.CreateAspNetAuthResult(expectSuccess, testFailureReason);
        var httpContextAccessorMock = TestHelpers.CreateHttpContextAccessorMock(out var user);

        var aspNetAuthServiceMock = new Mock<IAuthorizationService>(MockBehavior.Strict);
        aspNetAuthServiceMock.Setup(e => e.AuthorizeAsync(user, s_resourceObj, SampleRequestAspNetPolicyBuilderByName.PolicyName)).ReturnsAsync(aspNetAuthResult);

        var serviceCollection = new ServiceCollection()
            .AddSingleton<IAuthorizationService>(aspNetAuthServiceMock.Object)
            .AddSingleton<IHttpContextAccessor>(httpContextAccessorMock.Object);
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderType<SampleRequestAspNetPolicyBuilderByName, TestHelpers.SampleRequest>()
            .AddAspNetAdapter();

        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var authChecker = scope.ServiceProvider.GetRequiredService<IRequestAuthorizationChecker<TestHelpers.SampleRequest>>();
        var requestObj = new TestHelpers.SampleRequest();

        // Act
        var result = await authChecker.CheckAuthorization(requestObj, CancellationToken.None);

        // Assert
        Assert.Equal(expectSuccess, result.IsAuthorized);
        Assert.Equal(typeof(AspNetAuthorizationPolicyRequirement), result.Requirement.GetType());

        if (!expectSuccess)
        {
            Assert.Equal(aspNetAuthResult, result.Context);
            Assert.Equal(testFailureReason, result.FailureReason);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunCheckerWithAspNetPolicy_ByPolicyType_ReturnsAppropriateAuthResult(bool expectSuccess)
    {
        // Arrange
        var testFailureReason = "Test reason";
        var aspNetAuthResult = TestHelpers.CreateAspNetAuthResult(expectSuccess, testFailureReason);
        var httpContextAccessorMock = TestHelpers.CreateHttpContextAccessorMock(out var user);

        var aspNetAuthServiceMock = new Mock<IAuthorizationService>(MockBehavior.Strict);
        // The method "AuthorizeAsync(this IAuthorizationService service, ClaimsPrincipal user, object? resource, AuthorizationPolicy policy)" is an extension-method and cant be mocked.
        // This mocks the underlying method that the extension-method calls.
        aspNetAuthServiceMock.Setup(e => e.AuthorizeAsync(user, s_resourceObj, It.IsAny<IEnumerable<IAuthorizationRequirement>>())).ReturnsAsync(aspNetAuthResult);

        var serviceCollection = new ServiceCollection()
            .AddSingleton<IAuthorizationService>(aspNetAuthServiceMock.Object)
            .AddSingleton<IHttpContextAccessor>(httpContextAccessorMock.Object);
        serviceCollection.AddRequestAuthorizationCore()
            .AddRequirementBuilderType<SampleRequestAspNetPolicyBuilderByType, TestHelpers.SampleRequest>()
            .AddAspNetAdapter();

        var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions() { ValidateOnBuild = true, ValidateScopes = true });
        await using var scope = serviceProvider.CreateAsyncScope();
        var authChecker = scope.ServiceProvider.GetRequiredService<IRequestAuthorizationChecker<TestHelpers.SampleRequest>>();
        var requestObj = new TestHelpers.SampleRequest();

        // Act
        var result = await authChecker.CheckAuthorization(requestObj, CancellationToken.None);

        // Assert
        Assert.Equal(expectSuccess, result.IsAuthorized);
        Assert.Equal(typeof(AspNetAuthorizationPolicyRequirement), result.Requirement.GetType());

        if (!expectSuccess)
        {
            Assert.Equal(aspNetAuthResult, result.Context);
            Assert.Equal(testFailureReason, result.FailureReason);
        }
    }

    internal class SampleRequestAspNetPolicyBuilderByName : IRequestAuthorizationRequirementBuilder<TestHelpers.SampleRequest>
    {
        public const string PolicyName = "TestPolicy";

        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            TestHelpers.SampleRequest request,
            CancellationToken token)
        {
            return Task.FromResult<IRequestAuthorizationRequirement>(new AspNetAuthorizationPolicyRequirement(PolicyName, resource: s_resourceObj));
        }
    }

    internal class SampleRequestAspNetPolicyBuilderByType : IRequestAuthorizationRequirementBuilder<TestHelpers.SampleRequest>
    {
        public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
            TestHelpers.SampleRequest request,
            CancellationToken token)
        {
            var policy = new AuthorizationPolicy([new TestHelpers.AspNetTestRequirement()], ["TestScheme"]);
            return Task.FromResult<IRequestAuthorizationRequirement>(new AspNetAuthorizationPolicyRequirement(policy, resource: s_resourceObj));
        }
    }
}
