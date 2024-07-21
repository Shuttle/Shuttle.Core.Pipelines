using Shuttle.Core.Contract;
using System;

namespace Shuttle.Core.Pipelines
{
    public static class StateExtensions
    {
        public static void Add<TItem>(this IState state, TItem value)
        {
            Guard.AgainstNull(state, nameof(state)).Add(typeof(TItem).FullName ?? throw new InvalidOperationException(), value);
        }

        public static void Add<TItem>(this IState state, string key, TItem value)
        {
            Guard.AgainstNull(key, nameof(key));

            Guard.AgainstNull(state, nameof(state)).Add(key, value);
        }

        public static void Replace<TItem>(this IState state, TItem value)
        {
            var key = typeof(TItem).FullName ?? throw new InvalidOperationException();

            Guard.AgainstNull(state, nameof(state)).Remove(key);

            state.Add(key, value);
        }

        public static void Replace<TItem>(this IState state, string key, TItem value)
        {
            Guard.AgainstNull(key, nameof(key));

            Guard.AgainstNull(state, nameof(state)).Remove(key);

            state.Add(key, value);
        }

        public static TItem Get<TItem>(this IState state, string key)
        {
            Guard.AgainstNull(key, nameof(key));

            return (TItem)Guard.AgainstNull(state, nameof(state)).Get(key);
        }

        public static TItem Get<TItem>(this IState state)
        {
            return Guard.AgainstNull(state, nameof(state)).Get<TItem>(typeof(TItem).FullName);
        }

        public static bool Remove<TItem>(this IState state)
        {
            var key = typeof(TItem).FullName ?? throw new InvalidOperationException();

            return Guard.AgainstNull(state, nameof(state)).Remove(key);
        }

        public static bool Contains<TItem>(this IState state)
        {
            var key = typeof(TItem).FullName ?? throw new InvalidOperationException();

            return Guard.AgainstNull(state, nameof(state)).Contains(key);
        }
    }
}