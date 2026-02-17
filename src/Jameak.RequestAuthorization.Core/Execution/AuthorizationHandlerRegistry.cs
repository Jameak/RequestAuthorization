using System.Collections.Frozen;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Jameak.RequestAuthorization.Core.Execution;

internal sealed class AuthorizationHandlerRegistry
{
    private readonly FrozenDictionary<Type, Type> _map;

    public AuthorizationHandlerRegistry(IEnumerable<AuthorizationHandlerRegistrar> registrars)
    {
        var map = new Dictionary<Type, Type>();
        var invalidRegistrationTypes = new Dictionary<Type, HashSet<Type>>();
        foreach (var typeTuple in registrars.SelectMany(e => e.TypesToRegister))
        {
            if (map.TryGetValue(typeTuple.requirementType, out var alreadyRegisteredHandlerType))
            {
                if (alreadyRegisteredHandlerType == typeTuple.handlerType)
                {
                    continue;
                }

                var hashSet = invalidRegistrationTypes.GetValueOrDefault(typeTuple.requirementType, new HashSet<Type>());
                hashSet.Add(alreadyRegisteredHandlerType);
                hashSet.Add(typeTuple.handlerType);
                invalidRegistrationTypes[typeTuple.requirementType] = hashSet;
            }

            map[typeTuple.requirementType] = typeTuple.handlerType;
        }

        if (invalidRegistrationTypes.Count > 0)
        {
            var exceptionDetails = string.Join("\n", invalidRegistrationTypes.OrderBy(kvp => kvp.Key.FullName).Select((kvp, i) => $"""
                {i}. Requirement type: {kvp.Key.FullName}
                {new string(' ', $"{i}. ".Length)}Handlers:
                {new string(' ', $"{i}. ".Length)}{string.Join("\n- ", kvp.Value.OrderBy(handlerType => handlerType.FullName).Select(handlerType => handlerType.FullName))}
                """));


#pragma warning disable MA0026 // Fix TODO comment
            // TODO: Should we allow this? If yes, make it configurable on the options whether to allow it or throw.
            throw new InvalidHandlerRegistrationException($"Multiple different handler types registered for one or more requirements.\n{exceptionDetails}");
#pragma warning restore MA0026 // Fix TODO comment
        }

        _map = map.ToFrozenDictionary();
    }

    internal IRequestAuthorizationHandler GetHandler(IServiceProvider serviceProvider, IRequestAuthorizationRequirement requirement)
    {
        var reqType = requirement.GetType();

        if (!_map.TryGetValue(reqType, out var handlerType))
        {
            throw new InvalidOperationException($"No handler registered for requirement type {reqType.FullName}");
        }

        return (IRequestAuthorizationHandler)serviceProvider.GetRequiredService(handlerType);
    }
}
