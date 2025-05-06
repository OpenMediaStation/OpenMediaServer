using System.Linq.Expressions;

namespace OpenMediaServer.Interfaces.Repositories;

public interface IDataRepository
{
    Task WriteObject<T>(T item);
    Task<IEnumerable<T?>> ListObjects<T>(Expression<Func<T, bool>>? filter = null);
    Task<T?> GetObjectByID<T>(Guid id);
}