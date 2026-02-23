using System.Runtime.CompilerServices;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Execution;

internal sealed class RequestAuthorizationExecutor : IRequestAuthorizationExecutor
{
    private readonly AuthorizationHandlerRegistry _registry;
    private readonly IServiceProvider _serviceProvider;
    private static readonly AsyncLocal<HashSet<IRequestAuthorizationRequirement>?> s_visited = new();

    public RequestAuthorizationExecutor(
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
        var isRoot = false;

        if (s_visited.Value is null)
        {
            // Requirements must be tracked by reference equality, not value equality, in case users make use of 'record' classes.
            s_visited.Value = new HashSet<IRequestAuthorizationRequirement>(ReferenceEqualityComparer<IRequestAuthorizationRequirement>.Instance);
            isRoot = true;
        }

        try
        {
            if (!s_visited.Value.Add(requirement))
            {
                throw new CircularRequirementException(requirement);
            }

            var handler = _registry.GetHandler(_serviceProvider, requirement);
            var result = await handler.CheckRequirementAsync(requirement, token);
            s_visited.Value.Remove(requirement);
            return result;
        }
        finally
        {
            if (isRoot)
            {
                s_visited.Value = null;
            }
        }
    }

    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly ReferenceEqualityComparer<T> Instance = new();

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
