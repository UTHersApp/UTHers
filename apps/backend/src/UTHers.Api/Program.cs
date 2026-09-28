using System.Net.Http.Headers;
using UTHers.Application.Integrations.Portal;
using UTHers.Infrastructure.Integrations.UTH.Portal;

var builder = WebApplication.CreateBuilder(args);

var portalOptions = builder.Configuration
    .GetSection(PortalOptions.SectionName)
    .Get<PortalOptions>()
    ?? throw new InvalidOperationException($"Configuration section '{PortalOptions.SectionName}' is required.");

if (!Uri.TryCreate(portalOptions.BaseUrl, UriKind.Absolute, out var portalBaseUri) ||
    portalBaseUri.Scheme != Uri.UriSchemeHttps)
{
    throw new InvalidOperationException($"Configuration key '{PortalOptions.SectionName}:BaseUrl' must be an absolute HTTPS URL.");
}

if (portalOptions.TimeoutSeconds is < 1 or > 300)
{
    throw new InvalidOperationException($"Configuration key '{PortalOptions.SectionName}:TimeoutSeconds' must be between 1 and 300.");
}

var normalizedPortalBaseUri = new Uri($"{portalBaseUri.AbsoluteUri.TrimEnd('/')}/");

builder.Services
    .AddHttpClient<IPortalClient, PortalClient>(httpClient =>
    {
        httpClient.BaseAddress = normalizedPortalBaseUri;
        httpClient.Timeout = TimeSpan.FromSeconds(portalOptions.TimeoutSeconds);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    // VI: CAPTCHA nằm trong query string; tắt HttpClient logging để tránh rò rỉ token.
    // EN: CAPTCHA is in the query string; disable HttpClient logging to prevent token exposure.
    .RemoveAllLoggers();

var app = builder.Build();

app.Run();
