using System.Linq.Expressions;

namespace OpenMediaServer.Interfaces.Repositories;

public interface IDataRepository
{
    Task<IEnumerable<T>> ListObjects<T>(Expression<Func<T, bool>>? filter = null);
    
    Task<T?> GetObjectById<T>(Guid? id, Expression<Func<T, bool>>? additionalFilter = null);
    
    Task WriteObject<T>(T item);
    Task WriteObjects<T>(IEnumerable<T>? items);
    
    Task DeleteObjectWithFilter<T>(Guid id, Expression<Func<T, bool>>? filter = null);
    
    Task DeleteObjects<T>(IEnumerable<T> items);
}
