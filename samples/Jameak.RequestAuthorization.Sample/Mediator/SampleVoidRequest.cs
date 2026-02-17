using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Sample.Requirements;
using Mediator;

namespace Jameak.RequestAuthorization.Sample.Mediator;

public sealed record SampleVoidRequest(string Data) : IRequest;

public sealed class SampleVoidRequestHandler : IRequestHandler<SampleVoidRequest>
{
    public ValueTask<Unit> Handle(SampleVoidRequest request, CancellationToken cancellationToken)
    {
        return Unit.ValueTask;
    }
}

public sealed class SampleVoidRequestAuthRequirementBuilder : IRequestAuthorizationRequirementBuilder<SampleVoidRequest>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
        SampleVoidRequest request,
        CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(
            new MustBeAuthenticatedRequirement());
    }
}
