using Microsoft.AspNetCore.Mvc;

namespace Jameak.RequestAuthorization.Sample.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SampleController : ControllerBase
{
    private readonly global::Mediator.IMediator _mediator;
    private readonly global::MediatR.IMediator _mediatR;

    public SampleController(global::Mediator.IMediator mediator, global::MediatR.IMediator mediatR)
    {
        _mediator = mediator;
        _mediatR = mediatR;
    }

    [HttpGet]
    public async Task<Mediator.SampleResponse> MediatorTestEndpoint(CancellationToken token)
    {
        return await _mediator.Send(new Mediator.SampleRequest("test"), token);
    }

    [HttpGet]
    public async Task<MediatR.SampleResponse> MediatRTestEndpoint(CancellationToken token)
    {
        return await _mediatR.Send(new MediatR.SampleRequest("test"), token);
    }
}
