﻿using Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomExtensions
{
    public static class ArrayExtension
    {
        public enum ArrayOrientation { ROW, COLUMN }


        //Non-mvp: Test this
        /// <returns>A pair indicating the first element found and its index, or (-1, default) if nothing found.</returns>
        public static (int index, T element) FindFirstOccurrenceOfSet<T>(this T[] arr, ISet<T> set)
        {
            return FindFirstOccurrenceOfSet(arr, set, 0, arr.Length);
        }

        //Non-mvp: Test this
        /// <param name="startIndex">Start searching the array from this index inclusive i.e. don't look at lower indices</param>
        /// <param name="endIndex">Stop searching the array beyond this index, but include this index in the search. If this is >= length, then length is used.</param>
        /// <returns>A pair indicating the first element found and its index, or (-1, default) if nothing found.</returns>
        public static (int index, T element) FindFirstOccurrenceOfSet<T>(this T[] arr, ISet<T> set, int startIndex, int endIndex)
        {
            for (int i = startIndex; i <= arr.Length && i <= endIndex; i++)
            {
                T element = arr[i];
                if (set.Contains(element))
                {
                    return (i, element);
                }
            }
            return (-1, default);
        }


        public static T[] SubArray<T>(this T[] data, int startAt, int stopAt)
        {
            startAt = Math.Max(startAt, 0);
            stopAt = Math.Min(stopAt, data.Length);

            int length = stopAt - startAt;
            T[] result = new T[length];
            Array.Copy(data, startAt, result, 0, length);
            return result;
        }

        public static T[] SubArray<T>(this T[] data, int startAt)
        {
            return data.SubArray(startAt, data.Length);
        }

        public static string[] RemoveBlankEntries(this string[] data)
        {
            return data.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
        }

        public static T[] SliceRow<T>(this T[,] array, int row)
        {
            IEnumerable<T> enumSlice = array.SliceRowToEnum(row);
            return enumSlice.ToArray();
        }
        
        public static T[] SliceColumn<T>(this T[,] array, int column)
        {
            IEnumerable<T> enumSlice = array.SliceColumnToEnum(column);
            return enumSlice.ToArray();
        }

        public static T[,] To2DArray<T>(this T[] array, ArrayOrientation orientation)
        {
            T[,] outputArray;
            if (orientation == ArrayOrientation.ROW)
            {
                outputArray = new T[array.Length, 1];
            }
            else
            {
                outputArray = new T[1, array.Length];
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (orientation == ArrayOrientation.ROW)
                {
                    outputArray[i, 0] = array[i];
                }
                else
                {
                    outputArray[0, i] = array[i];
                }
            }
            return outputArray;
        }

        public static T[,] Transpose<T>(this T[,] array)
        {
            T[,] transposed = new T[array.GetLength(1), array.GetLength(0)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    transposed[j, i] = array[i, j];
                }
            }
            return transposed;
        }

        //TODO: Test
        public static T[] DeepCopy<T>(this T[] array)
        {
            int length = array.Length;
            T[] copy = new T[length];
            array.CopyTo(copy, 0);
            return copy;
        }

        public static IDictionary<T, IList<int>> Inverse<T>(this T[] array)
        {
            IDictionary<T, IList<int>> inverseMap = new Dictionary<T, IList<int>>();

            for (int index = 0; index < array.Length; index++)
            {
                T value = array[index];
                if (inverseMap.ContainsKey(value))
                {
                    inverseMap[value].Add(index);
                }
                else
                {
                    IList<int> lst = new List<int>();
                    lst.Add(index);
                    inverseMap.Add(value, lst);
                }
            }
            return inverseMap;
        }

        public static IDictionary<T, IList<int>> FindDuplicateIndices<T>(this T[] array)
        {
            return array.Inverse().FilterValues(lst => lst.Count > 1);
        }

        //Non-mvp: Test
        public static IDictionary<TVal, TOther> ZipToDictionary<TVal, TOther>(this TVal[] array, TOther[] other)
        {

            (TVal s, TOther u)[] zippedValues = array.ZipArray<TVal, TOther, (TVal, TOther)>((s, u) => (s, u), other);
            IDictionary<TVal, TOther> dict = new Dictionary<TVal, TOther>();
            foreach ((TVal leftValue, TOther rightValue) in zippedValues)
            {
                if (dict.ContainsKey(leftValue))
                {
                    throw new InvalidOperationException(string.Format(
                        "Zip failed: The key {0} seems to be duplicated in the left array.", leftValue));
                }
                dict.Add(leftValue, rightValue);
            }
            return dict;
        }

        public static IMultiValueMap<TVal, TOther> ZipToMultiValueMap<TVal, TOther>(this TVal[] array, TOther[] other)
        {
            (TVal s, TOther u)[] zippedValues = array.ZipArray<TVal, TOther, (TVal, TOther)>((s, u) => (s, u), other);
            IMultiValueMap<TVal, TOther> dict = new MultiValueMapImpl<TVal, TOther>();
            foreach ((TVal leftValue, TOther rightValue) in zippedValues)
            {
                dict.Add(leftValue, rightValue);
            }
            return dict;
        }

        public static TResult[] ZipArray<TVal, TOther, TResult>(this TVal[] array, Func<TVal, TOther, TResult> zipper, TOther[] other)
        {
            int zipLength = Math.Min(array.Length, other.Length);
            TResult[] resultArrayZeroBased = new TResult[zipLength];
            for (int i = 0; i < zipLength; i++)
            {
                resultArrayZeroBased[i] = zipper(array[i], other[i]);
            }
            return resultArrayZeroBased;
        }

        public static TResult[] ZipArray<TVal, TOther1, TOther2, TResult>(this TVal[] array, Func<TVal, TOther1, TOther2, TResult> zipper, TOther1[] other1, TOther2[] other2)
        {
            int zipLength = Math.Min(Math.Min(array.Length, other1.Length), other2.Length);
            TResult[] resultArrayZeroBased = new TResult[zipLength];
            for (int i = 0; i < zipLength; i++)
            {
                resultArrayZeroBased[i] = zipper(array[i], other1[i], other2[i]);
            }
            return resultArrayZeroBased;
        }

        public static TResult[] ZipArray<TVal, TOther1, TOther2, TOther3, TResult>
            (this TVal[] array, Func<TVal, TOther1, TOther2, TOther3, TResult> zipper, TOther1[] other1, TOther2[] other2, TOther3[] other3)
        {
            int zipLength = Math.Min(Math.Min(Math.Min(array.Length, other1.Length), other2.Length), other3.Length);
            TResult[] resultArrayZeroBased = new TResult[zipLength];
            for (int i = 0; i < zipLength; i++)
            {
                resultArrayZeroBased[i] = zipper(array[i], other1[i], other2[i], other3[i]);
            }
            return resultArrayZeroBased;
        }

        public static TResult[] ZipMulti<TOther, TResult>(this TResult[] array, Func<TResult, TOther, TResult> zipper, TOther[] other, params TOther[][] extras)
        {
            TResult[] currentZip = array.ZipArray(zipper, other);
            foreach(TOther[] extra in extras)
            {
                currentZip = currentZip.ZipArray<TResult, TOther, TResult>(zipper, extra);
            }
            return currentZip;
        }

        public static TResult[,] Map<TVal, TResult>(this TVal[,] array, Func<TVal, TResult> mapper)
        {
            int length0 = array.GetLength(0);
            int length1 = array.GetLength(1);
            TResult[,] resultArray = new TResult[length0, length1];
            for (int i = 0; i < length0; i++)
            {
                for(int j = 0; j < length1; j++)
                {
                    resultArray[i, j] = mapper(array[i,j]);
                }                
            }
            return resultArray;
        }

        public static TResult[] Map<TVal, TResult>(this TVal[] array, Func<TVal, TResult> mapper)
        {
            int length = array.Length;
            TResult[] resultArray = new TResult[length];
            for (int i = 0; i < length; i++)
            {
                resultArray[i] = mapper(array[i]);
            }
            return resultArray;
        }
    }
}
