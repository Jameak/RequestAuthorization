using MediatR;

namespace Jameak.RequestAuthorization.MediatRSample.MediatR;

public sealed class SampleRequestHandler : IRequestHandler<SampleRequest, SampleResponse>
{
    public Task<SampleResponse> Handle(SampleRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SampleResponse(request.Data));
    }
}
