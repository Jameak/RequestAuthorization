namespace Jameak.RequestAuthorization.Sample;

public interface IUserAccessor
{
    Guid CurrentUserId { get; }
}
