using System.Linq.Expressions;
using OpenMediaServer.Helpers;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Interfaces.Services;

namespace OpenMediaServer.Services;

public class TableBaseService<T>(IDataRepository dataRepository) : ITableBaseService<T>
{
    public async Task<T?> Get(Guid? id)
    {
        if (id == null) return default;
        
        var possibleItem = await dataRepository.GetObjectById<T>((Guid)id);
        
        return possibleItem;
    }
    
    public async Task UpdateOrInsert(T item)
    {
        await dataRepository.WriteObject(item);
    }

    public async Task<IEnumerable<T>?> List(Expression<Func<T, bool>>? filter = null)
    {
        var items = await dataRepository.ListObjects<T>();

        return items;
    }

    public async Task Delete(Guid id)
    {
        await dataRepository.DeleteObjectWithFilter<T>(id);
    }
}