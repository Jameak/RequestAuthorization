using System.Collections.Immutable;

namespace Jameak.RequestAuthorization.Core.Execution;

internal sealed class AuthorizationHandlerRegistrar
{
    public ImmutableList<(Type handlerType, Type requirementType)> TypesToRegister { get; init; }

    public AuthorizationHandlerRegistrar(IReadOnlyList<(Type handlerType, Type requirementType)> toRegister)
    {
        TypesToRegister = toRegister.ToImmutableList();
    }
}
