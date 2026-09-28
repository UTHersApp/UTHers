using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using UTHers.Application.Integrations.Portal;
using UTHers.Infrastructure.Integrations.UTH.Portal.Models;

namespace UTHers.Infrastructure.Integrations.UTH.Portal;

public sealed class PortalClient(HttpClient httpClient) : IPortalClient
{
    private const string LoginPath = "api/v1/user/login";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<PortalAuthenticationResult> AuthenticateAsync(
        PortalAuthenticationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var loginUri = $"{LoginPath}?g-recaptcha-response={Uri.EscapeDataString(request.ChallengeToken)}";
        var providerRequest = new PortalLoginRequest(request.Username, request.Password);

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                loginUri,
                providerRequest,
                SerializerOptions,
                cancellationToken);

            var failure = MapStatusCode(response.StatusCode);
            if (failure is not null)
            {
                return PortalAuthenticationResult.Failed(failure.Value);
            }

            PortalLoginResponse? providerResponse;
            try
            {
                await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                providerResponse = await JsonSerializer.DeserializeAsync<PortalLoginResponse>(
                    responseStream,
                    SerializerOptions,
                    cancellationToken);
            }
            catch (JsonException)
            {
                return PortalAuthenticationResult.Failed(PortalAuthenticationFailure.InvalidResponse);
            }

            return MapResponse(providerResponse);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return PortalAuthenticationResult.Failed(PortalAuthenticationFailure.CallerCancelled);
        }
        catch (OperationCanceledException)
        {
            return PortalAuthenticationResult.Failed(PortalAuthenticationFailure.UpstreamTimeout);
        }
        catch (HttpRequestException)
        {
            return PortalAuthenticationResult.Failed(PortalAuthenticationFailure.UpstreamUnavailable);
        }
        catch (Exception)
        {
            return PortalAuthenticationResult.Failed(PortalAuthenticationFailure.UnexpectedFailure);
        }
    }

    private static PortalAuthenticationFailure? MapStatusCode(HttpStatusCode statusCode)
    {
        if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return PortalAuthenticationFailure.InvalidCredentials;
        }

        if (statusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity)
        {
            return PortalAuthenticationFailure.AuthenticationRejected;
        }

        if (statusCode == HttpStatusCode.RequestTimeout)
        {
            return PortalAuthenticationFailure.UpstreamTimeout;
        }

        if (statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500)
        {
            return PortalAuthenticationFailure.UpstreamUnavailable;
        }

        return (int)statusCode is >= 200 and <= 299
            ? null
            : PortalAuthenticationFailure.UnexpectedFailure;
    }

    private static PortalAuthenticationResult MapResponse(PortalLoginResponse? response)
    {
        if (response?.Success is false)
        {
            return PortalAuthenticationResult.Failed(PortalAuthenticationFailure.AuthenticationRejected);
        }

        if (response?.Success is not true || string.IsNullOrWhiteSpace(response.Token))
        {
            return PortalAuthenticationResult.Failed(PortalAuthenticationFailure.InvalidResponse);
        }

        return PortalAuthenticationResult.Authenticated(response.Token);
    }
}
