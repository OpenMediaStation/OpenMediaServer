using System.Linq.Expressions;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Services;

public class VersionService(ILogger<VersionService> logger, IDataRepository dataRepository) : TableBaseService<InventoryItemVersion>(dataRepository), IVersionService
{

}