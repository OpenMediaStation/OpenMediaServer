using System.Linq.Expressions;
using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Services;

public class InventoryService(
    ILogger<InventoryService> logger,
    IDataRepository dataRepository,
    IImageService imageService)
    : IInventoryService
{
    
    public IEnumerable<string> ListCategories()
    {
        return ["Audiobook", "Book", "Episode", "Movie", "Season", "Show"];
    }

    public async Task<IEnumerable<InventoryItem>?> ListItems(string category)
    {
        var items = await dataRepository.ListObjects<InventoryItem>(n => n.Category == category && n.IsOrphan == false);
        return items.Select(i =>
        {
            _ = Task.Run(() => CreateMissingBlurHash(i));
            return i;
        });
    }

    public async Task<InventoryItem?> GetItem(Guid id)
    {
        var possibleItem = await dataRepository.GetObjectById<InventoryItem>(id);
        
        if (possibleItem == null)
        {
            logger.LogWarning("Id could not be found");
            return null;
        }
        return possibleItem;
    }

    public async Task<InventoryItem?> GetItem(string category, Expression<Func<InventoryItem, bool>> predicate)
    {
        logger.LogTrace("Getting item by name");
        
        
        var items = (await dataRepository.ListObjects<InventoryItem>(predicate.AndAlso(n => n.Category == category && n.IsOrphan == false))).ToArray();
        
        if (items.Length != 1)
        {
            logger.LogDebug("PossibleItems count in GetItem: {ItemCount}", items?.Length);
            logger.LogWarning("Id could not be found in category");

            return null;
        }

        return items.FirstOrDefault();
    }
    
    public async Task AddItems(IEnumerable<InventoryItem> items)
    {
        foreach (var item in items)
        {
            await AddItem(item);
        }
    }

    public async Task AddItem(InventoryItem item)
    {
        await dataRepository.WriteObject(item);
    }

    public async Task UpdateOrInsert(InventoryItem item)
    {
        await dataRepository.WriteObject(item);
    }
    
    public async Task Remove(InventoryItem item)
    {
        await dataRepository.DeleteObjectWithFilter<InventoryItem>(item.Id);
    }

    private async Task CreateMissingBlurHash(InventoryItem inventoryItem)
    {
        if (inventoryItem.DisplayImageBlurHash != null || inventoryItem.MetadataId == null)
            return;

        try
        {
            var displayImageType = string.Empty;
            switch (inventoryItem.Category)
            {
                case "Movie":
                    displayImageType = "poster";
                    break;
                case "Show":
                    displayImageType = "poster";
                    break;
                case "Episode":
                    displayImageType = "backdrop";
                    break;
                case "Season":
                    displayImageType = "poster";
                    break;
                case "Audiobook":
                    displayImageType = "cover";
                    break;
                case "Book":
                    displayImageType = "cover";
                    break;
                default:
                    return;
            }
            
            var path = imageService.GetPath(inventoryItem.Category, (Guid)inventoryItem.MetadataId, displayImageType, null, null);
            using (var stream = imageService.GetImageStream(path))
            {
                if (stream == null)
                    return;
                using (var memStr = new MemoryStream())
                {
                    stream.CopyTo(memStr);
                    var bytes = memStr.ToArray();

                    var blurHash = imageService.CreateBlurHash(bytes);
                    inventoryItem.DisplayImageBlurHash = blurHash;
                    
                    await UpdateOrInsert(inventoryItem);
                }
            }
            return ;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
