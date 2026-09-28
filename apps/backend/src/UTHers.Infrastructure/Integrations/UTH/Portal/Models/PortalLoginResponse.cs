using System.Text.Json.Serialization;

namespace UTHers.Infrastructure.Integrations.UTH.Portal.Models;

internal sealed record PortalLoginResponse(
    [property: JsonPropertyName("success")] bool? Success,
    [property: JsonPropertyName("token")] string? Token);
