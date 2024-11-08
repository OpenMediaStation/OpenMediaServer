using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Services;
using OpenMediaServer.Test.Mocks;
using Shouldly;

namespace OpenMediaServer.Test.Services;

public class DiscoveryMovieServiceShould
{
    private readonly ILogger<DiscoveryMovieService> _logger;
    private readonly FileSystemRepoMock _storageRepository;
    private readonly IMetadataService _metadataService;
    private readonly IFileInfoService _fileInfoService;
    private readonly IDiscoveryMovieService _inventoryMovieShowService;
    private readonly IInventoryService _inventoryService;

    public DiscoveryMovieServiceShould()
    {
        Setup.Configure();

        _logger = Substitute.For<ILogger<DiscoveryMovieService>>();
        _storageRepository = new FileSystemRepoMock();
        _metadataService = Substitute.For<IMetadataService>();
        _fileInfoService = Substitute.For<IFileInfoService>();
        _inventoryService = new InventoryService(Substitute.For<ILogger<InventoryService>>(), _storageRepository);
        _inventoryMovieShowService = new DiscoveryMovieService(_logger, _fileInfoService, _metadataService, _inventoryService);
    }

    [Theory]
    [InlineData("/media/Movies/Ex Machina.mkv", "Ex Machina", null)]
    [InlineData("/media/Movies/Millers.Girl.2024.mkv", "Millers Girl", null)]
    [InlineData("/media/Movies/Don't hex the water.mp4", "Don't hex the water", null)]
    [InlineData("/media/Movies/Ex Machina/Ex Machina.mkv", "Ex Machina", "/media/Movies/Ex Machina")]
    [InlineData("/media/Movies/Hunger Games.mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/Hunger Games (German).mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/Hunger Games - Directors Cut.mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/Det arktiska Skandinavien/Det arktiska Skandinavien.mp4", "Det arktiska Skandinavien", "/media/Movies/Det arktiska Skandinavien")]
    [InlineData("/media/Movies/Hunger Games - Directors Cut/Hunger Games - Directors Cut.mp4", "Hunger Games", "/media/Movies/Hunger Games - Directors Cut")]
    [InlineData("/media/Movies/BlueRay/New/FilmName/FilmName.mkv", "FilmName","/media/Movies/BlueRay/New/FilmName")]
    [InlineData("/media/Movies/This is - Movie Name/This is - Movie Name.mp4", "This is - Movie Name", "/media/Movies/This is - Movie Name")]
    public async Task CreateFromPaths_FirstItemMovie(string path, string title, string? folderPath)
    {
        // Arrange

        // Act
        await _inventoryMovieShowService.CreateMovie(path);
        var resultJson = _storageRepository.WrittenObjects.First();
        var result = JsonSerializer.Deserialize<IEnumerable<Movie>>(resultJson);

        // Assert
        var resultItem = result.First();
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Movie");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        resultItem.Versions.ShouldNotBeNull();
        resultItem.Versions.Count().ShouldBe(1);
        resultItem.Versions.First().Id.ShouldNotBe(Guid.Empty);
        resultItem.Versions.First().Path.ShouldBe(path);
        resultItem.FolderPath.ShouldBe(folderPath);
    }
}