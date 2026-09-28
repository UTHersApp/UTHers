namespace UTHers.Infrastructure.Integrations.UTH.Portal;

public sealed class PortalOptions
{
    public const string SectionName = "UniversityIntegrations:Portal";

    public string BaseUrl { get; init; } = string.Empty;

    public int TimeoutSeconds { get; init; } = 30;
}
