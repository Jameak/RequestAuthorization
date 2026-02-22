using Jameak.RequestAuthorization.Core.Diagnostics;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Results;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;

namespace Jameak.RequestAuthorization.Core.Tests.Diagnostics;

public class AuthorizationDiagnosticExporterTests
{
    [Fact]
    public void ToGraphvizDot_MatchesExpectedOutput()
    {
        // Arrange
        var testDiagnostic = new AuthorizationDiagnostic
        {
            SkippedChildren = [
                new TestRequirement("req1"),
                new TestRequirement("req2").Or(new TestRequirement("req3"))],
            EvaluatedChildren = [
                RequestAuthorizationResult.Success(
                    new TestRequirement("req4")
                    .Or(new TestRequirement("req5")),
                    diagnostic: new AuthorizationDiagnostic
                    {
                        SkippedChildren = [
                            new TestRequirement("req9")],
                        EvaluatedChildren = [
                            RequestAuthorizationResult.Success(
                                Require.All(
                                    new TestRequirement("req10"),
                                    new TestRequirement("req11"),
                                    new TestRequirement("req12")),
                                diagnostic: new AuthorizationDiagnostic{
                                    SkippedChildren = [new TestRequirement("req13")]
                                })]
                    }),
                RequestAuthorizationResult.Fail(
                    new TestRequirement("req6")
                    .And(new TestRequirement("req7"))),
                RequestAuthorizationResult.Fail(new TestRequirement("req8"))]
        };

        var root = RequestAuthorizationResult.Success(new TestRequirement("root req"), diagnostic: testDiagnostic);

        var expected = """
            digraph Authorization {
              node [shape=box];
              n0 [label="TestRequirement with \"root req\"", color=green];
              n1 [label="AnyRequirement(2 Requirements)", color=green];
              n2 [label="AllRequirement(3 Requirements)", color=green];
              n3 [label="TestRequirement with \"req13\"", color=orange];
              n2 -> n3
              n1 -> n2
              n4 [label="TestRequirement with \"req9\"", color=orange];
              n1 -> n4
              n0 -> n1
              n5 [label="AllRequirement(2 Requirements)", color=red];
              n0 -> n5
              n6 [label="TestRequirement with \"req8\"", color=red];
              n0 -> n6
              n7 [label="TestRequirement with \"req1\"", color=orange];
              n0 -> n7
              n8 [label="AnyRequirement(2 Requirements)", color=orange];
              n0 -> n8
            }
            """.ReplaceLineEndings();

        // Act
        var graphResult = AuthorizationDiagnosticExporter.ToGraphvizDot(root);

        // Assert
        Assert.Equal(expected, graphResult.ReplaceLineEndings());
    }
}
