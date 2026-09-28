using UTHers.Application.Integrations.Portal;
using UTHers.Application.UniversityConnections.Portal;

namespace UTHers.UnitTests.UniversityConnections.Portal;

public sealed class ConnectPortalBoundaryTests
{
    private const string SanitizedUsername = "student-account";
    private const string SanitizedPassword = "not-a-real-password";
    private const string SanitizedChallenge = "not-a-real-challenge";

    [Fact]
    public void CommandContainsExactlyTheApplicationInputs()
    {
        var propertyNames = typeof(ConnectPortalCommand)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "ChallengeToken", "Password", "Username" },
            propertyNames);
    }

    [Fact]
    public void CommandRedactsAuthenticationMaterialFromStringRepresentation()
    {
        var command = new ConnectPortalCommand(
            SanitizedUsername,
            SanitizedPassword,
            SanitizedChallenge);

        var text = command.ToString();

        Assert.DoesNotContain(SanitizedUsername, text, StringComparison.Ordinal);
        Assert.DoesNotContain(SanitizedPassword, text, StringComparison.Ordinal);
        Assert.DoesNotContain(SanitizedChallenge, text, StringComparison.Ordinal);
    }

    [Fact]
    public void ConnectedResultContainsNoPortalCredential()
    {
        var result = ConnectPortalResult.Connected();

        Assert.True(result.IsConnected);
        Assert.Null(result.Failure);
        Assert.DoesNotContain(
            result.GetType().GetProperties(),
            property => property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CallerCancellationCannotBecomeANormalConnectionFailure()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ConnectPortalResult.Failed(PortalAuthenticationFailure.CallerCancelled));

        Assert.Equal("failure", exception.ParamName);
    }
}
