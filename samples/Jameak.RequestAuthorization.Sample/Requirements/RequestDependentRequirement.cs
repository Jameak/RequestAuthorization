using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Sample.Mediator;

namespace Jameak.RequestAuthorization.Sample.Requirements;

public class RequestDependentRequirement : IRequestAuthorizationRequirement
{
    public RequestDependentRequirement(SampleRequest request)
    {
        Request = request;
    }

    public SampleRequest Request { get; }
}
