using System.Runtime.CompilerServices;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Results;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;
using Moq;

namespace Jameak.RequestAuthorization.Core.Tests.Execution;

public class AuthorizationStreamPipelineStepTests
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
        var accessorMock = new Mock<IRequestAuthorizationResultAccessor>(MockBehavior.Strict);
        static IAsyncEnumerable<object> next(CancellationToken token)
        {
            Assert.Fail("Should not be called");
            throw new Exception("Test has already failed, this is just to satisfy the return type");
        }

        unauthedResultHandlerMock
            .Setup(e => e.OnUnauthorizedStream<object, object>(requestObj, authResult, cancelToken))
            .ReturnsAsync(expectedResponseObj);
        checkerMock
            .Setup(e => e.CheckAuthorization(requestObj, cancelToken))
            .ReturnsAsync(authResult);
        accessorMock
            .SetupSet(e => e.AuthorizationResult = It.IsAny<RequestAuthorizationResult>());

        // Act
        var sut = new AuthorizationStreamPipelineStep<object, object>(
            checkerMock.Object,
            authedResultHandlerMock.Object,
            unauthedResultHandlerMock.Object,
            accessorMock.Object);

        var result = await sut.Handle(requestObj, next, cancelToken).ToListAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal([expectedResponseObj], result);
        unauthedResultHandlerMock.Verify(e => e.OnUnauthorizedStream<object, object>(requestObj, authResult, cancelToken), Times.Once);
        authedResultHandlerMock.Verify(e => e.OnAuthorized(requestObj, authResult, cancelToken), Times.Never);
        checkerMock.Verify(e => e.CheckAuthorization(requestObj, cancelToken), Times.Once);
        accessorMock.VerifySet(e => e.AuthorizationResult = It.IsAny<RequestAuthorizationResult>(), Times.Once);
    }

    [Fact]
    public async Task Handle_OnCheckSuccess_PipelineContinues()
    {
        // Arrange
        var requestObj = new object();
        var expectedResponseObjOne = new object();
        var expectedResponseObjTwo = new object();
        var authResult = RequestAuthorizationResult.Success(new TestRequirement());
        var cancelToken = CancellationToken.None;
        var authedResultHandlerMock = new Mock<IAuthorizedResultHandler>(MockBehavior.Strict);
        var unauthedResultHandlerMock = new Mock<IUnauthorizedResultHandler>(MockBehavior.Strict);
        var checkerMock = new Mock<IRequestAuthorizationChecker<object>>(MockBehavior.Strict);
        var accessorMock = new Mock<IRequestAuthorizationResultAccessor>(MockBehavior.Strict);
        async IAsyncEnumerable<object> next([EnumeratorCancellation] CancellationToken token)
        {
            yield return expectedResponseObjOne;
            yield return expectedResponseObjTwo;
        }

        authedResultHandlerMock
            .Setup(e => e.OnAuthorized(requestObj, authResult, cancelToken)).Returns(Task.CompletedTask);
        checkerMock
            .Setup(e => e.CheckAuthorization(requestObj, cancelToken))
            .ReturnsAsync(authResult);
        accessorMock
            .SetupSet(e => e.AuthorizationResult = It.IsAny<RequestAuthorizationResult>());

        // Act
        var sut = new AuthorizationStreamPipelineStep<object, object>(
            checkerMock.Object,
            authedResultHandlerMock.Object,
            unauthedResultHandlerMock.Object,
            accessorMock.Object);

        var result = await sut.Handle(requestObj, next, cancelToken).ToListAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(expectedResponseObjOne, result[0]);
        Assert.Equal(expectedResponseObjTwo, result[1]);
        unauthedResultHandlerMock.Verify(e => e.OnUnauthorizedStream<object, object>(requestObj, authResult, cancelToken), Times.Never);
        authedResultHandlerMock.Verify(e => e.OnAuthorized(requestObj, authResult, cancelToken), Times.Once);
        checkerMock.Verify(e => e.CheckAuthorization(requestObj, cancelToken), Times.Once);
        accessorMock.VerifySet(e => e.AuthorizationResult = It.IsAny<RequestAuthorizationResult>(), Times.Once);
    }
}
