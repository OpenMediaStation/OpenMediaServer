using OpenMediaServer;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Repositories;
using OpenMediaServer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IContentDiscoveryService, ContentDiscoveryService>();
builder.Services.AddSingleton<IStorageRepository, FileSystemRepository>();
builder.Services.AddSingleton<IInventoryService, InventoryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


Globals.ConfigFolder = "/home/lna-dev/Downloads/config";
Globals.DataFolder = "/home/lna-dev/Downloads/video";

var contentDiscoveryService = app.Services.GetService<IContentDiscoveryService>();
contentDiscoveryService?.ActiveScan(Globals.DataFolder);
contentDiscoveryService?.Watch(Globals.DataFolder);

app.MapGet("/video", () =>  
{  
    string path = "/home/lna-dev/Downloads/Don't Hex the Water.webm";
  
    return Results.Stream(new FileStream(path, FileMode.Open), enableRangeProcessing: true, contentType: "video/webm");
});

app.MapGet("/video2", () =>  
{  
    string path = "/home/lna-dev/Downloads/100 Sachen.mp4";
  
    return Results.Stream(new FileStream(path, FileMode.Open), enableRangeProcessing: true, contentType: "video/mp4");
});

app.Run();