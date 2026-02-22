using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.HandlerTypesForAssemblyScanTests;

public abstract class AbstractRequestHandler : RequestAuthorizationHandlerBase<AlwaysSuccessRequirement>;
