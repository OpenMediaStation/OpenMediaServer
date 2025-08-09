using System.Linq.Expressions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

public class VersionService(ILogger<VersionService> logger, IDataRepository dataRepository) : IVersionService
{
    public async Task DeleteVersion(Guid versionId)
    {
        await dataRepository.DeleteObjectWithFilter<InventoryItemVersion>(versionId);
    }
    
    public async Task<IEnumerable<InventoryItemVersion>?> ListItems(Expression<Func<InventoryItemVersion,bool>>? filter = null)
    {
        var items = await dataRepository.ListObjects<InventoryItemVersion>();

        return items;
    }
    
    public async Task UpdateOrInsert(InventoryItemVersion item)
    {
        await dataRepository.WriteObject(item);
    }
}