using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Interfaces.Services.Metadata;
using OpenMediaServer.Models;
using OpenMediaServer.Services;
using OpenMediaServer.Services.Discovery;
using OpenMediaServer.Test.Mocks;
using Shouldly;

namespace OpenMediaServer.Test.Services;

public class DiscoveryMovieServiceShould
{
    private readonly ILogger<DiscoveryMovieService> _logger;
    private readonly DataRepoMock _storageRepository;
    private readonly IMetadataService _metadataService;
    private readonly IFileInfoService _fileInfoService;
    private readonly IDiscoveryMovieService _inventoryMovieShowService;
    private readonly IInventoryService _inventoryService;
    private readonly IAddonService _addonService;
    private readonly IBinService _binService;
    private readonly IVersionService _versionService;
    private readonly IMovieMetadataService _movieMetadataService;

    public DiscoveryMovieServiceShould()
    {
        Setup.Configure();

        _logger = Substitute.For<ILogger<DiscoveryMovieService>>();
        _storageRepository = new DataRepoMock();
        _metadataService = Substitute.For<IMetadataService>();
        _fileInfoService = Substitute.For<IFileInfoService>();
        _addonService = Substitute.For<IAddonService>();
        _binService = Substitute.For<IBinService>();
        _movieMetadataService = Substitute.For<IMovieMetadataService>();
        _versionService = Substitute.For<IVersionService>();
        _inventoryService = new InventoryService(Substitute.For<ILogger<InventoryService>>(), _storageRepository, Mock.Of<IImageService>());
        _inventoryMovieShowService = new DiscoveryMovieService(_logger, _fileInfoService, _metadataService, _inventoryService, _addonService, _binService, _versionService, _movieMetadataService);
    }

    [Theory]
    [InlineData("/media/Movies/Ex Machina.mkv", "Ex Machina", null)]
    [InlineData("/media/Movies/Millers.Girl.2024.mkv", "Millers Girl", null)]
    [InlineData("/media/Movies/Don't hex the water.mp4", "Don't hex the water", null)]
    [InlineData("/media/Movies/Ex Machina/Ex Machina.mkv", "Ex Machina", "/media/Movies/Ex Machina")]
    [InlineData("/media/Movies/Hunger Games.mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/Hunger Games (German).mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/Det arktiska Skandinavien/Det arktiska Skandinavien.mp4", "Det arktiska Skandinavien", "/media/Movies/Det arktiska Skandinavien")]
    [InlineData("/media/Movies/Hunger Games - Directors Cut/Hunger Games - Directors Cut.mp4", "Hunger Games - Directors Cut", "/media/Movies/Hunger Games - Directors Cut")]
    [InlineData("/media/Movies/BlueRay/New/FilmName/FilmName.mkv", "FilmName","/media/Movies/BlueRay/New/FilmName")]
    [InlineData("/media/Movies/Hunger Games - Directors Cut.mp4", "Hunger Games - Directors Cut", null)]
    [InlineData("/media/Movies/Hunger Games - Directors Cut (2024).mkv", "Hunger Games - Directors Cut", null)]
    [InlineData("/media/Movies/This is - Movie Name/This is - Movie Name.mp4", "This is - Movie Name", "/media/Movies/This is - Movie Name")]
    [InlineData("/media/Movies/3D/This is - Movie Name-3D-HSBS.mkv", "This is - Movie Name", null)]
    [InlineData("/media/Movies/3D/This is - Movie Name/This is - Movie Name-3D-HSBS.mkv", "This is - Movie Name", "/media/Movies/3D/This is - Movie Name", "3D-HSBS")]
    [InlineData("/media/Movies/3D/This is - Movie Name (2010)/This is - Movie Name (2010) -3D-HOU.mkv", "This is - Movie Name", "/media/Movies/3D/This is - Movie Name (2010)", "3D-HOU")]
    [InlineData("/media/Movies/Hunger Games (german) - 3d-hou.mkv", "Hunger Games", null)]
    [InlineData("/media/Movies/Crouching.Tiger.Hidden.Dragon.4K.UltraHD.HDR.BDrip-HDC.mkv", "Crouching Tiger Hidden Dragon", null)]
    [InlineData("/media/Movies/Divergent 3： Allegiant/Divergent 3： Allegiant.mp4", "Divergent 3： Allegiant", "/media/Movies/Divergent 3： Allegiant")]
    [InlineData("/media/Movies/Divergent 3： Allegiant.mp4", "Divergent 3： Allegiant", null)]
    [InlineData("/media/Movies/Türkisch für Anfänger/Türkisch für Anfänger ｜ Komödie [13012323].mp4", "Türkisch für Anfänger", "/media/Movies/Türkisch für Anfänger")]
    [InlineData("/media/Movies/test/Ex Machina.mkv", "Ex Machina", null)]
    [InlineData("/media/Movies/test/Millers.Girl.2024.mkv", "Millers Girl", null)]
    [InlineData("/media/Movies/test/Don't hex the water.mp4", "Don't hex the water", null)]
    [InlineData("/media/Movies/test/Ex Machina/Ex Machina.mkv", "Ex Machina", "/media/Movies/test/Ex Machina")]
    [InlineData("/media/Movies/test/Hunger Games.mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/test/Hunger Games (German).mp4", "Hunger Games", null)]
    [InlineData("/media/Movies/test/Det arktiska Skandinavien/Det arktiska Skandinavien.mp4", "Det arktiska Skandinavien", "/media/Movies/test/Det arktiska Skandinavien")]
    [InlineData("/media/Movies/test/Hunger Games - Directors Cut/Hunger Games - Directors Cut.mp4", "Hunger Games - Directors Cut", "/media/Movies/test/Hunger Games - Directors Cut")]
    [InlineData("/media/Movies/test/BlueRay/New/FilmName/FilmName.mkv", "FilmName","/media/Movies/test/BlueRay/New/FilmName")]
    [InlineData("/media/Movies/test/Hunger Games - Directors Cut.mp4", "Hunger Games - Directors Cut", null)]
    [InlineData("/media/Movies/test/Hunger Games - Directors Cut (2024).mkv", "Hunger Games - Directors Cut", null)]
    [InlineData("/media/Movies/test/This is - Movie Name/This is - Movie Name.mp4", "This is - Movie Name", "/media/Movies/test/This is - Movie Name")]
    [InlineData("/media/Movies/test/3D/This is - Movie Name-3D-HSBS.mkv", "This is - Movie Name", null)]
    [InlineData("/media/Movies/test/3D/This is - Movie Name/This is - Movie Name-3D-HSBS.mkv", "This is - Movie Name", "/media/Movies/test/3D/This is - Movie Name", "3D-HSBS")]
    [InlineData("/media/Movies/test/3D/This is - Movie Name (2010)/This is - Movie Name (2010) -3D-HOU.mkv", "This is - Movie Name", "/media/Movies/test/3D/This is - Movie Name (2010)", "3D-HOU")]
    [InlineData("/media/Movies/test/Hunger Games (german) - 3d-hou.mkv", "Hunger Games", null)]
    [InlineData("/media/Movies/test/Crouching.Tiger.Hidden.Dragon.4K.UltraHD.HDR.BDrip-HDC.mkv", "Crouching Tiger Hidden Dragon", null)]
    [InlineData("/media/Movies/test/Divergent 3： Allegiant/Divergent 3： Allegiant.mp4", "Divergent 3： Allegiant", "/media/Movies/test/Divergent 3： Allegiant")]
    [InlineData("/media/Movies/test/Divergent 3： Allegiant.mp4", "Divergent 3： Allegiant", null)]
    [InlineData("/media/Movies/test/Türkisch für Anfänger/Türkisch für Anfänger ｜ Komödie [13012323].mp4", "Türkisch für Anfänger", "/media/Movies/test/Türkisch für Anfänger")]
    public async Task CreateFromPaths_FirstItemMovie(string path, string title, string? folderPath, string versionName = "")
    {
        // Arrange

        // Act
        await _inventoryMovieShowService.CreateMovie(path);
        var resultJson = _storageRepository.WrittenObjects.First();
        var resultItem = JsonSerializer.Deserialize<InventoryItem>(resultJson, Globals.JsonOptions);

        // Assert
        resultItem.Id.ShouldNotBe(Guid.Empty);
        resultItem.Title.ShouldBe(title);
        resultItem.Category.ShouldBe("Movie");
        resultItem.MetadataId.ShouldNotBe(Guid.Empty);
        resultItem.Versions.ShouldNotBeNull();
        resultItem.Versions.Count().ShouldBe(1);
        resultItem.Versions.First().Id.ShouldNotBe(Guid.Empty);
        resultItem.Versions.First().Path.ShouldBe(path);
        resultItem.Versions.First().Name.ShouldBe(versionName);
        resultItem.FolderPath.ShouldBe(folderPath);
    }
}