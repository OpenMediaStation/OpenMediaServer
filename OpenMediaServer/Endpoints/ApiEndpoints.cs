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
        try
        {
            var items = await _inventoryService.ListItems(category);

            return Results.Ok(items);
        }
        catch (FileNotFoundException fileEx)
        {
            _logger.LogWarning(fileEx, "Category could not be found");

            return Results.NotFound("Category not found");
        }
    }

    public async Task<IResult> GetItem(string category, Guid id)
    {
        try
        {
            var item = await _inventoryService.GetItem(id: id, category: category);

            return Results.Ok(item);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "Id could not be found in category");

            return Results.NotFound("Id not found in category");
        }
        catch (FileNotFoundException fileEx)
        {
            _logger.LogWarning(fileEx, "Category could not be found to retrieve id");

            return Results.NotFound("Category not found");
        }
    }
}
