using System;
using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

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

        if (items != null && items.Any())
        {
            return Results.Ok(items);
        }
        else
        {
            return Results.NotFound("Category not found");
        }
    }

    public async Task<IResult> GetItem(string category, Guid id)
    {            
        var item = await _inventoryService.GetItem(id: id, category: category);

        if ( item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in category");
        }
    }
}
