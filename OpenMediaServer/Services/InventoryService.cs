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
    private readonly IDataRepository _dataRepository = dataRepository;

    #region Get Functions
    
    public IEnumerable<string> ListCategories()
    {
        var categories = dataRepository.ListObjects<InventoryItem,string>(select: n => n.Category);
        return categories;
    }

    public async Task<IEnumerable<T>?> ListItems<T>(string category) where T : InventoryItem
    {
        var items = await dataRepository.ListObjectsAsync<T>(n => n.Category == category);
        return items.Select(i =>
        {
            _ = Task.Run(() => CreateMissingBlurHash<T>(i));
            return i;
        });
    }

    public async Task<T?> GetItem<T>(Guid id) where T : InventoryItem
    {
        var possibleItem = await dataRepository.GetObjectByIdAsync<T>(id);
        
        if (possibleItem == null)
        {
            logger.LogWarning("Id could not be found");
            return null;
        }
        return possibleItem;
    }

    public async Task<T?> GetItem<T>(string category, Func<T, bool> predicate) where T : InventoryItem
    {
        logger.LogTrace("Getting item by name");

        var items = (await dataRepository.ListObjectsAsync<T>(n => predicate(n) && n.Category == category)).ToArray();
        
        if (items.Length != 1)
        {
            logger.LogDebug("PossibleItems count in GetItem: {ItemCount}", items?.Length);
            logger.LogWarning("Id could not be found in category");

            return null;
        }

        return items.FirstOrDefault();
    }
    #endregion
    
    #region Set Functions
    
    public async Task AddItems(IEnumerable<InventoryItem> items)
    {
        foreach (var item in items)
        {
            await AddItem(item);
        }
    }

    public async Task AddItem<T>(T item) where T : InventoryItem
    {
        await dataRepository.WriteObjectAsync(item);
    }

    public async Task Update<T>(T item) where T : InventoryItem
    {
        var existingItem = await dataRepository.GetObjectByIdAsync<T>(item.Id);
        
        await dataRepository.WriteObjectAsync(item);
    }
    #endregion
    
    public async Task Remove<T>(T item) where T : InventoryItem
    {
        await dataRepository.DeleteObjectAsync<T>(item.Id);
    }

    private async Task CreateMissingBlurHash<T>(InventoryItem inventoryItem) where T : InventoryItem
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
                    
                    await Update((T)inventoryItem);
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
