﻿using CsharpExtras.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsharpExtras.Extensions
{
    public static class ArrayExtension2D
    {
        public static TVal[] FoldToSingleColumn<TVal>(this TVal[,] array, Func<TVal, TVal, TVal> foldFunction)
        {
            if (array.GetLength(0) == 0 || array.GetLength(1) == 0)
            {
                throw new ArgumentOutOfRangeException("Cannot fold an empty array");
            }

            TVal[] output = new TVal[array.GetLength(0)];
            for (int row = 0; row < array.GetLength(0); row++)
            {
                TVal[] rowData = array.SliceRow(row);
                output[row] = rowData.FoldToSingleValue(foldFunction);
            }
            return output;
        }

        public static TVal[] FoldToSingleRow<TVal>(this TVal[,] array, Func<TVal, TVal, TVal> foldFunction)
        {
            if (array.GetLength(0) == 0 || array.GetLength(1) == 0)
            {
                throw new ArgumentOutOfRangeException("Cannot fold an empty array");
            }

            TVal[] output = new TVal[array.GetLength(1)];
            for (int column = 0; column < array.GetLength(1); column++)
            {
                TVal[] columnData = array.SliceColumn(column);
                output[column] = columnData.FoldToSingleValue(foldFunction);
            }
            return output;
        }

        public static bool Any<TVal>(this TVal[,] array, Func<TVal, bool> checkerFunction)
        {
            for (int row = 0; row < array.GetLength(0); row++)
            {
                for (int column = 0; column < array.GetLength(1); column++)
                {
                    if (checkerFunction(array[row, column]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool All<TVal>(this TVal[,] array, Func<TVal, bool> checkerFunction)
        {
            Func<TVal, bool> inverseChecker = (value) => !checkerFunction(value);
            return !array.Any(inverseChecker);
        }

        public static int Count<TVal>(this TVal[,] array)
        {
            return array.GetLength(0) * array.GetLength(1);
        }

        public static int Count<TVal>(this TVal[,] array, Func<TVal, bool> checkerFunction)
        {
            int count = 0;
            for (int row = 0; row < array.GetLength(0); row++)
            {
                for (int column = 0; column < array.GetLength(1); column++)
                {
                    if (checkerFunction(array[row, column]))
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// Zip two 2D arrays into a single 2D array using a custom zipper function.
        /// If the two input arrays are of different sizes, the size of the output array is the intersection of the two input arrays.
        /// </summary>
        public static TResult[,] ZipArray<TVal, TOther, TResult>(this TVal[,] array, Func<TVal, TOther, TResult> zipper, TOther[,] other)
        {
            int zipLength0 = Math.Min(array.GetLength(0), other.GetLength(0));
            int zipLength1 = Math.Min(array.GetLength(1), other.GetLength(1));
            TResult[,] resultArrayZeroBased = new TResult[zipLength0, zipLength1];

            for (int dim0 = 0; dim0 < zipLength0; dim0++)
            {
                for (int dim1 = 0; dim1 < zipLength1; dim1++)
                {
                    resultArrayZeroBased[dim0, dim1] = zipper(array[dim0, dim1], other[dim0, dim1]);
                }
            }
            return resultArrayZeroBased;
        }

        public static TResult[,] Map<TVal, TResult>(this TVal[,] array, Func<TVal, TResult> mapper)
        {
            int length0 = array.GetLength(0);
            int length1 = array.GetLength(1);
            TResult[,] resultArray = new TResult[length0, length1];
            for (int i = 0; i < length0; i++)
            {
                for (int j = 0; j < length1; j++)
                {
                    resultArray[i, j] = mapper(array[i, j]);
                }
            }
            return resultArray;
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

        public static void AssertRowIndexIsInBounds<TVal>(this TVal[,] array, int row)
        {
            int length = array.GetLength(0);
            if (row < 0 || row >= length)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    "Row index {0} is out of bounds for 2D array. Expected range: [{1}, {2}]",
                    row, 0, length));
            }
        }
        public static void AssertColumnIndexIsInBounds<TVal>(this TVal[,] array, int column)
        {
            int length = array.GetLength(1);
            if (column < 0 || column >= length)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    "Column index {0} is out of bounds for 2D array. Expected range: [{1}, {2}]",
                    column, 0, length));
            }
        }

        /// <summary>
        /// Writes a 1D array to the specified row in a 2D array.
        /// The 1D array is shifted relative to the row in the 2D array by either a positive or negative shift.
        /// After the shift, the 1D array's values are written to any 2D array values that line up with it.
        /// Note: not all values in the 1D array are guaranteed to be written
        /// and not all values in the 2D row are guaranteed to be written.
        /// Only overlapping values will be written.
        /// </summary>        
        /// <param name="rowValues">The 1D array to write.</param>
        /// <param name="row">The row in the 2D array to which to write the array.</param>
        /// <param name="columnOffset">A positive or negative value specifying the amount by which to shift the 1D array.</param>
        public static void WriteToRow<TVal>(this TVal[,] array, TVal[] rowValues, int row, int columnOffset)
        {
            array.AssertRowIndexIsInBounds(row);
            int minColumnCount = Math.Min(rowValues.Length, array.GetLength(1) - columnOffset);
            int startColumn = Math.Max(0, -columnOffset);
            for(int column = startColumn; column < minColumnCount; column++)
            {
                array[row, column + columnOffset] = rowValues[column];
            }
        }

        /// <summary>
        /// Writes a 1D array to the specified column in a 2D array.
        /// The 1D array is shifted relative to the column in the 2D array by either a positive or negative shift.
        /// After the shift, the 1D array's values are written to any 2D array values that line up with it.
        /// Note: not all values in the 1D array are guaranteed to be written
        /// and not all values in the 2D column are guaranteed to be written.
        /// Only overlapping values will be written.
        /// </summary>        
        /// <param name="columnValues">The 1D array to write.</param>
        /// <param name="column">The column in the 2D array to which to write the array.</param>
        /// <param name="rowOffset">A positive or negative value specifying the amount by which to shift the 1D array.</param>h
        public static void WriteToColumn<TVal>(this TVal[,] array, TVal[] columnValues, int column, int rowOffset)
        {
            array.AssertColumnIndexIsInBounds(column);
            int minRowCount = Math.Min(columnValues.Length, array.GetLength(0) - rowOffset);
            int startRow = Math.Max(0, -rowOffset);
            for (int row = startRow; row < minRowCount; row++)
            {
                array[row + rowOffset, column] = columnValues[row];
            }
        }

        /// <summary>
        /// Rectangular sub array of this 2D array defined by the given coordinates.
        /// </summary>
        /// <param name="startAtRow">Row index to start at. Negative indices will be truncated to zero.</param>
        /// <param name="startAtCol">Column index to start at. Negative indices will be truncated to zero.</param>
        /// <param name="stopBeforeRow">Row index before which to stop. Indices greater than number of rows will be truncated to that number.</param>
        /// <param name="stopBeforeCol">Column index before which to stop. Indices greater than number of columns will be truncated to that number.</param>
        /// <returns></returns>
        public static TVal[,] SubArray<TVal>(this TVal[,] array, int startAtRow, int startAtColumn, int stopBeforeRow, int stopBeforeColumn)
        {
            startAtRow = Math.Max(startAtRow, 0);
            stopBeforeRow = Math.Min(stopBeforeRow, array.GetLength(0));
            startAtColumn = Math.Max(startAtColumn, 0);
            stopBeforeColumn = Math.Min(stopBeforeColumn, array.GetLength(1));

            int rowLength = stopBeforeRow - startAtRow;
            int columnLength = stopBeforeColumn - startAtColumn;
            //We want to return a canonical form of a zero size array. Don't want things like int[2, 0].
            if(rowLength == 0 || columnLength == 0)
            {
                rowLength = columnLength = 0;
            }
            TVal[,] result = new TVal[rowLength, columnLength];
            for(int row = startAtRow; row < stopBeforeRow; row++)
            {
                for(int col = startAtColumn; col < stopBeforeColumn; col++)
                {
                    result[row - startAtRow, col - startAtColumn] = array[row, col];
                }
            }
            return result;
        }
    }
}
