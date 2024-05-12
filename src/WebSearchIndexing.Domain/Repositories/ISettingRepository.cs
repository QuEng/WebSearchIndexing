using WebSearchIndexing.Domain.Entities;

namespace WebSearchIndexing.Domain.Repositories;

public interface ISettingRepository : IRepository<Setting, Guid>
{
    Task<Setting> GetSettingAsync();
}