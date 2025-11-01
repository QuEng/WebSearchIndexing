using WebSearchIndexing.Modules.Catalog.Domain;

namespace WebSearchIndexing.Modules.Catalog.Application.DTOs;

public sealed record SiteDto(Guid Id, string Host, string? DisplayName, DateTime CreatedAt)
{
    public static SiteDto FromDomain(Site site)
    {
        ArgumentNullException.ThrowIfNull(site);
        return new SiteDto(site.Id, site.Host, site.DisplayName, site.CreatedAt);
    }
}
