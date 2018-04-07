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

        public void Add(object value)
        {
            Guard.AgainstNull(value, nameof(value));

            _state.Add(value.GetType().FullName ?? throw new InvalidOperationException(), value);
        }

        public void Add(string key, object value)
        {
            Guard.AgainstNull(key, nameof(key));

            _state.Add(key, value);
        }

        public void Add<TItem>(TItem value)
        {
            _state.Add(typeof(TItem).FullName ?? throw new InvalidOperationException(), value);
        }

        public void Add<TItem>(string key, TItem value)
        {
            Guard.AgainstNull(key, nameof(key));

            _state.Add(key, value);
        }

        public void Replace(object value)
        {
            Guard.AgainstNull(value, nameof(value));

            var key = value.GetType().FullName ?? throw new InvalidOperationException();

            _state.Remove(key);
            _state.Add(key, value);
        }

        public void Replace(string key, object value)
        {
            Guard.AgainstNull(key, nameof(key));

            _state.Remove(key);
            _state.Add(key, value);
        }

        public void Replace<TItem>(TItem value)
        {
            var key = typeof(TItem).FullName ?? throw new InvalidOperationException();

            _state.Remove(key);
            _state.Add(key, value);
        }

        public void Replace<TItem>(string key, TItem value)
        {
            Guard.AgainstNull(key, nameof(key));

            _state.Remove(key);
            _state.Add(key, value);
        }

        public TItem Get<TItem>()
        {
            return Get<TItem>(typeof(TItem).FullName);
        }

        public TItem Get<TItem>(string key)
        {
            Guard.AgainstNull(key, nameof(key));

            if (_state.TryGetValue(key, out object result))
            {
                return (TItem) result;
            }

            return default(TItem);
        }

        public bool Contains(string key)
        {
            Guard.AgainstNull(key, nameof(key));

            return _state.ContainsKey(key);
        }
    }
}