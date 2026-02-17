using System.Collections.Immutable;

namespace Jameak.RequestAuthorization.Core.Execution;

internal sealed class AuthorizationHandlerRegistrar
{
    public ImmutableList<(Type handlerType, Type requirementType)> TypesToRegister { get; init; }


#pragma warning disable MA0026 // Fix TODO comment
    // TODO Make sure to write proper tests that this is actually needed. This type was created to handle the situation where a library user calls the DI-registration method multiple times, since each registration can overwrite the previous one, and we want it to be additive.
    public AuthorizationHandlerRegistrar(IReadOnlyList<(Type handlerType, Type requirementType)> toRegister)
#pragma warning restore MA0026 // Fix TODO comment
    {
        TypesToRegister = toRegister.ToImmutableList();
    }
}
