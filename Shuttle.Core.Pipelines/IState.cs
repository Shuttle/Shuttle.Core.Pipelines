namespace Shuttle.Core.Pipelines
{
    public interface IState
    {
        void Clear();
        void Add(string key, object value);
        void Replace(string key, object value);
        object Get(string key);
        bool Contains(string key);
        bool Remove(string key);
    }
}