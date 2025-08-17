using OpenMediaServer.DTOs.Endpoints;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Extensions.Mapping;

public static class InventoryItemAddonExtensions
{
    public static AddonDto ToDto(this InventoryItemAddon addon)
    {
        var result = new AddonDto()
        {
            Id = addon.Id,
            Path = addon.Path,
            Category = addon.Category,
            Subtitle = new AddonSubtitleDto
            {
                Language = addon.SubtitleLanguage,
            }
        };
        
        return result;
    }
}