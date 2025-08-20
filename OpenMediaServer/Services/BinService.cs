using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class BinService(ILogger<BinService> logger, IDataRepository dataRepository) : IBinService
{
    public async Task<T?> GetItem<T>(string title, string category) where T : InventoryItem
    {
        var possibleItems = (await dataRepository.ListObjects<T>(n => n.Category == category && n.Title == title && n.IsOrphan == true)).ToArray();
        
        if (possibleItems.Count() != 1)
        {
            logger.LogDebug("PossibleItems count in GetItem: {ItemCount}", possibleItems?.Length);
            logger.LogWarning("Id could not be found in category");

            return null;
        }

        return possibleItems.SingleOrDefault();
    }

    public async Task AddItem<T>(T item) where T : InventoryItem
    {
        item.IsOrphan = true;
        await dataRepository.WriteObject(item);
    }

    public async Task RemoveById<T>(T item) where T : InventoryItem
    {
            item.IsOrphan = false;
            await dataRepository.WriteObject(item);
    }
}
