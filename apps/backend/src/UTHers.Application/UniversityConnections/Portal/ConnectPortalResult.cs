using UTHers.Application.Integrations.Portal;

namespace UTHers.Application.UniversityConnections.Portal;

public sealed class ConnectPortalResult
{
    private ConnectPortalResult(bool isConnected, PortalAuthenticationFailure? failure)
    {
        IsConnected = isConnected;
        Failure = failure;
    }

    public bool IsConnected { get; }

    public PortalAuthenticationFailure? Failure { get; }

    public static ConnectPortalResult Connected() => new(true, null);

    public static ConnectPortalResult Failed(PortalAuthenticationFailure failure)
    {
        if (failure is PortalAuthenticationFailure.CallerCancelled)
        {
            throw new ArgumentOutOfRangeException(
                nameof(failure),
                failure,
                "Caller cancellation must be propagated instead of returned as a connection failure.");
        }

        return new ConnectPortalResult(false, failure);
    }
}
