using System.Linq.Expressions;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Interfaces.Services;

public interface IVersionService
{
    Task DeleteVersion(Guid versionId);

    Task<IEnumerable<InventoryItemVersion>?> ListItems(Expression<Func<InventoryItemVersion, bool>>? filter = null);
    Task UpdateOrInsert(InventoryItemVersion item);

}