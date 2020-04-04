﻿using CustomExtensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomExtensions
{

    namespace CustomExtensions
    {
        [TestFixture]
        public class IEnumerableTest
        {
            private const string Zero = "Zero";
            private const string One = "One";
            private const string Two = "Two";

            [Test, Category("Unit"), Category("Quick")]
            [TestCaseSource("ValidIndexingData")]
            public void GivenEnumerableWhenIndexedThenResultantDictionaryIsAsExpected(int startIndex, int step, int[] expectedKeysInOrder)
            {
                string[] mockData = new string[] { Zero, One, Two };
                IDictionary<int, string> indexedDict = mockData.Index(startIndex, step);
                ICollection<int> actualKeys = indexedDict.Keys;
                Assert.AreEqual(expectedKeysInOrder, actualKeys, "Keys in resultant dictionary differ from those expected.");
                for(int originalIndex = 0; originalIndex < mockData.Length; originalIndex++)
                {
                    int keyInIndexedDict = expectedKeysInOrder[originalIndex];
                    string expectedData = mockData[originalIndex];
                    string actualData = indexedDict[keyInIndexedDict];
                    Assert.AreEqual(expectedData, actualData, string.Format(
                        "Indexed dictionary data does not match original enumerable data. Index in original enumerable: {0}. Index in indexed dictionary: {1}",
                        originalIndex, keyInIndexedDict));
                }
            }

            public static IEnumerable<object[]> ValidIndexingData()
            {
                return new List<object[]>()
                {
                    new object[] { 0, 2, new int[] { 0, 2, 4} },
                    new object[] { 0, -3, new int[] { 0, -3, -6} },
                    new object[] { 6, 5, new int[] { 6, 11, 16} },
                    new object[] { 6, -7, new int[] { 6, -1, -8} },
                };
            }
        }
    }

}
