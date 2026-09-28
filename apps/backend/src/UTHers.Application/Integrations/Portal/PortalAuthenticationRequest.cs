namespace UTHers.Application.Integrations.Portal;

public sealed class PortalAuthenticationRequest
{
    public PortalAuthenticationRequest(string username, string password, string challengeToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(challengeToken);

        Username = username;
        Password = password;
        ChallengeToken = challengeToken;
    }

    public string Username { get; }

    public string Password { get; }

    public string ChallengeToken { get; }

    public override string ToString() => $"{nameof(PortalAuthenticationRequest)} {{ Sensitive data redacted }}";
}
