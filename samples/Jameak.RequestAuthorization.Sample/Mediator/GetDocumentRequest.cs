using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;
using Mediator;

namespace Jameak.RequestAuthorization.Sample.Mediator;

public record GetDocumentRequest(Guid DocumentId) : IRequest<GetDocumentResponse>;

public record GetDocumentResponse(string Content);

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

public class GetDocumentRequestRequirementBuilder
    : IRequestAuthorizationRequirementBuilder<GetDocumentRequest>
{
    private readonly IUserAccessor _userAccessor;

    public GetDocumentRequestRequirementBuilder(IUserAccessor userAccessor)
    {
        _userAccessor = userAccessor;
    }

    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(
        GetDocumentRequest request,
        CancellationToken token)
    {
        IRequestAuthorizationRequirement requirement = new HasReadAccessToDocument(
            request.DocumentId,
            _userAccessor.CurrentUserId);

        return Task.FromResult(requirement);
    }
}

public record HasReadAccessToDocument(
    Guid DocumentId,
    Guid UserId)
    : IRequestAuthorizationRequirement;

public class HasReadAccessToDocumentRequirementHandler
    : RequestAuthorizationHandlerBase<HasReadAccessToDocument>
{
    private readonly IAuthService _authService;

    public HasReadAccessToDocumentRequirementHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public override async Task<RequestAuthorizationResult> CheckRequirementAsync(
        HasReadAccessToDocument requirement,
        CancellationToken token)
    {
        if (await _authService.UserCanAccessDocument(
            requirement.UserId,
            requirement.DocumentId))
        {
            return RequestAuthorizationResult.Success(requirement);
        }

        return RequestAuthorizationResult.Fail(
            requirement,
            failureReason: "User is not allowed to access document");
    }
}
