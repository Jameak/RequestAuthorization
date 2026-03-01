using MediatR;

namespace Jameak.RequestAuthorization.MediatRSample.MediatR;

public sealed record SampleRequest(string Data) : IRequest<SampleResponse>;
