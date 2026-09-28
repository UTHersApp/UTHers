using UTHers.Api.Errors;
using UTHers.Application.Integrations.Portal;

namespace UTHers.ApiTests.Errors;

public sealed class PortalProblemDetailsMapperTests
{
    private const string TraceId = "sanitized-trace-id";

    [Theory]
    [InlineData(
        PortalAuthenticationFailure.InvalidCredentials,
        422,
        ApiProblemCodes.PortalInvalidCredentials,
        ApiProblemTypes.PortalInvalidCredentials)]
    [InlineData(
        PortalAuthenticationFailure.AuthenticationRejected,
        422,
        ApiProblemCodes.PortalAuthenticationRejected,
        ApiProblemTypes.PortalAuthenticationRejected)]
    [InlineData(
        PortalAuthenticationFailure.UpstreamUnavailable,
        503,
        ApiProblemCodes.PortalUnavailable,
        ApiProblemTypes.PortalUnavailable)]
    [InlineData(
        PortalAuthenticationFailure.UpstreamTimeout,
        504,
        ApiProblemCodes.PortalTimeout,
        ApiProblemTypes.PortalTimeout)]
    [InlineData(
        PortalAuthenticationFailure.InvalidResponse,
        502,
        ApiProblemCodes.PortalInvalidResponse,
        ApiProblemTypes.PortalInvalidResponse)]
    [InlineData(
        PortalAuthenticationFailure.UnexpectedFailure,
        502,
        ApiProblemCodes.PortalUpstreamFailure,
        ApiProblemTypes.PortalUpstreamFailure)]
    public void PortalFailuresMapToApprovedProblems(
        PortalAuthenticationFailure failure,
        int expectedStatus,
        string expectedCode,
        string expectedType)
    {
        var problem = PortalProblemDetailsMapper.Create(failure, TraceId);

        Assert.NotNull(problem);
        Assert.Equal(expectedStatus, problem.Status);
        Assert.Equal(expectedCode, problem.Extensions["code"]);
        Assert.Equal(expectedType, problem.Type);
        Assert.Equal(TraceId, problem.Extensions["traceId"]);
        Assert.Null(problem.Detail);
    }

    [Fact]
    public void CallerCancellationDoesNotCreateAProblemResponse()
    {
        var problem = PortalProblemDetailsMapper.Create(
            PortalAuthenticationFailure.CallerCancelled,
            TraceId);

        Assert.Null(problem);
    }

    [Fact]
    public void PortalProblemCodesAndTypesAreStableAndUnique()
    {
        var codes = new[]
        {
            ApiProblemCodes.PortalInvalidCredentials,
            ApiProblemCodes.PortalAuthenticationRejected,
            ApiProblemCodes.PortalUnavailable,
            ApiProblemCodes.PortalTimeout,
            ApiProblemCodes.PortalInvalidResponse,
            ApiProblemCodes.PortalUpstreamFailure
        };
        var types = new[]
        {
            ApiProblemTypes.PortalInvalidCredentials,
            ApiProblemTypes.PortalAuthenticationRejected,
            ApiProblemTypes.PortalUnavailable,
            ApiProblemTypes.PortalTimeout,
            ApiProblemTypes.PortalInvalidResponse,
            ApiProblemTypes.PortalUpstreamFailure
        };

        Assert.Equal(
            new[]
            {
                "PORTAL_INVALID_CREDENTIALS",
                "PORTAL_AUTHENTICATION_REJECTED",
                "PORTAL_UNAVAILABLE",
                "PORTAL_TIMEOUT",
                "PORTAL_INVALID_RESPONSE",
                "PORTAL_UPSTREAM_FAILURE"
            },
            codes);
        Assert.Equal(
            new[]
            {
                "https://api.uthers.app/problems/portal-invalid-credentials",
                "https://api.uthers.app/problems/portal-authentication-rejected",
                "https://api.uthers.app/problems/portal-unavailable",
                "https://api.uthers.app/problems/portal-timeout",
                "https://api.uthers.app/problems/portal-invalid-response",
                "https://api.uthers.app/problems/portal-upstream-failure"
            },
            types);
        Assert.Equal(codes.Length, codes.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(types.Length, types.Distinct(StringComparer.Ordinal).Count());
    }
}
