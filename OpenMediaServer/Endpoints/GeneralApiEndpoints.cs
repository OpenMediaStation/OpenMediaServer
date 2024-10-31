using OpenMediaServer.Interfaces.Endpoints;
using OpenMediaServer.Models.Auth;

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
        group.MapGet("/auth/info", AuthInfo);
    }

    public IResult Health()
    {
        return Results.Ok("healthy");
    }

    public IResult AuthInfo()
    {
        var info = new AuthInfo()
        {
            AuthorizeUrl = Globals.AuthorizeUrl,
            DeviceCodeUrl = Globals.DeviceCodeUrl,
            TokenUrl = Globals.TokenUrl,
            ClientId = Globals.ClientId,
        };

        return Results.Ok(info);
    }
}
