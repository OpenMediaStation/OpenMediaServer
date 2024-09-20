using System;
using OpenMediaServer.Interfaces.Endpoints;

namespace OpenMediaServer.Endpoints;

public class MovieEndpoints : IMovieEndpoints
{
    public void Map(WebApplication app)
    {
        app.MapGet("/video", GetData);
    }

    public IResult GetData(string name)
    {
        string path = Path.Combine(Globals.MediaFolder, "Movies", name);

        return Results.Stream(new FileStream(path, FileMode.Open), enableRangeProcessing: true, contentType: "video/webm");
    }
}
