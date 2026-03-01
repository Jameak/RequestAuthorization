namespace Jameak.RequestAuthorization.MediatorNativeAotSample.Services;

public interface IAuthService
{
    Task<bool> UserCanAccessDocument(Guid userId, Guid documentId);
}

public class FakeAuthService : IAuthService
{
    public Task<bool> UserCanAccessDocument(Guid userId, Guid documentId) => Task.FromResult(true);
}
