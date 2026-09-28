namespace UTHers.Api.Errors;

public static class ApiProblemCodes
{
    public const string PortalInvalidCredentials = "PORTAL_INVALID_CREDENTIALS";
    public const string PortalAuthenticationRejected = "PORTAL_AUTHENTICATION_REJECTED";
    public const string PortalUnavailable = "PORTAL_UNAVAILABLE";
    public const string PortalTimeout = "PORTAL_TIMEOUT";
    public const string PortalInvalidResponse = "PORTAL_INVALID_RESPONSE";
    public const string PortalUpstreamFailure = "PORTAL_UPSTREAM_FAILURE";
}
