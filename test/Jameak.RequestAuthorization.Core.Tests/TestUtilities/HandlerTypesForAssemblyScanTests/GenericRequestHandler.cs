using Jameak.RequestAuthorization.Core.Abstractions;
using Jameak.RequestAuthorization.Core.Results;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.HandlerTypesForAssemblyScanTests;

internal class GenericRequestHandler<T> : RequestAuthorizationHandlerBase<T> where T : IRequestAuthorizationRequirement
{
    public override Task<RequestAuthorizationResult> CheckRequirementAsync(T requirement, CancellationToken token) => throw new NotImplementedException();
}
