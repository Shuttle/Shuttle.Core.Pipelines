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
                if (!_pool.ContainsKey(key))
                {
                    _pool.Add(key, new List<TReusableObject>());
                }

                if (_pool.Count > 0)
                {
                    var reusableObjects = _pool[key];

                    if (reusableObjects.Count > 0)
                    {
                        var reusableObject = reusableObjects[0];

                        reusableObjects.RemoveAt(0);

                        return reusableObject;
                    }
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
                if (!_pool.ContainsKey(instance.GetType()))
                {
                    _pool.Add(instance.GetType(), new List<TReusableObject>());
                }

                _pool[instance.GetType()].Add(instance);
            }
        }
    }
}