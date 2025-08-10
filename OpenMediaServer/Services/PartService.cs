using System.Linq.Expressions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

public class PartService(ILogger<PartService> logger, IDataRepository dataRepository) : IPartService
{
    public async Task DeletePart(Guid partId)
    {
        await dataRepository.DeleteObjectWithFilter<InventoryItemPart>(partId);
    }
    
    public async Task<IEnumerable<InventoryItemPart>?> ListItems(Expression<Func<InventoryItemPart,bool>>? filter = null)
    {
        var items = await dataRepository.ListObjects<InventoryItemPart>();

        return items;
    }
    
    public async Task UpdateOrInsert(InventoryItemPart item)
    {
        await dataRepository.WriteObject(item);
    }
}