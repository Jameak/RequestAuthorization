using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Diagnostics;
using Jameak.RequestAuthorization.Core.Exceptions;
using Jameak.RequestAuthorization.Core.Execution;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Requirements;

internal sealed class AllRequirementHandler : RequestAuthorizationHandlerBase<AllRequirement>
{
    private readonly IRequestAuthorizationExecutor _executor;

    public AllRequirementHandler(IRequestAuthorizationExecutor executor)
    {
        _executor = executor;
    }

    public override async Task<RequestAuthorizationResult> CheckRequirementAsync(
        AllRequirement requirement,
        CancellationToken token)
    {
        var evaluatedResults = new List<RequestAuthorizationResult>();
        var skippedRequirements = new List<IRequestAuthorizationRequirement>();
        var failed = false;
        foreach (var inner in requirement.Requirements)
        {
            if (failed)
            {
                skippedRequirements.Add(inner);
                continue;
            }

            try
            {
                var result = await _executor.ExecuteAsync(inner, token);
                evaluatedResults.Add(result);
                if (!result.IsAuthorized)
                {
                    failed = true;
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
                failed = true;
            }
        }

        var diagnostic = new AuthorizationDiagnostic { EvaluatedChildren = evaluatedResults, SkippedChildren = skippedRequirements };
        return failed
            ? RequestAuthorizationResult.Fail(
                requirement: requirement,
                failureReason: $"At least one requirement did not succeed. See {nameof(RequestAuthorizationResult.Diagnostic)} for details.",
                diagnostic: diagnostic)
            : RequestAuthorizationResult.Success(requirement: requirement, diagnostic: diagnostic);
    }
}
