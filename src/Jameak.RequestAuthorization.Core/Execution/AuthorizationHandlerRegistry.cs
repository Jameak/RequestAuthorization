using System.Collections.Frozen;
using System.Globalization;
using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Exceptions;
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
                // Allow duplicate registrations of handlers for the same requirement as long as they are the same handler-type.
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
            var exceptionDetails = string.Join('\n', invalidRegistrationTypes.OrderBy(kvp => kvp.Key.FullName).Select((kvp, i) => $"""
                {(i + 1).ToString(CultureInfo.InvariantCulture)}. Requirement type: {kvp.Key.FullName}
                {Indent(i + 1)}Handlers:
                {Indent(i + 1)}- {string.Join($"\n{Indent(i + 1)}- ", kvp.Value.OrderBy(handlerType => handlerType.FullName).Select(handlerType => handlerType.FullName))}
                """));

            throw new InvalidHandlerRegistrationException($"Multiple different handler types registered for one or more requirements.\n{exceptionDetails}");
        }

        _map = map.ToFrozenDictionary();

        static string Indent(int index)
        {
            return new string(' ', $"{(index + 1).ToString(CultureInfo.InvariantCulture)}. ".Length);
        }
    }

    internal IRequestAuthorizationHandler GetHandler(IServiceProvider serviceProvider, IRequestAuthorizationRequirement requirement)
    {
        var reqType = requirement.GetType();

        if (!_map.TryGetValue(reqType, out var handlerType))
        {
            throw new MissingHandlerRegistrationException($"No handler registered for requirement type: {reqType.FullName}");
        }

        try
        {
            return (IRequestAuthorizationHandler)serviceProvider.GetRequiredService(handlerType);
        }
        catch (Exception ex)
        {
            throw new RegisteredHandlerInstantiationFailureException($"Retrieving handler '{handlerType.FullName}' from service provider failed. See inner exception for details.", ex);
        }
    }
}
