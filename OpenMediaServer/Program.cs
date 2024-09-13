using OpenMediaServer;
using OpenMediaServer.Endpoints;
using OpenMediaServer.Interfaces.Endpoints;
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
builder.Services.AddSingleton<IMovieEndpoints, MovieEndpoints>();

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

var movieEndpoints = app.Services.GetService<IMovieEndpoints>();
movieEndpoints?.Map(app);

app.Run();