using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.HandlerTypesForAssemblyScanTests;

public abstract class GenericAbstractRequestHandler<T> : RequestAuthorizationHandlerBase<T> where T : IRequestAuthorizationRequirement;
