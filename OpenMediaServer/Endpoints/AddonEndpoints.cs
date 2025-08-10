using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

public class AddonEndpoints(ILogger<AddonEndpoints> logger, IInventoryService inventoryService, IAddonService addonService) : IAddonEndpoints
{
    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/addon");

        group.MapGet("list", ListAddons).RequireAuthorization();
        group.MapGet("", GetAddon).RequireAuthorization();
        group.MapGet("download", GetAddonContent).RequireAuthorization();
    }

    public async Task<IResult> ListAddons(Guid inventoryItemId, string category)
    {
        var addons = await addonService.ListItems(i => i.InventoryItemId == inventoryItemId);
        
        return Results.Ok(addons);
    }   

    public async Task<IResult> GetAddon(Guid inventoryItemId, string category, Guid addonId)
    {
        var addons = await addonService.ListItems(i => i.Id == addonId);
        
        var addon = addons?.FirstOrDefault();

        if (addon == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(addon);
    }

    public async Task<IResult> GetAddonContent(Guid inventoryItemId, string category, Guid addonId)
    {
        var stream = await addonService.DownloadAddon(inventoryItemId, category, addonId);

        if (stream == null)
        {
            return Results.NotFound();
        }

        return Results.Stream(stream, contentType: "text/vtt; charset=utf-8"); // TODO dynamic content type
    }
}
