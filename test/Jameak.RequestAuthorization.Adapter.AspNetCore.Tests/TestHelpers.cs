using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Jameak.RequestAuthorization.Adapter.AspNetCore.Tests;

internal static class TestHelpers
{
    public static Mock<IHttpContextAccessor> CreateHttpContextAccessorMock(out ClaimsPrincipal user)
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        var httpContext = new DefaultHttpContext();
        user = new ClaimsPrincipal();
        httpContext.User = user;
        httpContextAccessorMock.Setup(e => e.HttpContext).Returns(httpContext);
        return httpContextAccessorMock;
    }

    public static AuthorizationResult CreateAspNetAuthResult(bool createSuccess, string failureMessage)
    {
        if (createSuccess)
        {
            return AuthorizationResult.Success();
        }

        return AuthorizationResult.Failed(AuthorizationFailure.Failed([new AuthorizationFailureReason(handler: null!, message: failureMessage)]));
    }

    public record SampleRequest();

    public class AspNetTestRequirement : IAuthorizationRequirement;
}
