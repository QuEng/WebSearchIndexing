
namespace WebSearchIndexing.Data.Repositories;

public class SettingRepository(IDbContextFactory<IndexingDbContext> factory) :
    BaseRepository<Setting, Guid>(factory),
    ISettingRepository
{
    private readonly IDbContextFactory<IndexingDbContext> _factory = factory;
    public async Task<Setting> GetSettingAsync()
    {
        using var context = _factory.CreateDbContext();
        var setting = await context.Set<Setting>().FirstOrDefaultAsync();
        if (setting is null)
        {
            setting = new Setting();
            var result = await context.Set<Setting>().AddAsync(setting);
            await context.SaveChangesAsync();

            setting = result.Entity;
        }

        return setting!;
    }
}