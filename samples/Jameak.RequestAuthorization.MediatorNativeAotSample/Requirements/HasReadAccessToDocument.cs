using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.MediatorNativeAotSample.Requirements;

public record HasReadAccessToDocument(
    Guid DocumentId,
    Guid UserId)
    : IRequestAuthorizationRequirement;
