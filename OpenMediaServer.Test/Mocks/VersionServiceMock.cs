using System.Linq.Expressions;
using OpenMediaServer.Interfaces.Services;
using OpenMediaServer.Models.Inventory;

namespace OpenMediaServer.Test.Mocks;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class VersionServiceMock : IVersionService
{
    private readonly ConcurrentDictionary<Guid, InventoryItemVersion> _store = new();

    public Task<InventoryItemVersion?> Get(Guid? id)
    {
        if (id is null) return Task.FromResult<InventoryItemVersion?>(null);
        _store.TryGetValue(id.Value, out var item);
        return Task.FromResult(item);
    }

    public Task UpdateOrInsert(InventoryItemVersion item)
    {
        if (item is null) throw new ArgumentNullException(nameof(item));

        // Ensure the item has an Id
        if (item.Id == Guid.Empty)
        {
            item.Id = Guid.NewGuid();
        }

        _store.AddOrUpdate(item.Id, item, (_, __) => item);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<InventoryItemVersion>?> List(Expression<Func<InventoryItemVersion, bool>>? filter = null)
    {
        IEnumerable<InventoryItemVersion> result = _store.Values;

        if (filter != null)
        {
            result = result.AsQueryable().Where(filter);
        }

        return Task.FromResult<IEnumerable<InventoryItemVersion>?>(result);
    }

    public Task Delete(Guid id)
    {
        _store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}