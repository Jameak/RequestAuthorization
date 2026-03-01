using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.MediatorNativeAotSample.Services;

namespace Jameak.RequestAuthorization.MediatorNativeAotSample.Mediator;

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
