using System.Linq.Expressions;

namespace OpenMediaServer.Interfaces.Services;

public interface ITableBaseService<T>
{
    Task<T?> Get(Guid? id);
    Task UpdateOrInsert(T item);
    Task<IEnumerable<T>?> List(Expression<Func<T, bool>>? filter = null);
    Task Delete(Guid id);
}