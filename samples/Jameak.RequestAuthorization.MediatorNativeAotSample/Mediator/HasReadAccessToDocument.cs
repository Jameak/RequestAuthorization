using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.MediatorNativeAotSample.Mediator;

public record HasReadAccessToDocument(
    Guid DocumentId,
    Guid UserId)
    : IRequestAuthorizationRequirement;
