using MyPersonalFitness.Core.Interfaces;

namespace MyPersonalFitness.Core.Data;

/// <summary>
/// A simple thread-safe in-memory repository. Used as the default implementation for
/// the Web (Blazor WASM) project and for unit tests. Platform-specific projects
/// (MAUI) should override with a SQLite-backed implementation.
/// </summary>
public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _store = [];
    private int _nextId = 1;
    private readonly Func<T, int> _idGetter;
    private readonly Action<T, int> _idSetter;

    public InMemoryRepository(Func<T, int> idGetter, Action<T, int> idSetter)
    {
        _idGetter = idGetter;
        _idSetter = idSetter;
    }

    public Task<T?> GetByIdAsync(int id) =>
        Task.FromResult(_store.FirstOrDefault(e => _idGetter(e) == id));

    public Task<IEnumerable<T>> GetAllAsync() =>
        Task.FromResult<IEnumerable<T>>([.. _store]);

    public Task<int> AddAsync(T entity)
    {
        _idSetter(entity, _nextId++);
        _store.Add(entity);
        return Task.FromResult(_idGetter(entity));
    }

    public Task UpdateAsync(T entity)
    {
        var idx = _store.FindIndex(e => _idGetter(e) == _idGetter(entity));
        if (idx >= 0) _store[idx] = entity;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        _store.RemoveAll(e => _idGetter(e) == id);
        return Task.CompletedTask;
    }
}
