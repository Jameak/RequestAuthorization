using Jameak.RequestAuthorization.Core.Abstractions;

namespace Jameak.RequestAuthorization.Core.Results;

internal class RequestAuthorizationResultAccessor : IRequestAuthorizationResultAccessor
{
    private static readonly AsyncLocal<AuthResultHolder> s_authorizationResultCurrent = new();

    public RequestAuthorizationResult? AuthorizationResult
    {
        get
        {
            return s_authorizationResultCurrent.Value?.Result;
        }
        set
        {
            s_authorizationResultCurrent.Value?.Result = null;

            if (value != null)
            {
                s_authorizationResultCurrent.Value = new AuthResultHolder { Result = value };
            }
        }
    }

    private sealed class AuthResultHolder
    {
        public RequestAuthorizationResult? Result;
    }
}
