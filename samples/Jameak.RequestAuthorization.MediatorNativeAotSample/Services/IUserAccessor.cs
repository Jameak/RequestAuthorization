namespace Jameak.RequestAuthorization.MediatorNativeAotSample.Services;

public interface IUserAccessor
{
    Guid CurrentUserId { get; }
}

public class FakeUserAccessor : IUserAccessor
{
    public static readonly Guid HardcodedUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public Guid CurrentUserId => HardcodedUserId;
}
