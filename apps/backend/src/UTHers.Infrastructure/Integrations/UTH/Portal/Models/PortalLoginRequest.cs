using System.Text.Json.Serialization;

namespace UTHers.Infrastructure.Integrations.UTH.Portal.Models;

internal sealed record PortalLoginRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password);
