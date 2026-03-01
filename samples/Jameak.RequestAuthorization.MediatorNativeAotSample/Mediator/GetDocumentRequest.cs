using System.Text.Json.Serialization;
using Mediator;

namespace Jameak.RequestAuthorization.MediatorNativeAotSample.Mediator;

public record GetDocumentRequest([property: JsonRequired] Guid DocumentId) : IRequest<GetDocumentResponse>;
