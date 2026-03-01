using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;
using Jameak.RequestAuthorization.MediatorNativeAotSample.Services;

namespace Jameak.RequestAuthorization.MediatorNativeAotSample.Mediator;

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
