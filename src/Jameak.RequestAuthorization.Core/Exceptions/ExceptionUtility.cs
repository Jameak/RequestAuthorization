namespace Jameak.RequestAuthorization.Core.Exceptions;

internal static class ExceptionUtility
{
    public static bool ShouldNotBeWrapped(Exception ex)
    {
        return ex is MissingHandlerRegistrationException || ex is RegisteredHandlerInstantiationFailureException;
    }
}
