using System.Net;
using System.Text;
using System.Text.Json;
using UTHers.Application.Integrations.Portal;
using UTHers.Infrastructure.Integrations.UTH.Portal;

namespace UTHers.IntegrationTests.Portal;

public sealed class PortalClientTests
{
    private const string SanitizedUsername = "student-account";
    private const string SanitizedPassword = "not-a-real-password";
    private const string SanitizedChallenge = "challenge +/?&=";
    private const string SanitizedToken = "header.payload.signature";

    [Fact]
    public async Task AuthenticateAsyncMapsSuccessfulLogin()
    {
        var client = CreateClient((_, _) => JsonResponse(HttpStatusCode.OK, $$"""
            {"success":true,"token":"{{SanitizedToken}}"}
            """));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        Assert.True(result.IsAuthenticated);
        Assert.Equal(SanitizedToken, result.Token);
        Assert.Null(result.Failure);
    }

    [Fact]
    public async Task AuthenticateAsyncBuildsExpectedUriAndJsonRequest()
    {
        HttpMethod? actualMethod = null;
        Uri? actualUri = null;
        string? actualMediaType = null;
        string? actualBody = null;

        var client = CreateClient(async (request, cancellationToken) =>
        {
            actualMethod = request.Method;
            actualUri = request.RequestUri;
            actualMediaType = request.Content?.Headers.ContentType?.MediaType;
            actualBody = await request.Content!.ReadAsStringAsync(cancellationToken);

            return JsonResponse(HttpStatusCode.OK, $$"""
                {"success":true,"token":"{{SanitizedToken}}"}
                """);
        });

        await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(HttpMethod.Post, actualMethod);
        Assert.Equal(
            "https://portal.test/api/v1/user/login?g-recaptcha-response=challenge%20%2B%2F%3F%26%3D",
            actualUri?.AbsoluteUri);
        Assert.Equal("application/json", actualMediaType);

        using var document = JsonDocument.Parse(actualBody!);
        var properties = document.RootElement.EnumerateObject().ToArray();
        Assert.Equal(2, properties.Length);
        Assert.Equal(SanitizedUsername, document.RootElement.GetProperty("username").GetString());
        Assert.Equal(SanitizedPassword, document.RootElement.GetProperty("password").GetString());
    }

    [Fact]
    public async Task AuthenticateAsyncMapsSuccessFalseToAuthenticationRejected()
    {
        var client = CreateClient((_, _) => JsonResponse(
            HttpStatusCode.OK,
            """{"success":false,"token":null}"""));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        AssertFailure(result, PortalAuthenticationFailure.AuthenticationRejected);
    }

    [Fact]
    public async Task AuthenticateAsyncMapsUnauthorizedToInvalidCredentials()
    {
        var client = CreateClient((_, _) => JsonResponse(
            HttpStatusCode.Unauthorized,
            """{"message":"sanitized rejection"}"""));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        AssertFailure(result, PortalAuthenticationFailure.InvalidCredentials);
    }

    [Fact]
    public async Task AuthenticateAsyncMapsBadRequestToAuthenticationRejected()
    {
        var client = CreateClient((_, _) => JsonResponse(
            HttpStatusCode.BadRequest,
            """{"message":"sanitized rejection"}"""));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        AssertFailure(result, PortalAuthenticationFailure.AuthenticationRejected);
    }

    [Fact]
    public async Task AuthenticateAsyncMapsServerErrorToUpstreamUnavailable()
    {
        var client = CreateClient((_, _) => JsonResponse(
            HttpStatusCode.ServiceUnavailable,
            """{"message":"sanitized upstream error"}"""));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        AssertFailure(result, PortalAuthenticationFailure.UpstreamUnavailable);
    }

    [Fact]
    public async Task AuthenticateAsyncMapsTransportFailureToUpstreamUnavailable()
    {
        var client = CreateClient((_, _) => Task.FromException<HttpResponseMessage>(
            new HttpRequestException("Sanitized transport failure.")));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        AssertFailure(result, PortalAuthenticationFailure.UpstreamUnavailable);
    }

    [Fact]
    public async Task AuthenticateAsyncMapsUnexpectedFailureWithoutLeakingException()
    {
        var client = CreateClient((_, _) => Task.FromException<HttpResponseMessage>(
            new InvalidOperationException("Sanitized unexpected failure.")));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        AssertFailure(result, PortalAuthenticationFailure.UnexpectedFailure);
    }

    [Fact]
    public async Task AuthenticateAsyncMapsHttpClientTimeoutToUpstreamTimeout()
    {
        var client = CreateClient(
            async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                throw new InvalidOperationException("Unreachable.");
            },
            TimeSpan.FromMilliseconds(20));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        AssertFailure(result, PortalAuthenticationFailure.UpstreamTimeout);
    }

    [Fact]
    public async Task AuthenticateAsyncMapsCallerCancellationSeparately()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var client = CreateClient(async (_, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable.");
        });

        var result = await client.AuthenticateAsync(CreateRequest(), cancellation.Token);

        AssertFailure(result, PortalAuthenticationFailure.CallerCancelled);
    }

    [Fact]
    public async Task AuthenticateAsyncMapsMalformedJsonToInvalidResponse()
    {
        var client = CreateClient((_, _) => JsonResponse(HttpStatusCode.OK, "{not-json"));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        AssertFailure(result, PortalAuthenticationFailure.InvalidResponse);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"success\":true}")]
    [InlineData("{\"success\":true,\"token\":\"\"}")]
    public async Task AuthenticateAsyncMapsMissingAuthenticationResultToInvalidResponse(string responseBody)
    {
        var client = CreateClient((_, _) => JsonResponse(HttpStatusCode.OK, responseBody));

        var result = await client.AuthenticateAsync(CreateRequest(), CancellationToken.None);

        AssertFailure(result, PortalAuthenticationFailure.InvalidResponse);
    }

    [Fact]
    public void ApplicationPortalContractsDoNotExposeInfrastructureTypes()
    {
        var applicationAssembly = typeof(IPortalClient).Assembly;
        var infrastructureAssemblyName = typeof(PortalClient).Assembly.GetName().Name;

        var exposedTypes = applicationAssembly
            .GetExportedTypes()
            .Where(type => type.Namespace?.StartsWith("UTHers.Application.Integrations.Portal", StringComparison.Ordinal) == true)
            .SelectMany(GetContractTypes)
            .ToArray();

        Assert.DoesNotContain(
            exposedTypes,
            type => string.Equals(type.Assembly.GetName().Name, infrastructureAssemblyName, StringComparison.Ordinal));
    }

    [Fact]
    public void ProviderDtosAreInternalToInfrastructure()
    {
        var providerDtos = typeof(PortalClient).Assembly
            .GetTypes()
            .Where(type => type.Namespace == "UTHers.Infrastructure.Integrations.UTH.Portal.Models")
            .ToArray();

        Assert.NotEmpty(providerDtos);
        Assert.All(providerDtos, type => Assert.False(type.IsPublic));
    }

    [Fact]
    public void AuthenticationModelsRedactSensitiveValuesFromStringRepresentation()
    {
        var requestText = CreateRequest().ToString();
        var resultText = PortalAuthenticationResult.Authenticated(SanitizedToken).ToString();

        Assert.DoesNotContain(SanitizedUsername, requestText, StringComparison.Ordinal);
        Assert.DoesNotContain(SanitizedPassword, requestText, StringComparison.Ordinal);
        Assert.DoesNotContain(SanitizedChallenge, requestText, StringComparison.Ordinal);
        Assert.DoesNotContain(SanitizedToken, resultText, StringComparison.Ordinal);
    }

    private static IEnumerable<Type> GetContractTypes(Type type)
    {
        foreach (var referencedType in ExpandType(type))
        {
            yield return referencedType;
        }

        foreach (var property in type.GetProperties())
        {
            foreach (var referencedType in ExpandType(property.PropertyType))
            {
                yield return referencedType;
            }
        }

        foreach (var method in type.GetMethods())
        {
            foreach (var referencedType in ExpandType(method.ReturnType))
            {
                yield return referencedType;
            }

            foreach (var parameter in method.GetParameters())
            {
                foreach (var referencedType in ExpandType(parameter.ParameterType))
                {
                    yield return referencedType;
                }
            }
        }
    }

    private static IEnumerable<Type> ExpandType(Type type)
    {
        yield return type;

        if (type.HasElementType && type.GetElementType() is { } elementType)
        {
            foreach (var referencedType in ExpandType(elementType))
            {
                yield return referencedType;
            }
        }

        foreach (var genericArgument in type.GetGenericArguments())
        {
            foreach (var referencedType in ExpandType(genericArgument))
            {
                yield return referencedType;
            }
        }
    }

    private static PortalAuthenticationRequest CreateRequest() =>
        new(SanitizedUsername, SanitizedPassword, SanitizedChallenge);

    private static PortalClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory,
        TimeSpan? timeout = null)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(responseFactory))
        {
            BaseAddress = new Uri("https://portal.test/"),
            Timeout = timeout ?? TimeSpan.FromSeconds(5)
        };

        return new PortalClient(httpClient);
    }

    private static PortalClient CreateClient(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responseFactory,
        TimeSpan? timeout = null) =>
        CreateClient((request, cancellationToken) => Task.FromResult(responseFactory(request, cancellationToken)), timeout);

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string content) =>
        new(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

    private static void AssertFailure(
        PortalAuthenticationResult result,
        PortalAuthenticationFailure expectedFailure)
    {
        Assert.False(result.IsAuthenticated);
        Assert.Null(result.Token);
        Assert.Equal(expectedFailure, result.Failure);
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            responseFactory(request, cancellationToken);
    }
}
