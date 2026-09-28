namespace UTHers.Application.Integrations.Portal;

public interface IPortalClient
{
    Task<PortalAuthenticationResult> AuthenticateAsync(
        PortalAuthenticationRequest request,
        CancellationToken cancellationToken);
}
