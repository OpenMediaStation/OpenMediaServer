using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Services;
using OpenMediaServer.Test.Mocks;
using Shouldly;

namespace OpenMediaServer.Test.Services;

public class DiscoveryShowServiceShould
{
    private readonly ILogger<DiscoveryShowService> _logger;
    private readonly FileSystemRepoMock _storageRepository;
    private readonly IMetadataService _metadataService;
    private readonly IFileInfoService _fileInfoService;
    private readonly IDiscoveryShowService _inventoryShowService;
    private readonly IInventoryService _inventoryService;
    private readonly IAddonService _addonService;

    public DiscoveryShowServiceShould()
    {
        Setup.Configure();

        _logger = Substitute.For<ILogger<DiscoveryShowService>>();
        _storageRepository = new FileSystemRepoMock();
        _metadataService = Substitute.For<IMetadataService>();
        _fileInfoService = Substitute.For<IFileInfoService>();
        _addonService = Substitute.For<IAddonService>();
        _inventoryService = new InventoryService(Substitute.For<ILogger<InventoryService>>(), _storageRepository);
        _inventoryShowService = new DiscoveryShowService(_logger, _fileInfoService, _metadataService, _inventoryService, _addonService);
    }

    [Theory]
    [InlineData("/media/Shows/Mr Robot/Season 1/Mr Robot S01E01.mp4", "Mr Robot S1E1", null)]
    [InlineData("/media/Shows/Mr Robot/Season 10/Mr Robot S01E01.mp4", "Mr Robot S1E1", null)]
    // [InlineData("/media/Shows/Mr Robot/Season 1/S01E02.mp4", "Mr Robot S1E2", null)]
    // [InlineData("/media/Shows/Mr Robot/Season 3/S01E14.mp4", "Mr Robot S1E14", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Mr Robot Episode name S4E3.mp4", "Mr Robot S4E3", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Episode name S04E03.mp4", "Mr Robot S4E3", null)]
    [InlineData("/media/Shows/Babylon Berlin/Season 1/Folge 1 Staffel 1 S01E01.mp4", "Babylon Berlin S1E1", null)]
    [InlineData("/media/Shows/Cyberpunk Edgerunners/Season 1/Cyberpunk Edgerunners S01E02.mp4", "Cyberpunk Edgerunners S1E2", null)]
    [InlineData("/media/Shows/The Expanse/Season 1/The EXPANSE - S01E02.mp4", "The Expanse S1E2", null)]
    [InlineData("/media/Shows/The Expanse/Season 1/The EXPANSE - S01 E02.mp4", "The Expanse S1E2", null)]
    [InlineData("/media/Shows/The 100/Season 1/S01E02.mp4", "The 100 S1E2", null)]
    [InlineData("/media/Shows/Cyberpunk Edegrunners/Season 1/Cyberpunk - Edgerunners - S01E02 - DUAL 1080p WEB H.264 -Asdfdf (AG).mkv", "Cyberpunk Edegrunners S1E2", null)]
    public async Task CreateFromPaths_FirstItemEpisode(string path, string title, string? folderPath)
    {
        // Arrange

        // Act
        await _inventoryShowService.CreateShow(path);
        var resultJson = _storageRepository.WrittenObjects.First(i => i.Contains("\"Episode\""));
        var result = JsonSerializer.Deserialize<IEnumerable<Episode>>(resultJson);

        // Assert
        var resultItem = result.First();
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Episode");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        resultItem.Versions.ShouldNotBeNull();
        resultItem.Versions.Count().ShouldBe(1);
        resultItem.Versions.First().Id.ShouldNotBe(Guid.Empty);
        resultItem.Versions.First().Path.ShouldBe(path);
        resultItem.FolderPath.ShouldBe(folderPath);
    }

    [Theory]
    [InlineData("/media/Shows/Mr Robot/Season 1/Mr Robot S01E01.mp4", "/media/Shows/Mr Robot/Season 1", 1)]
    [InlineData("/media/Shows/The Expanse/Season 5/S05E02.mp4", "/media/Shows/The Expanse/Season 5", 5)]
    [InlineData("/media/Shows/The Expanse/Season 5/S05 E02.mp4", "/media/Shows/The Expanse/Season 5", 5)]
    [InlineData("/media/Shows/The Expanse/Season 5/The Expanse S05E02.mp4", "/media/Shows/The Expanse/Season 5", 5)]
    [InlineData("/media/Shows/The Expanse/Season 5/The Expanse - S05E02.mp4", "/media/Shows/The Expanse/Season 5", 5)]
    [InlineData("/media/Shows/Cyberpunk Edegrunners/Season 1/Cyberpunk - Edgerunners - S01E02 - DUAL 1080p WEB H.264 -Asdfdf (AG).mkv", "/media/Shows/Cyberpunk Edegrunners/Season 1", 1)]
    public async Task CreateFromPaths_FirstItemSeason(string path, string? folderPath, int seasonNr)
    {
        // Arrange

        // Act
        await _inventoryShowService.CreateShow(path);
        var resultJson = _storageRepository.WrittenObjects.First(i => i.Contains("\"Season\""));
        var result = JsonSerializer.Deserialize<IEnumerable<Season>>(resultJson);

        // Assert
        var resultItem = result.First();
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.FolderPath.ShouldBe(folderPath);
        resultItem.SeasonNr.ShouldBe(seasonNr);
    }

    [Theory]
    [InlineData("/media/Shows/Mr Robot/Season 1/Mr Robot S01E01.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 10/Mr Robot S01E01.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 1/S01E02.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 3/S01E14.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 4/Mr Robot Episode name S04E03.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Mr Robot/Season 4/Episode name S04E03.mp4", "Mr Robot", "/media/Shows/Mr Robot")]
    [InlineData("/media/Shows/Babylon Berlin/Season 1/Folge 1 Staffel 1 S01E01.mp4", "Babylon Berlin", "/media/Shows/Babylon Berlin")]
    [InlineData("/media/Shows/Cyberpunk Edgerunners/Season 1/Cyberpunk Edgerunners S01E02.mp4", "Cyberpunk Edgerunners", "/media/Shows/Cyberpunk Edgerunners")]
    [InlineData("/media/Shows/Cyberpunk Edgerunners/Season 1/S01E02.mp4", "Cyberpunk Edgerunners", "/media/Shows/Cyberpunk Edgerunners")]
    [InlineData("/media/Shows/The Expanse/Season 1/The EXPANSE - S01E02.mp4", "The Expanse", "/media/Shows/The Expanse")]
    [InlineData("/media/Shows/The Expanse/Season 1/The EXPANSE - S01 E02.mp4", "The Expanse", "/media/Shows/The Expanse")]
    [InlineData("/media/Shows/The 100/Season 1/S01E02.mp4", "The 100", "/media/Shows/The 100")]
    [InlineData("/media/Shows/Cyberpunk Edegrunners/Season 1/Cyberpunk - Edgerunners - S01E02 - DUAL 1080p WEB H.264 -Asdfdf (AG).mkv", "Cyberpunk Edegrunners", "/media/Shows/Cyberpunk Edegrunners")]
    public async Task CreateFromPaths_FirstItemShow(string path, string title, string? folderPath)
    {
        // Arrange

        // Act
        await _inventoryShowService.CreateShow(path);
        var resultJson = _storageRepository.WrittenObjects.First();
        var result = JsonSerializer.Deserialize<IEnumerable<Show>>(resultJson);

        // Assert
        var resultItem = result.First();
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Show");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        resultItem.FolderPath.ShouldBe(folderPath);
    }
}