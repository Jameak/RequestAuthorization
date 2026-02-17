using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Sample.Requirements;
using Mediator;

namespace Jameak.RequestAuthorization.Sample.Mediator;

public sealed record SampleRequest(string Data) : IRequest<SampleResponse>;

public sealed record SampleResponse(string Data);

public sealed class SampleRequestHandler : IRequestHandler<SampleRequest, SampleResponse>
{
    public ValueTask<SampleResponse> Handle(
        SampleRequest request,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new SampleResponse(request.Data));
    }
}

public sealed class SampleRequestAuthRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleRequest>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
        SampleRequest request,
        CancellationToken token)
    {
        return Task.FromResult(
            Require.Any(new RequestDependentRequirement(request), new RequestDependentRequirement(request), new RequestDependentRequirement(request)));
    }
}
