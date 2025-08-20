using OpenMediaServer.DTOs.Endpoints.Inventory;
using OpenMediaServer.Models;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Extensions.Mapping;

public static class InventoryItemExtensions
{
    public static InventoryItemDto ToDto(this InventoryItem inventoryItem, IEnumerable<InventoryItemVersion> versions, IEnumerable<InventoryItemAddon> addons, Dictionary<Guid, List<InventoryItemPart>>? parts, IEnumerable<InventoryItem>? episodes, IEnumerable<InventoryItem>? seasons)
    {
        if (parts?.Count == 0)
        {
            parts = null;
        }
        
        var result = new InventoryItemDto()
        {
            Id = inventoryItem.Id,
            Title = inventoryItem.Title,
            Category = inventoryItem.Category,
            MetadataId = inventoryItem.MetadataId,
            Versions = versions.Select(i => i.ToDto(parts?[i.Id])),
            Addons = addons.Select(i => i.ToDto()),
            DisplayImageBlurHash = inventoryItem.DisplayImageBlurHash,
            FolderPath = inventoryItem.FolderPath,
            ShowId = inventoryItem.ShowId,
            SeasonNr = inventoryItem.SeasonNr,
            SeasonId = inventoryItem.SeasonId,
            EpisodeNr = inventoryItem.EpisodeNr,
            EpisodeIds = episodes?.Select(i => i.Id),
            SeasonIds = seasons?.Select(i => i.Id),
        };
        
        return result;
    }

    public static InventoryItemVersionDto ToDto(this InventoryItemVersion inventoryItemVersion, List<InventoryItemPart>? parts)
    {
        var result = new InventoryItemVersionDto()
        {
            Id = inventoryItemVersion.Id,
            Path = inventoryItemVersion.Path,
            FileInfoId = inventoryItemVersion.FileInfoId,
            Name = inventoryItemVersion.Name,
            Parts = parts?.Select(i => i.ToDto())
        };
        
        return result;
    }    
    
    public static InventoryItemPartDto ToDto(this InventoryItemPart part)
    {
        var result = new InventoryItemPartDto()
        {
            Id = part.Id,
            Path = part.Path,
            FileInfoId = part.FileInfoId,
            Name = part.Name,
            PrimaryIdentifier = part.PrimaryIdentifier,
            SecondaryIdentifier = part.SecondaryIdentifier,
        };
        
        return result;
    }
}