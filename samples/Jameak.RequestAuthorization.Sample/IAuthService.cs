namespace Jameak.RequestAuthorization.Sample;

public interface IAuthService
{
    Task<bool> UserCanAccessDocument(Guid userId, Guid documentId);
    bool IsAllowed<T>(T toCheck);
}
