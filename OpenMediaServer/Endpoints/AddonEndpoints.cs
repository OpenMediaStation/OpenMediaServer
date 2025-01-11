using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

public class AddonEndpoints(ILogger<AddonEndpoints> logger, IInventoryService inventoryService, IAddonService addonService) : IAddonEndpoints
{
    private readonly ILogger<AddonEndpoints> _logger = logger;
    private readonly IInventoryService _inventoryService = inventoryService;
    private readonly IAddonService _addonService = addonService;

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api/addon");

        group.MapGet("list", ListAddons).RequireAuthorization();
        group.MapGet("", GetAddon).RequireAuthorization();
        group.MapGet("download", GetAddonContent).RequireAuthorization();
    }

    public async Task<IResult> ListAddons(Guid inventoryItemId, string category)
    {
        var item = await _inventoryService.GetItem<InventoryItem>(inventoryItemId, category);

        return Results.Ok(item?.Addons);
    }   

    public async Task<IResult> GetAddon(Guid inventoryItemId, string category, Guid addonId)
    {
        var item = await _inventoryService.GetItem<InventoryItem>(inventoryItemId, category);

        var addon = item?.Addons?.Where(i => i.Id == addonId).FirstOrDefault();

        if (addon == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(addon);
    }

    public async Task<IResult> GetAddonContent(Guid inventoryItemId, string category, Guid addonId)
    {
        var stream = await _addonService.DownloadAddon(inventoryItemId, category, addonId);

        if (stream == null)
        {
            return Results.NotFound();
        }

        return Results.Stream(stream, contentType: "text/vtt; charset=utf-8"); // TODO dynamic content type
    }
}
