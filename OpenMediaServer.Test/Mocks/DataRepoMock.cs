using System.Linq.Expressions;
using System.Text.Json;
using OpenMediaServer.Interfaces.Repositories;

namespace OpenMediaServer.Test.Mocks;

public class DataRepoMock : IDataRepository
{
    public List<string?> WrittenObjects { get; set; } = new();

    public Task<T?> GetObjectById<T>(Guid? id, Expression<Func<T, bool>>? additionalFilter = null)
    {
        throw new NotImplementedException();
    }

    public async Task WriteObject<T>(T item)
    {
        WrittenObjects.Add(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
    }

    public async Task WriteObjects<T>(IEnumerable<T>? items)
    {
        foreach (var item in items ?? [])
        {
            WrittenObjects.Add(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
        }
    }

    public async Task<IEnumerable<T>> ListObjects<T>(Expression<Func<T, bool>>? filter = null)
    {
        return [];
    }
    
    public async Task WriteObjectsAsync<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            WrittenObjects.Add(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
        }
    }

    public async Task DeleteObjectWithFilter<T>(Guid id, Expression<Func<T, bool>>? filter = null)
    {
        throw new NotImplementedException();
    }
    
    public async Task DeleteObjects<T>(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            WrittenObjects.Remove(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
        }
    }
}