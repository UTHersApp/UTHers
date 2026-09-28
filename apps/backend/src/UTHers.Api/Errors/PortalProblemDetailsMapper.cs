using Microsoft.AspNetCore.Mvc;
using UTHers.Application.Integrations.Portal;

namespace UTHers.Api.Errors;

public static class PortalProblemDetailsMapper
{
    public static ProblemDetails? Create(
        PortalAuthenticationFailure failure,
        string traceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        var definition = failure switch
        {
            PortalAuthenticationFailure.InvalidCredentials => new ProblemDefinition(
                StatusCodes.Status422UnprocessableEntity,
                ApiProblemCodes.PortalInvalidCredentials,
                ApiProblemTypes.PortalInvalidCredentials,
                "Portal authentication failed"),
            PortalAuthenticationFailure.AuthenticationRejected => new ProblemDefinition(
                StatusCodes.Status422UnprocessableEntity,
                ApiProblemCodes.PortalAuthenticationRejected,
                ApiProblemTypes.PortalAuthenticationRejected,
                "Portal authentication failed"),
            PortalAuthenticationFailure.UpstreamUnavailable => new ProblemDefinition(
                StatusCodes.Status503ServiceUnavailable,
                ApiProblemCodes.PortalUnavailable,
                ApiProblemTypes.PortalUnavailable,
                "Portal is unavailable"),
            PortalAuthenticationFailure.UpstreamTimeout => new ProblemDefinition(
                StatusCodes.Status504GatewayTimeout,
                ApiProblemCodes.PortalTimeout,
                ApiProblemTypes.PortalTimeout,
                "Portal request timed out"),
            PortalAuthenticationFailure.InvalidResponse => new ProblemDefinition(
                StatusCodes.Status502BadGateway,
                ApiProblemCodes.PortalInvalidResponse,
                ApiProblemTypes.PortalInvalidResponse,
                "Portal returned an invalid response"),
            PortalAuthenticationFailure.UnexpectedFailure => new ProblemDefinition(
                StatusCodes.Status502BadGateway,
                ApiProblemCodes.PortalUpstreamFailure,
                ApiProblemTypes.PortalUpstreamFailure,
                "Portal request failed"),
            PortalAuthenticationFailure.CallerCancelled => null,
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, "Unknown Portal authentication failure.")
        };

        if (definition is null)
        {
            return null;
        }

        var problem = new ProblemDetails
        {
            Type = definition.Type,
            Title = definition.Title,
            Status = definition.Status
        };
        problem.Extensions["code"] = definition.Code;
        problem.Extensions["traceId"] = traceId;

        return problem;
    }

    private sealed record ProblemDefinition(int Status, string Code, string Type, string Title);
}
