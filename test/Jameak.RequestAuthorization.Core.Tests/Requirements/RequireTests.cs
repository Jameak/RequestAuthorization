using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;

namespace Jameak.RequestAuthorization.Core.Tests.Requirements;

public class RequireTests
{
    [Fact]
    public void Any_Or_ProducesEquivalentComposition()
    {
        var requirementOne = new TestRequirement();
        var requirementTwo = new TestRequirement();
        var expected = new AnyRequirement([requirementOne, requirementTwo]);

        var actualOne = Require.Any(requirementOne, requirementTwo);
        var actualTwo = requirementOne.Or(requirementTwo);

        AssertRequirementsAreEquivalent(expected, actualOne);
        AssertRequirementsAreEquivalent(expected, actualTwo);
    }

    [Fact]
    public void All_And_ProducesEquivalentComposition()
    {
        var requirementOne = new TestRequirement();
        var requirementTwo = new TestRequirement();
        var expected = new AllRequirement([requirementOne, requirementTwo]);

        var actualOne = Require.All(requirementOne, requirementTwo);
        var actualTwo = requirementOne.And(requirementTwo);

        AssertRequirementsAreEquivalent(expected, actualOne);
        AssertRequirementsAreEquivalent(expected, actualTwo);
    }

    private static void AssertRequirementsAreEquivalent(AllRequirement expected, IRequestAuthorizationRequirement actual)
    {
        if (actual is not AllRequirement actualTyped)
        {
            Assert.Fail();
            return;
        }

        Assert.Equal(expected.Requirements.Count, actualTyped.Requirements.Count);
        for (var i = 0; i < expected.Requirements.Count; i++)
        {
            var expectedRequirement = expected.Requirements[i];
            var actualRequirement = actualTyped.Requirements[i];
            Assert.Equal(expectedRequirement, actualRequirement);
        }
    }

    private static void AssertRequirementsAreEquivalent(AnyRequirement expected, IRequestAuthorizationRequirement actual)
    {
        if (actual is not AnyRequirement actualTyped)
        {
            Assert.Fail();
            return;
        }

        Assert.Equal(expected.Requirements.Count, actualTyped.Requirements.Count);
        for (var i = 0; i < expected.Requirements.Count; i++)
        {
            var expectedRequirement = expected.Requirements[i];
            var actualRequirement = actualTyped.Requirements[i];
            Assert.Equal(expectedRequirement, actualRequirement);
        }
    }
}
