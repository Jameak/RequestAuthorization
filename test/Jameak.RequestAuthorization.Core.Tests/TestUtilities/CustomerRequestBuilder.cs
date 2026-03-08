using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities;

internal class CustomerRequestBuilderKind1 : IRequestAuthorizationRequirementBuilder<ICustomerRequest>
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(ICustomerRequest request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
    }
}
internal class CustomerRequestBuilderKind2<T> : IRequestAuthorizationRequirementBuilder<T> where T : ICustomerRequest
{
    public Task<IRequestAuthorizationRequirement> BuildRequirementAsync(T request, CancellationToken token)
    {
        return Task.FromResult<IRequestAuthorizationRequirement>(new AlwaysSuccessRequirement());
    }
}

