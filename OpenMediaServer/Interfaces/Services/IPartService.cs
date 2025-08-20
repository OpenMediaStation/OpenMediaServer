using System.Linq.Expressions;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Interfaces.Services;

public interface IPartService
{
    Task DeletePart(Guid partId);
    Task<IEnumerable<InventoryItemPart>?> ListItems(Expression<Func<InventoryItemPart, bool>>? filter = null);
    Task UpdateOrInsert(InventoryItemPart item);
}