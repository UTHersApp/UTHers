namespace UTHers.Application.UniversityConnections.Portal;

public interface IConnectPortalUseCase
{
    Task<ConnectPortalResult> ExecuteAsync(
        ConnectPortalCommand command,
        CancellationToken cancellationToken);
}
