using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Results;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Moq;

namespace Jameak.RequestAuthorization.Core.Tests.Execution;

public class AuthorizationPipelineStepTests
{
    [Fact]
    public async Task Handle_OnCheckFailure_ReturnsUnauthorizedResult()
    {
        // Arrange
        var requestObj = new object();
        var expectedResponseObj = new object();
        var authResult = RequestAuthorizationResult.Fail(new TestRequirement());
        var cancelToken = CancellationToken.None;
        var authedResultHandlerMock = new Mock<IAuthorizedResultHandler>(MockBehavior.Strict);
        var unauthedResultHandlerMock = new Mock<IUnauthorizedResultHandler>(MockBehavior.Strict);
        var checkerMock = new Mock<IRequestAuthorizationChecker<object>>(MockBehavior.Strict);
        static Task<object> next(CancellationToken token)
        {
            Assert.Fail("Should not be called");
            throw new Exception("Test has already failed, this is just to satisfy the return type");
        }

        unauthedResultHandlerMock
            .Setup(e => e.OnUnauthorized<object, object>(requestObj, authResult, cancelToken))
            .ReturnsAsync(expectedResponseObj);
        checkerMock
            .Setup(e => e.CheckAuthorization(requestObj, cancelToken))
            .ReturnsAsync(authResult);

        // Act
        var sut = new AuthorizationPipelineStep<object, object>(
            checkerMock.Object,
            authedResultHandlerMock.Object,
            unauthedResultHandlerMock.Object);

        var result = await sut.Handle(requestObj, next, cancelToken);

        // Assert
        Assert.Equal(expectedResponseObj, result);
        unauthedResultHandlerMock.Verify(e => e.OnUnauthorized<object, object>(requestObj, authResult, cancelToken), Times.Once);
        authedResultHandlerMock.Verify(e => e.OnAuthorized<object>(requestObj, authResult, cancelToken), Times.Never);
        checkerMock.Verify(e => e.CheckAuthorization(requestObj, cancelToken), Times.Once);
    }

    [Fact]
    public async Task Handle_OnCheckSuccess_PipelineContinues()
    {
        // Arrange
        var requestObj = new object();
        var expectedResponseObj = new object();
        var authResult = RequestAuthorizationResult.Success(new TestRequirement());
        var cancelToken = CancellationToken.None;
        var authedResultHandlerMock = new Mock<IAuthorizedResultHandler>(MockBehavior.Strict);
        var unauthedResultHandlerMock = new Mock<IUnauthorizedResultHandler>(MockBehavior.Strict);
        var checkerMock = new Mock<IRequestAuthorizationChecker<object>>(MockBehavior.Strict);
        Task<object> next(CancellationToken token) => Task.FromResult(expectedResponseObj);

        authedResultHandlerMock
            .Setup(e => e.OnAuthorized(requestObj, authResult, cancelToken)).Returns(Task.CompletedTask);
        checkerMock
            .Setup(e => e.CheckAuthorization(requestObj, cancelToken))
            .ReturnsAsync(authResult);

        // Act
        var sut = new AuthorizationPipelineStep<object, object>(
            checkerMock.Object,
            authedResultHandlerMock.Object,
            unauthedResultHandlerMock.Object);

        var result = await sut.Handle(requestObj, next, cancelToken);

        // Assert
        Assert.Equal(expectedResponseObj, result);
        unauthedResultHandlerMock.Verify(e => e.OnUnauthorized<object, object>(requestObj, authResult, cancelToken), Times.Never);
        authedResultHandlerMock.Verify(e => e.OnAuthorized<object>(requestObj, authResult, cancelToken), Times.Once);
        checkerMock.Verify(e => e.CheckAuthorization(requestObj, cancelToken), Times.Once);
    }
}
