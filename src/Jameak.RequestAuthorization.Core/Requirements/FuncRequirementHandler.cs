using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Requirements;

internal sealed class FuncRequirementHandler<TRequirement> : RequestAuthorizationHandlerBase<TRequirement> where TRequirement : IRequestAuthorizationRequirement
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Func<IServiceProvider, TRequirement, Task<RequestAuthorizationResult>> _evaluationFunc;

    public FuncRequirementHandler(IServiceProvider serviceProvider, Func<IServiceProvider, TRequirement, Task<RequestAuthorizationResult>> evaluationFunc)
    {
        _serviceProvider = serviceProvider;
        _evaluationFunc = evaluationFunc;
    }

    public override async Task<RequestAuthorizationResult> CheckRequirementAsync(TRequirement requirement, CancellationToken token)
    {
        return await _evaluationFunc(_serviceProvider, requirement);
    }
}
