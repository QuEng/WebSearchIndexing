namespace WebSearchIndexing.Data;

public class IndexingDbContext(DbContextOptions<IndexingDbContext> options) : DbContext(options)
{
    public DbSet<ServiceAccount> ServiceAccounts { get; set; }
    public DbSet<UrlRequest> UrlRequests { get; set; }
    public DbSet<Setting> Settings { get; set; }
}