﻿using CsharpExtras.Extensions.Helper.Dictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsharpExtras.Extensions
{
    public static class DictionaryExtension
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
        TKey key, TValue defaultValue)
        {
            return dictionary.GetValueOrDefault(key, () => defaultValue);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
             TKey key, Func<TValue> defaultValueProvider)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value
                 : defaultValueProvider();
        }

        /// <summary>
        /// Maps the values in the given dictionary using the given mapper function.
        /// </summary>
        /// <typeparam name="TKey">Type of keys in the dictionary</typeparam>
        /// <typeparam name="TValue">Type of values in the source dictionary</typeparam>
        /// <typeparam name="TMapped">Type of values in the target dictionary</typeparam>
        /// <param name="dictionary">The source dictionary</param>
        /// <param name="mapper">A function which, given a value of the source type, returns a value of the target type</param>
        /// <returns>A new dictionary whose keys are the same as the source dictionary and whose values are the 
        /// result of applying the mapper function over the values in the source dictionary.</returns>
        public static IDictionary<TKey, TMapped> MapValues<TKey, TValue, TMapped>
            (this IDictionary<TKey, TValue> dictionary, Func<TValue, TMapped> mapper) =>
            dictionary.MapValues((k, v) => mapper(v));

        /// <summary>
        /// Maps the values in the given dictionary using the given mapper function
        /// </summary>
        /// <typeparam name="TKey">Type of keys in the dictionary</typeparam>
        /// <typeparam name="TValue">Type of values in the source dictionary</typeparam>
        /// <typeparam name="TMapped">Type of values in the target dictionary</typeparam>
        /// <param name="dictionary">The source dictionary</param>
        /// <param name="mapper">A function which, given a key/value pair from the source dictionary, returns a value of the target type</param>
        /// <returns>A new dictionary whose keys are the same as the source dictionary and whose values are the 
        /// result of applying the mapper function over the key/value pairs in the source dictionary.</returns>
        public static IDictionary<TKey, TMapped> MapValues<TKey, TValue, TMapped>
            (this IDictionary<TKey, TValue> dictionary, Func<TKey, TValue, TMapped> mapper)
        {
            Dictionary<TKey, TMapped> dictToReturn = new Dictionary<TKey, TMapped>();
            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                TKey key = pair.Key;
                dictToReturn.Add(key, mapper(key, dictionary[key]));
            }
            return dictToReturn;
        }

        /// <summary>
        /// Zips together this dictionary with the other dictionary for all common keys.
        /// The keyset of the resulting dictionary will be the intersection of keysets of the two starting dictionaries.
        /// </summary>
        /// <typeparam name="TKey">The key type in both dictionaries</typeparam>
        /// <typeparam name="TValue">The value type in this dictionary</typeparam>
        /// <typeparam name="TOther">The value type in the other dictionary</typeparam>
        /// <typeparam name="TResult">The value type in the resultant dictionary</typeparam>
        /// <param name="zipper">The function that defines the values in the resultant dictionary.</param>
        /// <returns></returns>
        public static IDictionary<TKey, TResult> ZipValues<TKey, TValue, TOther, TResult>(
            this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TOther> other, Func<TValue, TOther, TResult> zipper)
        {
            IDictionary<TKey, TResult> resultDict = new Dictionary<TKey, TResult>();
            foreach(TKey key in dictionary.Keys)
            {
                if (other.ContainsKey(key))
                {
                    resultDict.Add(key, zipper(dictionary[key], other[key]));
                }
            }
            return resultDict;
        }

        public static IDictionary<TKey, TValue> FilterValues<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<TValue, bool> filter)
        {
            Dictionary<TKey, TValue> dictToReturn = new Dictionary<TKey, TValue>();
            foreach (TKey k in dictionary.Keys)
            {
                TValue value = dictionary[k];
                if (filter(value))
                {
                    dictToReturn.Add(k, value);
                }
            }
            return dictToReturn;
        }

        public static void IncrementValue<TKey>(this IDictionary<TKey, int> dictionary, TKey key, int incrementAmount = 1)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] += incrementAmount;
            }
            else
            {
                dictionary[key] = incrementAmount;
            }
        }

        public static void AddWithKeyDerivedFromValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TValue value, Func<TValue, TKey> func)
        {
            TKey key = func(value);
            dictionary.Add(key, value);
        }

        public static void AddWithValueDerivedFromKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey,TValue> func)
        {
            TValue value = func(key);
            dictionary.Add(key, value);
        }

        /// <summary>
        /// Compares this dictionary to another one. Dictionaries are deemed equal if they are the same size and contain the same set of key/value pairs.
        /// </summary>
        /// <param name="isEqualValues">This function is used to compare values within the dictionary, returning true iff values are equal in some sense</param>
        /// <returns>A dictionary comparison result</returns>
        public static IDictionaryComparison Compare<TKey, TValue>(this IDictionary<TKey, TValue> dictionary
            , IDictionary<TKey, TValue> other, Func<TValue, TValue, bool> isEqualValues)
        {
            int count = dictionary.Count;
            int otherCount = other.Count;
            if(count != otherCount)
            {
                return new DictionaryComparisonImpl<TKey, TValue>(count, otherCount, null);
            }
            foreach(KeyValuePair<TKey, TValue> kvp in dictionary)
            {
                TKey key = kvp.Key;
                if (other.ContainsKey(key))
                {
                    TValue value = kvp.Value;
                    TValue otherValue = other[key];
                    if(!isEqualValues(value, otherValue))
                    {
                        return new DictionaryComparisonImpl<TKey, TValue>(count, otherCount, (kvp.Key, kvp.Value));
                    }
                }
                else
                {
                    return new DictionaryComparisonImpl<TKey, TValue>(count, otherCount, (kvp.Key, kvp.Value));
                }
            }
            return new DictionaryComparisonImpl<TKey, TValue>(count, otherCount, null);
        }
    }
}
