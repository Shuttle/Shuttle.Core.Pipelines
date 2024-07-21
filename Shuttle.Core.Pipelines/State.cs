using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class State : IState
    {
        private readonly Dictionary<string, object> _state = new Dictionary<string, object>();

        public void Clear()
        {
            _state.Clear();
        }

        public void Add(string key, object value)
        {
            Guard.AgainstNull(key, nameof(key));

            _state.Add(key, value);
        }

        public void Replace(string key, object value)
        {
            Guard.AgainstNull(key, nameof(key));

            _state.Remove(key);
            _state.Add(key, value);
        }

        public object Get(string key)
        {
            Guard.AgainstNull(key, nameof(key));

            return _state.TryGetValue(key, out var result) ? result : default;
        }

        public bool Contains(string key)
        {
            Guard.AgainstNull(key, nameof(key));

            return _state.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _state.Remove(key);
        }
    }
}