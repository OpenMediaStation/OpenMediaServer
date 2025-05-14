using System.Linq.Expressions;

namespace OpenMediaServer.Interfaces.Repositories;

public interface IDataRepository
{
    #region queries
    Task<IEnumerable<T>> ListObjectsAsync<T>(Expression<Func<T, bool>>? filter = null);
    IEnumerable<T> ListObjects<T>(Expression<Func<T, bool>>? filter = null);
    Task<IEnumerable<TO>> ListObjectsAsync<T,TO>(Expression<Func<T,TO>>? select = null);
    IEnumerable<TO> ListObjects<T,TO>(Expression<Func<T,TO>>? select = null);
    Task<T?> GetObjectByIdAsync<T>(Guid id, Expression<Func<T, bool>>? additionalFilter = null);
    T? GetObjectById<T>(Guid id, Expression<Func<T, bool>>? filter = null);
    #endregion
    
    #region modifiers
    Task WriteObjectAsync<T>(T item);
    void WriteObject<T>(T item);
    
    Task WriteObjectsAsync<T>(IEnumerable<T> items);
    void WriteObjects<T>(IEnumerable<T> items);
    
    Task DeleteObjectAsync<T>(T item);
    void DeleteObject<T>(T item);
    
    Task DeleteObjectAsync<T>(Guid id, Expression<Func<T, bool>>? filter = null);
    void DeleteObject<T>(Guid id, Expression<Func<T, bool>>? filter = null);
    
    Task DeleteObjectsAsync<T>(IEnumerable<T> items);
    void DeleteObjects<T>(IEnumerable<T> items);
    
    #endregion
}
