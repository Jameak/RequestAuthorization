using Jameak.RequestAuthorization.Sample.Mediator;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Jameak.RequestAuthorization.Sample.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<GetDocumentResponse> GetDocument(GetDocumentRequest documentRequest, CancellationToken token)
    {
        return await _mediator.Send(documentRequest, token);
    }
}
