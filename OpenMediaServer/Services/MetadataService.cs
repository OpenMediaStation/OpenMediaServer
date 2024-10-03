using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services;

public class MetadataService : IMetadataService
{
    private readonly ILogger<MetadataService> _logger;
    private readonly IOmdbAPI _omdbAPI;
    private readonly IConfiguration _configuration;
    private readonly IFileSystemRepository _storageRepository;

    public MetadataService(ILogger<MetadataService> logger, IOmdbAPI omdbAPI, IConfiguration configuration, IFileSystemRepository storageRepository)
    {
        _logger = logger;
        _omdbAPI = omdbAPI;
        _configuration = configuration;
        _storageRepository = storageRepository;
    }

    public async Task<MetadataModel?> CreateNewMetadata(string category, Guid parentId, string title, string? year = null)
    {
        var metadatas = await ListMetadata(category);

        MetadataModel? metadata = new();

        switch (category)
        {
            case "Movie":
                {
                    var omdbData = await _omdbAPI.GetMetadata
                     (
                         name: title,
                         apiKey: _configuration.GetValue<string>("OpenMediaServer:OMDbKey"),
                         year: year
                     );

                    metadata = new MetadataModel()
                    {
                        Title = omdbData?.Title,
                        Movie = new()
                        {
                            Year = omdbData?.Year,
                            Rated = omdbData?.Rated,
                            Released = omdbData?.Released,
                            Runtime = omdbData?.Runtime,
                            Genre = omdbData?.Genre,
                            Director = omdbData?.Director,
                            Writer = omdbData?.Writer,
                            Actors = omdbData?.Actors,
                            Plot = omdbData?.Plot,
                            Language = omdbData?.Language,
                            Country = omdbData?.Country,
                            Awards = omdbData?.Awards,
                            Poster = omdbData?.Poster,
                            Ratings = omdbData?.Ratings?.ConvertAll(rating => new Models.Rating
                            {
                                Source = rating.Source,
                                Value = rating.Value
                            }),
                            Metascore = omdbData?.Metascore,
                            ImdbRating = omdbData?.ImdbRating,
                            ImdbVotes = omdbData?.ImdbVotes,
                            ImdbID = omdbData?.ImdbID,
                            Type = omdbData?.Type,
                            DVD = omdbData?.DVD,
                            BoxOffice = omdbData?.BoxOffice,
                            Production = omdbData?.Production,
                            Website = omdbData?.Website,
                        }
                    };

                    break;
                }
            case "Show":
                {
                    var omdbData = await _omdbAPI.GetMetadata
                    (
                        name: title,
                        apiKey: _configuration.GetValue<string>("OpenMediaServer:OMDbKey"),
                        year: year
                    );

                    metadata = new MetadataModel()
                    {
                        Title = omdbData?.Title,
                        Show = new()
                        {
                            Year = omdbData?.Year,
                            Rated = omdbData?.Rated,
                            Released = omdbData?.Released,
                            Runtime = omdbData?.Runtime,
                            Genre = omdbData?.Genre,
                            Director = omdbData?.Director,
                            Writer = omdbData?.Writer,
                            Actors = omdbData?.Actors,
                            Plot = omdbData?.Plot,
                            Language = omdbData?.Language,
                            Country = omdbData?.Country,
                            Awards = omdbData?.Awards,
                            Poster = omdbData?.Poster,
                            Ratings = omdbData?.Ratings?.ConvertAll(rating => new Models.Rating
                            {
                                Source = rating.Source,
                                Value = rating.Value
                            }),
                            Metascore = omdbData?.Metascore,
                            ImdbRating = omdbData?.ImdbRating,
                            ImdbVotes = omdbData?.ImdbVotes,
                            ImdbID = omdbData?.ImdbID,
                            Type = omdbData?.Type,
                            DVD = omdbData?.DVD,
                            BoxOffice = omdbData?.BoxOffice,
                            Production = omdbData?.Production,
                            Website = omdbData?.Website,
                        }
                    };

                    break;
                }

            // case "Episode":
            //     {
            //         break;
            //     }

            default:
                {
                    _logger.LogWarning("Cannot create metadata for type {Type}", category);

                    return null;
                }
        }

        metadata.Id = Guid.NewGuid();
        metadata.Category = category;
        metadata.ParentId = parentId;

        metadatas = metadatas.Append(metadata);

        await _storageRepository.WriteObject(Path.Combine(Globals.ConfigFolder, "metadata", category) + ".json", metadatas);

        return metadata;
    }

    public async Task<IEnumerable<MetadataModel>> ListMetadata(string category)
    {
        var metadatas = await _storageRepository.ReadObject<IEnumerable<MetadataModel>>(Path.Combine(Globals.ConfigFolder, "metadata", category) + ".json");

        metadatas ??= [];

        return metadatas;
    }

    public async Task<MetadataModel?> GetMetadata(string category, Guid id)
    {
        var metadatas = await _storageRepository.ReadObject<IEnumerable<MetadataModel>>(Path.Combine(Globals.ConfigFolder, "metadata", category) + ".json");

        var metadata = metadatas?.FirstOrDefault(x => x.Id == id);

        return metadata;
    }

    public async Task<bool> UpdateOrAddMetadata(MetadataModel metadataModel)
    {
        if (metadataModel.Category == null)
        {
            return false;
        }

        var metadatas = (await ListMetadata(metadataModel.Category)).ToList();

        var existingMetadata = metadatas.FirstOrDefault(m => m.Id == metadataModel.Id);

        if (existingMetadata != null)
        {
            var index = metadatas.IndexOf(existingMetadata);
            metadatas[index] = metadataModel;
        }
        else
        {
            metadatas.Add(metadataModel);
        }

        await _storageRepository.WriteObject(Path.Combine(Globals.ConfigFolder, "metadata", metadataModel.Category) + ".json", metadatas);

        return true;
    }
}
