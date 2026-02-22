using Jameak.RequestAuthorization.Core.Diagnostics;
using Jameak.RequestAuthorization.Core.Results;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;

namespace Jameak.RequestAuthorization.Core.Tests.Results;

public class RequestAuthorizationResultTests
{
    [Fact]
    public void Success_ProducesAuthorizedResult()
    {
        // Act
        var requirement = new TestRequirement();
        var diagnostic = new AuthorizationDiagnostic();
        var context = new object();

        // Act
        var authResult = RequestAuthorizationResult.Success(
            requirement,
            diagnostic: diagnostic,
            context: context);

        // Assert
        Assert.True(authResult.IsAuthorized);
        Assert.Equal(requirement, authResult.Requirement);
        Assert.Equal(diagnostic, authResult.Diagnostic);
        Assert.Equal(context, authResult.Context);
    }

    [Fact]
    public void Failure_ProducesUnauthorizedResult()
    {
        // Act
        var requirement = new TestRequirement();
        var reason = "reason";
        var exception = new Exception(reason);
        var diagnostic = new AuthorizationDiagnostic();
        var context = new object();

        // Act
        var authResult = RequestAuthorizationResult.Fail(
            requirement,
            failureReason: reason,
            failureException: exception,
            diagnostic: diagnostic,
            context: context);

        // Assert
        Assert.False(authResult.IsAuthorized);
        Assert.Equal(requirement, authResult.Requirement);
        Assert.Equal(reason, authResult.FailureReason);
        Assert.Equal(exception, authResult.FailureException);
        Assert.Equal(diagnostic, authResult.Diagnostic);
        Assert.Equal(context, authResult.Context);
    }
}
