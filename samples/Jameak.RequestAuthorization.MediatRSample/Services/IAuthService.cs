namespace Jameak.RequestAuthorization.MediatRSample.Services;

public interface IAuthService
{
    bool IsAllowed<T>(T toCheck);
}

public class FakeAuthService : IAuthService
{
    private static readonly Random s_random = new(1234);

    public bool IsAllowed<T>(T toCheck)
    {
        lock (s_random)
        {
            return s_random.NextDouble() < 0.5;
        }
    }
}
