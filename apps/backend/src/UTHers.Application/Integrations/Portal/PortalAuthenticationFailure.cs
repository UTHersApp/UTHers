namespace UTHers.Application.Integrations.Portal;

public enum PortalAuthenticationFailure
{
    InvalidCredentials,
    AuthenticationRejected,
    UpstreamUnavailable,
    CallerCancelled,
    UpstreamTimeout,
    InvalidResponse,
    UnexpectedFailure
}
