using UTHers.Api.UniversityConnections.Portal;

namespace UTHers.ApiTests.UniversityConnections.Portal;

public sealed class PortalConnectionApiBoundaryTests
{
    [Fact]
    public void FutureEndpointBoundaryIsStable()
    {
        Assert.Equal("POST", PortalConnectionApiBoundary.Method);
        Assert.Equal("/api/v1/me/connections/portal", PortalConnectionApiBoundary.Route);
        Assert.True(PortalConnectionApiBoundary.RequiresUthersAuthentication);
    }
}
