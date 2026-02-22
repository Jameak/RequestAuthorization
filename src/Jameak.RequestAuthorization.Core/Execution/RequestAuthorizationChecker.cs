using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Configuration;
using Jameak.RequestAuthorization.Core.DependencyInjection;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Requirements;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Execution;

internal sealed class RequestAuthorizationChecker<TRequest> : IRequestAuthorizationChecker<TRequest>
{
    private readonly IRequestAuthorizationRequirementBuilder<TRequest>[] _requirementBuilders;
    private readonly IEnumerable<IGlobalRequestAuthorizationRequirementBuilder> _globalRequirementBuilders;
    private readonly IRequestAuthorizationExecutor _executor;
    private readonly RequestAuthorizationOptions _options;

    public RequestAuthorizationChecker(
        IEnumerable<IRequestAuthorizationRequirementBuilder<TRequest>> requirementBuilders,
        IEnumerable<IGlobalRequestAuthorizationRequirementBuilder> globalRequirementBuilders,
        IRequestAuthorizationExecutor executor,
        RequestAuthorizationOptions options)
    {
        _requirementBuilders = requirementBuilders.ToArray();
        _globalRequirementBuilders = globalRequirementBuilders;
        _executor = executor;
        _options = options;
    }

    public async Task<RequestAuthorizationResult> CheckAuthorization(
        TRequest request,
        CancellationToken token)
    {
        var requirementBuilderValidationSuccess = (_options.RequirementBuilderValidation) switch
        {
            RequirementBuilderValidationKind.AtLeastOneBuilder => _requirementBuilders.Length >= 1,
            RequirementBuilderValidationKind.ExactlyOneBuilder => _requirementBuilders.Length == 1,
            RequirementBuilderValidationKind.ZeroOrOneBuilders => _requirementBuilders.Length <= 1,
            RequirementBuilderValidationKind.ZeroOrMoreBuilders => true,
            _ => throw new ArgumentOutOfRangeException($"Unknown enum value: {_options.RequirementBuilderValidation}"),
        };

        if (!requirementBuilderValidationSuccess)
        {
            throw new InvalidBuilderRegistrationException($"Retrieved {_requirementBuilders.Length} {nameof(IRequestAuthorizationRequirementBuilder<>)}-classes from the dependency container that can handle '{request?.GetType().FullName}' which doesn't match the config-value '{nameof(RequirementBuilderValidationKind)}.{_options.RequirementBuilderValidation}'. You need to either fix your builder-class declarations or change the {nameof(RequestAuthorizationOptions)}.{nameof(RequestAuthorizationOptions.RequirementBuilderValidation)}-option in your {nameof(ServiceCollectionExtensions.AddRequestAuthorizationCore)}(...) call.");
        }

        List<IRequestAuthorizationRequirement> requirements = [];

        foreach (var globalBuilder in _globalRequirementBuilders)
        {
            try
            {
                requirements.Add(await globalBuilder.BuildRequirementAsync(request, token));
            }
            catch (Exception ex)
            {
                return RequirementBuildingFailed($"Global requirement building failed with exception. See {nameof(RequestAuthorizationResult.FailureException)} for details.", ex);
            }
        }

        foreach (var requirementBuilder in _requirementBuilders)
        {
            try
            {
                requirements.Add(await requirementBuilder.BuildRequirementAsync(request, token));
            }
            catch (Exception ex)
            {
                return RequirementBuildingFailed($"Requirement building failed with exception. See {nameof(RequestAuthorizationResult.FailureException)} for details.", ex);
            }
        }

        return await CheckRequirementsAsync(requirements, token);

        static RequestAuthorizationResult RequirementBuildingFailed(string message, Exception ex)
        {
            return RequestAuthorizationResult.Fail(
                requirement: new RequirementBuildingMustNotThrowExceptionRequirement(),
                failureReason: message,
                failureException: ex);
        }
    }

    private async Task<RequestAuthorizationResult> CheckRequirementsAsync(
        List<IRequestAuthorizationRequirement> requirements,
        CancellationToken token)
    {
        if (requirements.Count == 0)
        {
            return RequestAuthorizationResult.Success(new RequirementBuildingProducedNoRequirements());
        }

        IRequestAuthorizationRequirement requirementToCheck;
        if (requirements.Count == 1)
        {
            requirementToCheck = requirements.Single();
        }
        else
        {
            requirementToCheck = Require.All([.. requirements]);
        }

        RequestAuthorizationResult authResult;
        try
        {
            authResult = await _executor.ExecuteAsync(requirementToCheck, token);
        }
        catch (Exception ex) when (ExceptionUtility.ShouldNotBeWrapped(ex))
        {
            throw;
        }
        catch (Exception ex)
        {
            authResult = RequestAuthorizationResult.Fail(
                requirement: requirementToCheck,
                failureReason: $"Requirement checking failed with exception. See {nameof(RequestAuthorizationResult.FailureException)} for details.",
                failureException: ex);
        }

        return authResult;
    }
}
