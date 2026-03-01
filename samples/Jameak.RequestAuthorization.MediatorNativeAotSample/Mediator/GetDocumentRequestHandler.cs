using Jameak.RequestAuthorization.MediatorNativeAotSample.Services;
using Mediator;

namespace Jameak.RequestAuthorization.MediatorNativeAotSample.Mediator;

public class GetDocumentRequestHandler : IRequestHandler<GetDocumentRequest, GetDocumentResponse>
{
    private readonly IDocumentService _documentService;

    public GetDocumentRequestHandler(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    public async ValueTask<GetDocumentResponse> Handle(
        GetDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _documentService.GetDocument(request.DocumentId);
        return new GetDocumentResponse(document.Content);
    }
}
