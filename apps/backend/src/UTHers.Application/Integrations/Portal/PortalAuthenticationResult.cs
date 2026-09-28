namespace UTHers.Application.Integrations.Portal;

public sealed class PortalAuthenticationResult
{
    private PortalAuthenticationResult(
        bool isAuthenticated,
        string? token,
        PortalAuthenticationFailure? failure)
    {
        IsAuthenticated = isAuthenticated;
        Token = token;
        Failure = failure;
    }

    public bool IsAuthenticated { get; }

    public string? Token { get; }

    public PortalAuthenticationFailure? Failure { get; }

    public static PortalAuthenticationResult Authenticated(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return new PortalAuthenticationResult(true, token, null);
    }

    public static PortalAuthenticationResult Failed(PortalAuthenticationFailure failure) =>
        new(false, null, failure);

    public override string ToString() =>
        IsAuthenticated
            ? $"{nameof(PortalAuthenticationResult)} {{ IsAuthenticated = true, Token = [REDACTED] }}"
            : $"{nameof(PortalAuthenticationResult)} {{ IsAuthenticated = false, Failure = {Failure} }}";
}
