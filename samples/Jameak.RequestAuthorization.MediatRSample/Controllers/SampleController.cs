using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.MediatRSample.MediatR;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jameak.RequestAuthorization.MediatRSample.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class SampleController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SampleController> _logger;

    public SampleController(IMediator mediator, ILogger<SampleController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetSample(CancellationToken token)
    {
        try
        {
            return Ok(await _mediator.Send(new SampleRequest("test"), token));
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogInformation(ex, "Request was unauthorized");
            return Unauthorized();
        }
    }
}
