using OpenMediaServer.Interfaces.APIs;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services;

public class MetadataService : IMetadataService
{
    private readonly ILogger<MetadataService> _logger;
    private readonly IFileSystemRepository _storageRepository;
    private readonly IMovieMetadataService _movieMetadataService;
    private readonly IShowMetadataService _showMetadataService;
    private readonly IBookMetadataService _bookMetadataService;
    private readonly IAudioBookMetadataService _audioBookMetadataService;

    public MetadataService(ILogger<MetadataService> logger, IFileSystemRepository storageRepository, IMovieMetadataService movieMetadataService, IShowMetadataService showMetadataService, IBookMetadataService bookMetadataService, IAudioBookMetadataService audioBookMetadataService)
    {
        _logger = logger;
        _storageRepository = storageRepository;
        _movieMetadataService = movieMetadataService;
        _showMetadataService = showMetadataService;
        _bookMetadataService = bookMetadataService;
        _audioBookMetadataService = audioBookMetadataService;
    }

    public async Task<MetadataModel?> CreateNewMetadata(string category, Guid parentId, string title, string? year = null, int? season = null, int? episode = null, string? language = null)
    {
        var metadatas = await ListMetadata(category);

        MetadataModel? metadata = new();
        var metadataId = Guid.NewGuid();

        language ??= Globals.PreferredLanguage;

        switch (category)
        {
            case "Movie":
                {
                    metadata = await _movieMetadataService.GetMetadata(year, title, language, metadataId);

                    break;
                }
            case "Show":
                {
                    metadata = await _showMetadataService.GetShowMetadata(year, title, language, metadataId);

                    break;
                }

            case "Season":
                {
                    metadata = await _showMetadataService.GetSeasonMetadata(year, title, language, metadataId, season);

                    break;
                }

            case "Episode":
                {
                    metadata = await _showMetadataService.GetEpisodeMetadata(year, title, language, metadataId, season, episode);

                    break;
                }

            case "Book":
                {
                    metadata = await _bookMetadataService.GetMetadata(year, title, language, metadataId);

                    break;
                }

            case "Audiobook":
                {
                    metadata = await _audioBookMetadataService.GetMetadata(year, title, language, metadataId);

                    break;
                }

            default:
                {
                    _logger.LogWarning("Cannot create metadata for type {Type}", category);

                    return null;
                }
        }
        metadata.Id = metadataId;
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
