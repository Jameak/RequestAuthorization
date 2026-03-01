using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.MediatRSample.Requirements;

namespace Jameak.RequestAuthorization.MediatRSample.MediatR;

public sealed class SampleRequestAuthRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleRequest>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
        SampleRequest request,
        CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new RequestDependentRequirement(request));
    }
}
