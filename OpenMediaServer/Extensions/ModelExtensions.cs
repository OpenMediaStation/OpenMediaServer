using System;
using OpenMediaServer.DTOs;
using OpenMediaServer.Models;

namespace OpenMediaServer.Extensions;

public static class ModelExtensions
{
    public static MovieMetadataModel ToMetadataItem(this OMDbModel model)
    {
        var metadataItem = new MovieMetadataModel()
        {
            Title = model.Title,
            Year = model.Year,
            Rated = model.Rated,
            Released = model.Released,
            Runtime = model.Runtime,
            Genre = model.Genre,
            Director = model.Director,
            Writer = model.Writer,
            Actors = model.Actors,
            Plot = model.Plot,
            Language = model.Language,
            Country = model.Country,
            Awards = model.Awards,
            Poster = model.Poster,
            Ratings = model.Ratings?.ConvertAll(rating => new Models.Rating
            {
                Source = rating.Source,
                Value = rating.Value
            }),
            Metascore = model.Metascore,
            ImdbRating = model.ImdbRating,
            ImdbVotes = model.ImdbVotes,
            ImdbID = model.ImdbID,
            Type = model.Type,
            DVD = model.DVD,
            BoxOffice = model.BoxOffice,
            Production = model.Production,
            Website = model.Website,
            Response = model.Response
        };

        return metadataItem;
    }
}
