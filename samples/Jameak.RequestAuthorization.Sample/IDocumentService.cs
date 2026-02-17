namespace Jameak.RequestAuthorization.Sample;

public interface IDocumentService
{
    Task<Document> GetDocument(Guid documentId);
}

public record Document(Guid Id, string Content, Guid DocumentOwner);
