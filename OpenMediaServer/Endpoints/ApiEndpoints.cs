using System;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Endpoints;

public class ApiEndpoints : IApiEndpoints
{
    private readonly ILogger<ApiEndpoints> _logger;
    private readonly IInventoryService _inventoryService;

    public ApiEndpoints(ILogger<ApiEndpoints> logger, IInventoryService inventoryService)
    {
        _logger = logger;
        _inventoryService = inventoryService;
    }

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/api");

        group.MapGet("/categories", ListCategories);
        group.MapGet("/items", ListItems);
        group.MapGet("/item", GetItem);
    }

    public async Task<IResult> ListCategories()
    {
        var categories = await _inventoryService.ListCategories();

        return Results.Ok(categories);
    }

    public async Task<IResult> ListItems(string category)
    {
        var items = await _inventoryService.ListItems(category);

        return Results.Ok(items);
    }

    public async Task<IResult> GetItem(string category, string id)
    {
        var item = await _inventoryService.GetItem(id: id, category: category);

        return Results.Ok(item);
    }
}
