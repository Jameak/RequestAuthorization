using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Requirements;

internal sealed class FuncRequirementBuilder<TRequest> : IRequestAuthorizationRequirementBuilder<TRequest>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, TRequest, Task<IRequestAuthorizationRequirement>> _evaluationFunc;

    public FuncRequirementBuilder(IServiceProvider serviceProvider, Func<IServiceProvider, TRequest, Task<IRequestAuthorizationRequirement>> evaluationFunc)
    {
        _serviceProvider = serviceProvider;
        _evaluationFunc = evaluationFunc;
    }

    public async Task<IRequestAuthorizationRequirement> BuildRequirementAsync(TRequest request, CancellationToken token)
    {
        return await _evaluationFunc(_serviceProvider, request);
    }
}
