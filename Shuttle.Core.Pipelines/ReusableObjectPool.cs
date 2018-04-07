using System;
using System.Collections.Generic;
using Shuttle.Core.Contract;

namespace Shuttle.Core.Pipelines
{
    public class ReusableObjectPool<TReusableObject>
        where TReusableObject : class
    {
        private static readonly object Lock = new object();
        private readonly Func<Type, TReusableObject> _factoryMethod;
        private readonly Dictionary<Type, List<TReusableObject>> _pool = new Dictionary<Type, List<TReusableObject>>();

        public ReusableObjectPool()
        {
        }

        public ReusableObjectPool(Func<Type, TReusableObject> factoryMethod)
        {
            Guard.AgainstNull(factoryMethod, nameof(factoryMethod));

            _factoryMethod = factoryMethod;
        }

        public TReusableObject Get(Type key)
        {
            Guard.AgainstNull(key, nameof(key));

            lock (Lock)
            {
                if (!_pool.TryGetValue(key, out var reusableObjects))
                {
                    reusableObjects = new List<TReusableObject>();
                    _pool.Add(key, reusableObjects);
                }

                if (reusableObjects.Count > 0)
                {
                    int lastIndex = reusableObjects.Count - 1;
                    var reusableObject = reusableObjects[lastIndex];

                    reusableObjects.RemoveAt(lastIndex);

                    return reusableObject;
                }

                return _factoryMethod?.Invoke(key);
            }
        }

        public bool Contains(TReusableObject instance)
        {
            Guard.AgainstNull(instance, nameof(instance));

            lock (Lock)
            {
                return _pool[instance.GetType()].Find(item => item.Equals(instance)) != null;
            }
        }

        public void Release(TReusableObject instance)
        {
            Guard.AgainstNull(instance, nameof(instance));

            lock (Lock)
            {
                var type = instance.GetType();
                if (!_pool.TryGetValue(type, out var reusableObjects))
                {
                    reusableObjects = new List<TReusableObject>();
                    _pool.Add(type, reusableObjects);
                }

                reusableObjects.Add(instance);
            }
        }
    }
}