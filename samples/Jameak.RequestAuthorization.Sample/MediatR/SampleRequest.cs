using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Sample.Requirements;
using MediatR;

namespace Jameak.RequestAuthorization.Sample.MediatR;

public sealed record SampleRequest(string Data) : IRequest<SampleResponse>;

public sealed record SampleResponse(string Data);

public sealed class SampleRequestHandler : IRequestHandler<SampleRequest, SampleResponse>
{
    public Task<SampleResponse> Handle(SampleRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SampleResponse(request.Data));
    }
}

public sealed class SampleRequestAuthRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleRequest>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
        SampleRequest request,
        CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new MustBeAuthenticatedRequirement());
    }
}
