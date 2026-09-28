namespace UTHers.Api.Errors;

public static class ApiProblemTypes
{
    private const string BaseUri = "https://api.uthers.app/problems";

    public const string PortalInvalidCredentials = $"{BaseUri}/portal-invalid-credentials";
    public const string PortalAuthenticationRejected = $"{BaseUri}/portal-authentication-rejected";
    public const string PortalUnavailable = $"{BaseUri}/portal-unavailable";
    public const string PortalTimeout = $"{BaseUri}/portal-timeout";
    public const string PortalInvalidResponse = $"{BaseUri}/portal-invalid-response";
    public const string PortalUpstreamFailure = $"{BaseUri}/portal-upstream-failure";
}
