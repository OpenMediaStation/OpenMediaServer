using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;

namespace OpenMediaServer.Endpoints;

/// <summary>
/// Endpoints that do not fit in another endpoint service
/// </summary>
public class GeneralApiEndpoints(ILogger<GeneralApiEndpoints> logger) : IGeneralApiEndpoints
{
    private readonly ILogger<GeneralApiEndpoints> _logger = logger;

    public void Map(WebApplication app)
    {
        var group = app.MapGroup("/");

        group.MapGet("/health", Health);
    }

    public IResult Health()
    {
        return Results.Ok("healthy");
    }
}
