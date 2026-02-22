using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal class TestRequirement : IRequestAuthorizationRequirement
{
    public string? Text { get; }

    public TestRequirement(string? text = null)
    {
        Text = text;
    }

    public override string ToString() => $"{nameof(TestRequirement)} with \"{Text}\"";
}
