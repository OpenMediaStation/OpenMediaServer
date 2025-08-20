using System.Linq.Expressions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models.Metadata;

namespace OpenMediaServer.Services.Metadata;

public class MetadataService(
    ILogger<MetadataService> logger,
    IDataRepository dataRepository,
    IMovieMetadataService movieMetadataService,
    IShowMetadataService showMetadataService,
    ISeasonMetadataService seasonMetadataService,
    IEpisodeMetadataService episodeMetadataService,
    IBookMetadataService bookMetadataService,
    IAudioBookMetadataService audioBookMetadataService,
    IImageService imageService,
    IChapterService chapterService)
    : IMetadataService
{
    public async Task<MetadataModel?> CreateNewMetadata(string category, Guid parentId, string title,
        string? year = null, int? season = null, int? episode = null, string? language = null, string? path = null)
    {
        // var metadatas = await ListMetadata(category);

        MetadataModel? metadata;
        var metadataId = Guid.NewGuid();

        language ??= Globals.PreferredLanguage;

        switch (category)
        {
            case "Movie":
            {
                metadata = await movieMetadataService.GetMetadata(year, title, language, metadataId);

                break;
            }
            case "Show":
            {
                metadata = await showMetadataService.GetShowMetadata(year, title, language, metadataId);

                break;
            }

            case "Season":
            {
                metadata = await seasonMetadataService.GetSeasonMetadata(year, title, language, metadataId, season);

                break;
            }

            case "Episode":
            {
                metadata = await episodeMetadataService.GetEpisodeMetadata(year, title, language, metadataId, season,
                    episode);

                break;
            }

            case "Book":
            {
                metadata = await bookMetadataService.GetMetadata(year, title, language, metadataId);

                break;
            }

            case "Audiobook":
            {
                metadata = await audioBookMetadataService.GenerateMetadata(year, title, language, metadataId, path);

                break;
            }

            default:
            {
                logger.LogWarning("Cannot create metadata for type {Type}", category);

                return null;
            }
        }

        metadata.Id = metadataId;
        metadata.Category = category;

        // metadatas = metadatas.Append(metadata);

        await dataRepository.WriteObject(metadata);

        if (category == "Audiobook")
        {
            await chapterService.ExtractChapters(path, metadataId);
        }

        return metadata;
    }

    public async Task<IEnumerable<MetadataModel>> ListMetadata(string category)
    {
        var metadataObjects = await dataRepository.ListObjects<MetadataModel>(m => m.Category == category);

        return metadataObjects;
    }

    public async Task<MetadataModel?> GetMetadata(string category, Guid id)
    {
        var metadata = await dataRepository.GetObjectById<MetadataModel>(id);
        _ = Task.Run(() => CreateBlurHashIfMissing(metadata));
        return metadata;
    }

    private async Task CreateBlurHashIfMissing(MetadataModel? metadata)
    {
        if (metadata?.Category == null)
            return;

        var props = typeof(MetadataModel).GetProperties();
        var matchingProp = props.FirstOrDefault(p =>
            p.Name.Equals(metadata.Category, StringComparison.InvariantCultureIgnoreCase));
        if (matchingProp == null)
            return;

        var subObj = matchingProp.GetValue(metadata);

        var subObjProps = matchingProp.PropertyType.GetProperties();

        var blurHashProps =
            subObjProps.Where(p => p.Name.EndsWith("BlurHash", StringComparison.InvariantCultureIgnoreCase));
        foreach (var blurHashProp in blurHashProps)
        {
            if (blurHashProp.GetValue(subObj) != null)
                continue;

            var imgProp = subObjProps.FirstOrDefault(p =>
                p.Name.Equals(blurHashProp.Name.Replace("BlurHash", ""), StringComparison.InvariantCultureIgnoreCase));
            if (imgProp == null)
                continue;

            try
            {
                var path = imageService.GetPath(metadata.Category, metadata.Id, imgProp.Name.ToLower(), null, null);
                using var stream = imageService.GetImageStream(path);
                if (stream == null)
                    continue;
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                blurHashProp.SetValue(subObj, imageService.CreateBlurHash(ms.ToArray()));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        _ = await UpdateOrAddMetadata(metadata);
    }

    public async Task<bool> UpdateOrAddMetadata(MetadataModel metadataModel)
    {
        if (metadataModel.Category == null)
        {
            return false;
        }

        await dataRepository.WriteObject(metadataModel);

        return true;
    }
}