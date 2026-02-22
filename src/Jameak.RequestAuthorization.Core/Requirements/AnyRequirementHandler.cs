using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Diagnostics;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Requirements;

internal sealed class AnyRequirementHandler : RequestAuthorizationHandlerBase<AnyRequirement>
{
    private readonly IRequestAuthorizationExecutor _executor;

    public AnyRequirementHandler(IRequestAuthorizationExecutor executor)
    {
        _executor = executor;
    }

    public override async Task<RequestAuthorizationResult> CheckRequirementAsync(
        AnyRequirement requirement,
        CancellationToken token)
    {
        var evaluatedResults = new List<RequestAuthorizationResult>();
        var skippedRequirements = new List<IRequestAuthorizationRequirement>();
        var succeeded = false;
        foreach (var inner in requirement.Requirements)
        {
            if (succeeded)
            {
                skippedRequirements.Add(inner);
                continue;
            }

            try
            {
                var result = await _executor.ExecuteAsync(inner, token);
                evaluatedResults.Add(result);
                if (result.IsAuthorized)
                {
                    succeeded = true;
                }
            }
            catch (Exception ex) when (ExceptionUtility.ShouldNotBeWrapped(ex))
            {
                throw;
            }
            catch (Exception ex)
            {
                evaluatedResults.Add(RequestAuthorizationResult.Fail(
                    requirement: inner,
                    failureException: ex,
                    failureReason: $"Requirement check failed with exception. See {nameof(RequestAuthorizationResult.FailureException)} for details."));
            }
        }

        var diagnostic = new AuthorizationDiagnostic { EvaluatedChildren = evaluatedResults, SkippedChildren = skippedRequirements };
        return succeeded
            ? RequestAuthorizationResult.Success(requirement: requirement, diagnostic: diagnostic)
            : RequestAuthorizationResult.Fail(
                requirement: requirement,
                failureReason: $"No requirements succeeded. See {nameof(RequestAuthorizationResult.Diagnostic)} for details.",
                diagnostic: diagnostic);
    }
}
