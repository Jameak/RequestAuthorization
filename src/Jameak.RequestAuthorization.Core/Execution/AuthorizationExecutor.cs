using System.Runtime.CompilerServices;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Internal;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Execution;

internal sealed class AuthorizationExecutor : IRequestAuthorizationExecutor
{
    private readonly AuthorizationHandlerRegistry _registry;
    private readonly IServiceProvider _serviceProvider;
    // Authorization graphs must be tracked by reference equality, not value equality, in case users make use of 'record' classes.
    private readonly HashSet<IRequestAuthorizationRequirement> _visited = new(ReferenceEqualityComparer<IRequestAuthorizationRequirement>.Instance);

    public AuthorizationExecutor(
        AuthorizationHandlerRegistry registry,
        IServiceProvider serviceProvider)
    {
        _registry = registry;
        _serviceProvider = serviceProvider;
    }

    public async Task<RequestAuthorizationResult> ExecuteAsync(
        IRequestAuthorizationRequirement requirement,
        CancellationToken token)
    {
        if (!_visited.Add(requirement))
        {
            return RequestAuthorizationResult.Fail(requirement,
                failureReason: "Circular requirement detected");
        }

        var handler = _registry.GetHandler(_serviceProvider, requirement);
        return await handler.CheckRequirementAsync(requirement, token);
    }

    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly ReferenceEqualityComparer<T> Instance = new();

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
