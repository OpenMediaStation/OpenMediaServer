using System;
using System.Linq.Expressions;
using System.Text.Json;
using OpenMediaServer.Interfaces.Repositories;
using OpenMediaServer.Repositories;

namespace OpenMediaServer.Test.Mocks;

public class DataRepoMock : IDataRepository
{
    public List<string?> WrittenObjects { get; set; } = new();

    public async Task WriteObjectAsync<T>(T item)
    {
        WrittenObjects.Add(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
    }

    public void WriteObject<T>(T item)
    {
        WrittenObjects.Add(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
    }
    
    public async Task<IEnumerable<T>> ListObjectsAsync<T>(Expression<Func<T, bool>>? filter = null)
    {
        return [];
    }

    public IEnumerable<T> ListObjects<T>(Expression<Func<T, bool>>? filter = null)
    {
        return [];
    }

    public async Task<IEnumerable<TO>> ListObjectsAsync<T, TO>(Expression<Func<T, TO>>? select = null)
    {
        return [];
    }

    public IEnumerable<TO> ListObjects<T, TO>(Expression<Func<T, TO>>? select = null)
    {
        return [];
    }

    public async Task<T?> GetObjectByIdAsync<T>(Guid id, Expression<Func<T, bool>>? additionalFilter = null)
    {
        return default;
    }

    public T? GetObjectById<T>(Guid id, Expression<Func<T, bool>>? filter = null)
    {
        return default;
    }

    public async Task WriteObjectsAsync<T>(IEnumerable<T> items)
    {
        foreach(var item in items)
        {
            WrittenObjects.Add(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
        }
    }

    public void WriteObjects<T>(IEnumerable<T> items)
    {
        foreach(var item in items)
        {
            WrittenObjects.Add(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
        }
    }

    public async Task DeleteObjectAsync<T>(T item)
    {
        WrittenObjects.Remove(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
    }

    public void DeleteObject<T>(T item)
    {
        WrittenObjects.Remove(JsonSerializer.Serialize(item, options: Globals.JsonOptions));
    }

    public async Task DeleteObjectAsync<T>(Guid id, Expression<Func<T, bool>>? filter = null)
    {
        throw new NotImplementedException();
    }

    public void DeleteObject<T>(Guid id, Expression<Func<T, bool>>? filter = null)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteObjectsAsync<T>(IEnumerable<T> items)
    {
        throw new NotImplementedException();
    }

    public void DeleteObjects<T>(IEnumerable<T> items)
    {
        throw new NotImplementedException();
    }
}
