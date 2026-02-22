using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Tests.TestUtilities.BuilderTypesForAssemblyScanTests;

internal interface GenericInterfaceRequestBuilder<T> : IRequestAuthorizationRequirementBuilder<T>
{
}
