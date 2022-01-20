﻿using CsharpExtras.Compare;
using System;
using System.Collections.Generic;
using System.Text;

namespace CsharpExtras.Map.Sparse.Compare
{
    internal class SparseArrayComparisonImpl<TVal> : IComparisonResult
    {
        public bool IsEqual => MessageAndIsEqual.isEqual;
        public string Message => MessageAndIsEqual.message;

        /// <param name="thisUsedValuesCount">Number of elements in this array</param>
        /// <param name="otherUsedValuesCount">Number of elements in other array</param>
        /// <param name="firstMismatch">First mistmach, if one exists, including the keys and the underly array comparison</param>
        public SparseArrayComparisonImpl(int thisDimension, int otherDimension, int thisUsedValuesCount, int otherUsedValuesCount,
            (IList<int> keyTuple, TVal val)? firstMismatch)
        {
            ThisUsedValuesCount = thisUsedValuesCount;
            OtherUsedValuesCount = otherUsedValuesCount;
            ThisDimension = thisDimension;
            OtherDimension = otherDimension;
            FirstMismatch = firstMismatch;
        }

        private int ThisDimension { get; }
        private int OtherDimension { get; }
        private int ThisUsedValuesCount { get; }
        private int OtherUsedValuesCount { get; }

        private (IList<int> keyTuple, TVal val)? FirstMismatch { get; }

        private (string, bool)? _messageAndIsEqual;
        private (string message, bool isEqual) MessageAndIsEqual => _messageAndIsEqual ??= GetMessageAndIsEqual();

        private (string message, bool isEqual) GetMessageAndIsEqual()
        {
            if (ThisDimension != OtherDimension)
            {
                return ($"Dimension mismatch. This array has arity = {ThisDimension}. Other array has arity = {OtherDimension}", false);
            }
            if (ThisUsedValuesCount != OtherUsedValuesCount)
            {
                return ($"Used Values Count mismatch. This array uses {ThisUsedValuesCount} elements. Other array uses {OtherUsedValuesCount} elements", false);
            }
            if (FirstMismatch is (IList<int> keyTuple, TVal val))
            {
                return ($"Mismatch found at key tuple {string.Join(",", keyTuple)} and value {val}", false);
            }
            return ("Arrays are equal", true);
        }
    }
}
