using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Results;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;

namespace Jameak.RequestAuthorization.Core.Tests.Results;

public class DefaultAuthorizedResultHandlerTests
{
    [Fact]
    public async Task OnUnauthorized_ThrowsException()
    {
        var authFailure = RequestAuthorizationResult.Fail(
            new TestRequirement(),
            failureReason: "reason",
            failureException: new Exception());

        var handler = new DefaultUnauthorizedResultHandler();

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(async () => await handler.OnUnauthorized<object, object>(new object(), authFailure, CancellationToken.None));
        Assert.Equal(authFailure, exception.AuthResult);
        Assert.Equal(authFailure.FailureException, exception.InnerException);
        Assert.Equal(authFailure.FailureReason, exception.Message);
    }

    [Fact]
    public async Task OnUnauthorizedStream_ThrowsException()
    {
        var authFailure = RequestAuthorizationResult.Fail(
            new TestRequirement(),
            failureReason: "reason",
            failureException: new Exception());

        var handler = new DefaultUnauthorizedResultHandler();

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(async () => await handler.OnUnauthorizedStream<object, object>(new object(), authFailure, CancellationToken.None));
        Assert.Equal(authFailure, exception.AuthResult);
        Assert.Equal(authFailure.FailureException, exception.InnerException);
        Assert.Equal(authFailure.FailureReason, exception.Message);
    }
}
