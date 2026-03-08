using System.Text.Json.Serialization;
using Jameak.RequestAuthorization.MediatorNativeAotSample.Mediator;

namespace Jameak.RequestAuthorization.MediatorNativeAotSample;

[JsonSerializable(typeof(GetDocumentResponse))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;
