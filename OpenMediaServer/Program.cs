using OpenMediaServer;
using OpenMediaServer.APIs;
using OpenMediaServer.Endpoints;
using OpenMediaServer.Interfaces.APIs;
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
builder.Services.AddSingleton<IStreamingService, StreamingService>();
builder.Services.AddSingleton<IMetadataAPI, OMDbAPI>();
builder.Services.AddSingleton<IApiEndpoints, ApiEndpoints>();
builder.Services.AddSingleton<IStreamingEndpoints, StreamingEndpoints>();

builder.Services.AddHttpClient<IMetadataAPI, OMDbAPI>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var contentDiscoveryService = app.Services.GetService<IContentDiscoveryService>();
contentDiscoveryService?.ActiveScan(Globals.MediaFolder);
contentDiscoveryService?.Watch(Globals.MediaFolder);

app.Services.GetService<IApiEndpoints>()?.Map(app);
app.Services.GetService<IStreamingEndpoints>()?.Map(app);

app.Run();