namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal interface ICustomerRequest
{
    public Guid CustomerId { get; }
}

internal class SomeKindOfCustomerRequest : ICustomerRequest
{
    public Guid CustomerId => Guid.NewGuid();
}
