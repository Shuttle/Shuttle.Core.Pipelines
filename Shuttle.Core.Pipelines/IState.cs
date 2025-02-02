namespace Shuttle.Core.Pipelines;

public interface IState
{
    void Add(string key, object? value);
    void Clear();
    bool Contains(string key);
    object? Get(string key);
    bool Remove(string key);
    void Replace(string key, object? value);
}