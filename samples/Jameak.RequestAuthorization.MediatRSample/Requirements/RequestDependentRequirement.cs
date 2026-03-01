using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.MediatRSample.MediatR;

namespace Jameak.RequestAuthorization.MediatRSample.Requirements;

public class RequestDependentRequirement : IRequestAuthorizationRequirement
{
    public RequestDependentRequirement(SampleRequest request)
    {
        Request = request;
    }

    public SampleRequest Request { get; }
}
