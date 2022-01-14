﻿using CsharpExtras.Api;
using CsharpExtras.Event.Wrapper;
using CsharpExtras.Extensions;
using CsharpExtras.ValidatedType.Numeric.Integer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsharpExtras.Map.Dictionary.Curry
{
    class CurryDictionaryRecursive<TKey, TVal> : CurryDictionaryBase<TKey, TVal>
    {
        private readonly ICsharpExtrasApi _api;

        public event Action<int>? CountUpdated;

        public override TVal this[params TKey[] keys]
        {
            get => GetValueFromTuple(keys);
        }        
        public override NonnegativeInteger Arity { get; }

        //NB: We must not expose this to the outside world - otherwise we risk an inconsistent Count 
        private readonly IEventObjWrapper<IDictionary<TKey, ICurryDictionary<TKey, TVal>>, int> _currier;

        //NB: Do not update this explicitly - only UpdateCount function should be used for that
        private NonnegativeInteger _count = (NonnegativeInteger)0;
        public override NonnegativeInteger Count => _count;

        public CurryDictionaryRecursive(int arity, ICsharpExtrasApi api) : this((PositiveInteger)arity, api) { }

        public CurryDictionaryRecursive(PositiveInteger arity, ICsharpExtrasApi api)
        {
            Arity = (NonnegativeInteger)arity;
            _api = api;
            _currier = _api.NewEventObjWrapper<IDictionary<TKey, ICurryDictionary<TKey, TVal>>, int>
                (new Dictionary<TKey, ICurryDictionary<TKey, TVal>>(), UpdateCount);
        }
        public override IEnumerable<IList<TKey>> Keys => _currier.Get(c => (0, GetKeys(c)));

        private IEnumerable<IList<TKey>> GetKeys(IDictionary<TKey, ICurryDictionary<TKey, TVal>> currier)
        {
            foreach (KeyValuePair<TKey, ICurryDictionary<TKey, TVal>> pair in currier)
            {
                TKey key = pair.Key;
                ICurryDictionary<TKey, TVal> dict = pair.Value;
                IEnumerable<IList<TKey>> childKeyset = dict.Keys;
                foreach (IList<TKey> tuple in childKeyset)
                {
                    tuple.Insert(0, key);
                    yield return tuple;
                }
            }
        }

        public override bool ContainsKeyTuple(IEnumerable<TKey> keyTuple)
        {
            AssertArityIsCorrect(keyTuple);
            return ContainsKeyTuplePrefix(keyTuple);
        }

        public override bool ContainsKeyTuplePrefix(IEnumerable<TKey> prefix)
        {
            if (!prefix.Any())
            {
                return true;
            }
            TKey firstKey = prefix.First();
            if (!CurrierContainsKey(firstKey))
            {
                return false;
            }
            ICurryDictionary<TKey, TVal> curriedChild = GetCurriedChild(firstKey);
            IEnumerable<TKey> tailPrefix = prefix.Skip(1);
            return curriedChild.ContainsKeyTuplePrefix(tailPrefix);
        }

        private ICurryDictionary<TKey, TVal> GetCurriedChild(TKey firstKey)
        {
            return _currier.Get(c => (0, c[firstKey]));
        }

        private bool CurrierContainsKey(TKey firstKey)
        {
            return _currier.Get(c => (0, c.ContainsKey(firstKey)));
        }

        public override TVal GetValueFromTuple(IEnumerable<TKey> keyTuple)
        {
            Func<ICurryDictionary<TKey, TVal>, IEnumerable<TKey>, TVal> recursor =
                (dict, keys) => dict.GetValueFromTuple(keys);
            return TailRecurse(recursor, keyTuple);
        }

        public override ICurryDictionary<TKey, TVal> GetCurriedDictionary(IEnumerable<TKey> prefix)
        {
            if (!prefix.Any())
            {
                return this;
            }
            TKey firstKey = prefix.First();
            if (!CurrierContainsKey(firstKey))
            {
                throw new ArgumentException($"Cannot curry dictionary with given prefix as key {firstKey} is not found");
            }
            ICurryDictionary<TKey, TVal> curriedChild = GetCurriedChild(firstKey);
            IEnumerable<TKey> tailPrefix = prefix.Skip(1);
            return curriedChild.GetCurriedDictionary(tailPrefix);
        }

        public override bool Add(TVal value, IEnumerable<TKey> keyTuple)
        {
            AssertArityIsCorrect(keyTuple);
            TKey firstKey = keyTuple.First();
            IEnumerable<TKey> tail = keyTuple.Skip(1);
            if (CurrierContainsKey(firstKey))
            {
                ICurryDictionary<TKey, TVal> curryChild = GetCurriedChild(firstKey);
                return curryChild.Add(value, tail);
            }
            else if(Arity > 1)
            {
                CurryDictionaryRecursive<TKey, TVal> curryChild = new CurryDictionaryRecursive<TKey, TVal>(Arity - 1, _api);
                curryChild.CountUpdated += UpdateCount;
                AddDirectChild(firstKey, curryChild);
                bool isAddSuccessful = curryChild.Add(value, tail);
                return isAddSuccessful;
            }
            else
            {
                ICurryDictionary<TKey, TVal> curryChild = new NullaryCurryDictionary<TKey, TVal>(value);
                AddDirectChild(firstKey, curryChild);
                return true;
            }
        }

        private void AddDirectChild(TKey key, ICurryDictionary<TKey, TVal> curryChild)
        {
            int count = curryChild.Count;
            _currier.Run(c =>
            {
                c.Add(key, curryChild);
                return count;
            });
        }

        public override bool Update(TVal value, IEnumerable<TKey> keyTuple)
        {
            if (!ContainsKeyTuple(keyTuple))
            {
                return false;
            }
            return TailRecurse((d, k) => d.Update(value, k), keyTuple);
        }            

        /// <summary>
        /// Updates the count by the given amount
        /// </summary>
        /// <param name="delta">The amount, which can be negative, by which to update the count.</param>
        /// <exception cref="ArgumentException">Thrown if the updated count goes negative</exception>
        private void UpdateCount(int delta)
        {
            int newCount = Count + delta;
            if(newCount < 0)
            {
                throw new ArgumentException($"Cannot update count of {Count} by delta {delta} as it would result in a negative count");
            }
            _count = (NonnegativeInteger) (Count+delta);
            CountUpdated?.Invoke(delta);
        }

        //Assumes keyTuple is in this dictionary
        private TReturn TailRecurse<TReturn>(Func<ICurryDictionary<TKey, TVal>, IEnumerable<TKey>, TReturn> recursor,
            IEnumerable<TKey> keyTuple)
        {
            AssertArityIsCorrect(keyTuple);
            TKey firstKey = keyTuple.First();
            ICurryDictionary<TKey, TVal> curriedChild = GetCurriedChild(firstKey);
            IEnumerable<TKey> tail = keyTuple.Skip(1);
            return recursor(curriedChild, tail);
        }

        public override NonnegativeInteger Remove(IEnumerable<TKey> prefix)
        {
            if (!prefix.Any())
            {
                return (NonnegativeInteger)0;
            }
            TKey firstKey = prefix.First();
            if (!CurrierContainsKey(firstKey))
            {
                return (NonnegativeInteger)0;
            }
            ICurryDictionary<TKey, TVal> curryChild = GetCurriedChild(firstKey);
            IEnumerable<TKey> tail = prefix.Skip(1);
            if (!tail.Any())
            {                
                NonnegativeInteger removeCount = curryChild.Count;
                RemoveDirectChild(firstKey);
                return removeCount;
            }
            else
            {
                return curryChild.Remove(tail);
            }
        }

        private void RemoveDirectChild(TKey key)
        {
            if (!CurrierContainsKey(key))
            {
                return;
            }
            ICurryDictionary<TKey, TVal> curryChild = GetCurriedChild(key);
            int count = curryChild.Count;
            _currier.Run(c =>
            {
                if (c.Remove(key))
                {
                    return -count;
                }
                else
                {
                    return 0;
                }
            });
        }
    }
}
