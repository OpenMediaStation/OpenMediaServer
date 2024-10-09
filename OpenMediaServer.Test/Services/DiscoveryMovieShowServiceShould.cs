using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models;
using OpenMediaServer.Services;
using OpenMediaServer.Test.Mocks;
using Shouldly;

namespace OpenMediaServer.Test.Services;

public class DiscoveryMovieShowServiceShould
{
    private readonly ILogger<DiscoveryMovieShowService> _logger;
    private readonly FileSystemRepoMock _storageRepository;
    private readonly IMetadataService _metadataService;
    private readonly IFileInfoService _fileInfoService;
    private readonly IDiscoveryMovieShowService _inventoryMovieShowService;
    private readonly IInventoryService _inventoryService;

    public DiscoveryMovieShowServiceShould()
    {
        _logger = Substitute.For<ILogger<DiscoveryMovieShowService>>();
        _storageRepository = new FileSystemRepoMock();
        _metadataService = Substitute.For<IMetadataService>();
        _fileInfoService = Substitute.For<IFileInfoService>();
        _inventoryService = new InventoryService(Substitute.For<ILogger<InventoryService>>(), _storageRepository);
        _inventoryMovieShowService = new DiscoveryMovieShowService(_logger, _fileInfoService, _metadataService, _inventoryService);
    }

    [Theory]
    [InlineData("/media/Movies/Ex Machina.mkv", "Ex Machina", null)]
    [InlineData("/media/Movies/Ex Machina/Ex Machina.mkv", "Ex Machina", "/media/Movies/Ex Machina")]
    [InlineData("/media/Movies/Hunger Games.mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/Hunger Games (German).mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/Hunger Games - Directors Cut.mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/Hunger Games - Directors Cut/Hunger Games - Directors Cut.mp4", "Hunger Games - Directors Cut", null)]
    [InlineData("/media/Movies/Det arktiska Skandinavien/Det arktiska Skandinavien.mp4", "Det arktiska Skandinavien", "/media/Movies/Det arktiska Skandinavien")]
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

    [Theory]
    [InlineData("/media/Shows/Mr Robot/Season 1/Mr Robot S01E01.mp4", "Mr Robot S01E01", null)]
    [InlineData("/media/Shows/Mr Robot/Season 10/Mr Robot S01E01.mp4", "Mr Robot S01E01", null)]
    [InlineData("/media/Shows/Mr Robot/Season 1/S01E02.mp4", "S01E02", null)]
    [InlineData("/media/Shows/Mr Robot/Season 3/S01E14.mp4", "S01E14", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/S1E3.mp4", "S1E3", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Mr Robot S1E3.mp4", "Mr Robot S1E3", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Mr Robot Episode name S04E03.mp4", "Episode name", null)]
    [InlineData("/media/Shows/Mr Robot/Season 4/Episode name S04E03.mp4", "Episode name", null)]
    public async Task CreateFromPaths_FirstItemEpisode(string path, string title, string? folderPath)
    {
        // Arrange

        // Act
        await _inventoryMovieShowService.CreateShow(path);
        var resultJson = _storageRepository.WrittenObjects.First();
        var result = JsonSerializer.Deserialize<IEnumerable<Movie>>(resultJson);

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

    // [Theory]
    // [InlineData("/media/Books/practicalsocialengineering.epub", "practicalsocialengineering", null)]
    // [InlineData("/media/Books/Practical Socialengineering.epub", "Practical Socialengineering", null)]
    // [InlineData("/media/Books/Die Tribute von Panem/Die Tribute Von Panem. Gefährliche Liebe/Die Tribute Von Panem. Gefährliche Liebe.epub", "Die Tribute Von Panem. Gefährliche Liebe", "/media/Books/Die Tribute von Panem/Die Tribute Von Panem. Gefährliche Liebe")]
    // [InlineData("/media/Books/Die Tribute von Panem/Die Tribute von Panem X - Das Lied von Vogel und Schlange/Die Tribute von Panem X - Das Lied von Vogel und Schlange.epub", "Die Tribute von Panem X - Das Lied von Vogel und Schlange", "/media/Books/Die Tribute von Panem/Die Tribute von Panem X - Das Lied von Vogel und Schlange")]
    // [InlineData("/media/Books/Hjärta serien/Ishjärta/Ishjärta.epub", "Ishjärta", "/media/Books/Hjärta serien/Ishjärta")]
    // [InlineData("/media/Books/Quality Land/QualityLand 2.0 Kikis Geheimnis/QualityLand 2.0 Kikis Geheimnis.m4b", "QualityLand 2.0 Kikis Geheimnis", "/media/Books/Quality Land/QualityLand 2.0 Kikis Geheimnis")]
    // public async Task CreateFromPaths_FirstItemBook(string path, string title, string? folderPath)
    // {
    //     // Arrange
    //     var paths = new List<string>
    //     {
    //         path
    //     };

    //     // Act
    //     await _inventoryMovieShowService.CreateFromPaths(paths);
    //     var resultJson = _storageRepository.WrittenObjects.First();
    //     var result = JsonSerializer.Deserialize<IEnumerable<Movie>>(resultJson);

    //     // Assert
    //     var resultItem = result.First();
    //     resultItem.Id.ShouldNotBe(Guid.Empty);
    //     resultItem.Title.ShouldBe(title);
    //     resultItem.Category.ShouldBe("Book");
    //     resultItem.MetadataId.ShouldNotBe(Guid.Empty);
    //     resultItem.Versions.ShouldNotBeNull();
    //     resultItem.Versions.Count().ShouldBe(1);
    //     resultItem.Versions.First().Id.ShouldNotBe(Guid.Empty);
    //     resultItem.Versions.First().Path.ShouldBe(path);
    //     resultItem.FolderPath.ShouldBe(folderPath);
    // }
}