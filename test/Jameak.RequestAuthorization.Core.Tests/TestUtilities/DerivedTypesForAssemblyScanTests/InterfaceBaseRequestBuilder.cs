using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.DerivedTypesForAssemblyScanTests;

internal interface InterfaceBaseRequestBuilder<T> : IRequestAuthorizationRequirementBuilder<IBaseRequest1<T>>;
