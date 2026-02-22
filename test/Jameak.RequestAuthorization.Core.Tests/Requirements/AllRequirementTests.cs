using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Tests.TestUtilities;

namespace Jameak.RequestAuthorization.Core.Tests.Requirements;

public class AllRequirementTests
{
    [Fact]
    public void Constructor_SavesRequirements()
    {
        var requirementOne = new TestRequirement();
        var requirementTwo = new TestRequirement();

        var all = new AllRequirement([requirementOne, requirementTwo]);

        Assert.Equal(2, all.Requirements.Count);
        Assert.Equal(requirementOne, all.Requirements[0]);
        Assert.Equal(requirementTwo, all.Requirements[1]);
    }

    [Fact]
    public void Constructor_WithSingleArgThrows()
    {
        var requirementOne = new TestRequirement();

        Assert.Throws<ArgumentException>(() => new AllRequirement([requirementOne]));
    }
}
