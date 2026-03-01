namespace Jameak.RequestAuthorization.MediatorNativeAotSample.Services;

public interface IDocumentService
{
    Task<Document> GetDocument(Guid documentId);
}

public class FakeDocumentService : IDocumentService
{
    public Task<Document> GetDocument(Guid documentId) => Task.FromResult(
        new Document(
            Id: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Content: "Document content",
            DocumentOwner: FakeUserAccessor.HardcodedUserId));
}

public record Document(Guid Id, string Content, Guid DocumentOwner);
