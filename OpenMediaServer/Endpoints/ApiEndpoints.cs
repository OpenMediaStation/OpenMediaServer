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
        group.MapGet("/movie", GetMovie); 
        group.MapGet("/show", GetShow); 
        group.MapGet("/episode", GetEpisode); 
        group.MapGet("/season", GetSeason); 
        group.MapGet("/item", GetItem);
    }

    public IResult ListCategories()
    {
        var categories = _inventoryService.ListCategories();

        return Results.Ok(categories);
    }

    public async Task<IResult> ListItems(string category)
    {
        var items = await _inventoryService.ListItems<InventoryItem>(category);

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
        var item = await _inventoryService.GetItem<InventoryItem>(id: id, category: category);

        if ( item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in category");
        }
    }

    public async Task<IResult> GetMovie(Guid id)
    {            
        var item = await _inventoryService.GetItem<Movie>(id: id, category: "Movie");

        if ( item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in movies");
        }
    }

    public async Task<IResult> GetShow(Guid id)
    {            
        var item = await _inventoryService.GetItem<Show>(id: id, category: "Show");

        if ( item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in shows");
        }
    }

    public async Task<IResult> GetEpisode(Guid id)
    {            
        var item = await _inventoryService.GetItem<Episode>(id: id, category: "Episode");

        if ( item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in episodes");
        }
    }

    public async Task<IResult> GetSeason(Guid id)
    {            
        var item = await _inventoryService.GetItem<Season>(id: id, category: "Season");

        if ( item != null)
        {
            return Results.Ok(item);
        }
        else
        {
            return Results.NotFound("Id not found in seasons");
        }
    }
}
