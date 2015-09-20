// Copyright (c) 2015 Liam McSherry
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace McSherry.SemVer
{
    /// <summary>
    /// <para>
    /// Provides tests for the base <see cref="SemanticVersion"/> functionality.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class SemanticVersionTests
    {
        private const string Category = "Semantic Version Base";

        /// <summary>
        /// <para>
        /// Tests that validation of build metadata items is working
        /// as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void MetadataValidation()
        {
            var validItems = new string[]
            {
                // These items are taken from the specification.
                "001",                                  // 0
                "20130313144700",                       // 1
                "exp",                                  // 2
                "sha",                                  // 3
                "5114f85",                              // 4

                // These we came up with ourselves.
                "contains-some-hyphens",                // 5
                "-leading-hyphen",                      // 6
                "trailing-hyphen-",                     // 7
                "-leading-and-trailing-",               // 8

                // The specification doesn't mention
                // leading or trailing hyphens, so we
                // have to assume they're valid (if
                // ugly).
            };

            // Iterate through all the items we expect to be valid,
            // checking that they are valid.
            for (int i = 0; i < validItems.Length; i++)
            {
                Assert.IsTrue(SemanticVersion.IsValidMetadata(validItems[i]),
                              String.Format(
                                  "Unexpected rejection: item {0} (\"{1}\").",
                                  i, validItems[i]
                                  ));
            }

            var invItems = new string[]
            {
                "",                         // 0
                null,                       // 1
                " ",                        // 2
                "infix space",              // 3
                " leading space",           // 4
                "trailing space ",          // 5
                " leading and trailing ",   // 6
                "Tür",                      // 7
                "jalapeño",                 // 8
                "çava",                     // 9
                "?",                        // 10
            };
            // Iterate through all the items we expect to be invalid.
            for (int i = 0; i < invItems.Length; i++)
            {
                Assert.IsFalse(SemanticVersion.IsValidMetadata(invItems[i]),
                               String.Format(
                                   "Unexpected acceptance: item {0} (\"{1}\").",
                                   i, invItems[i]
                                   ));
            }
        }
        /// <summary>
        /// <para>
        /// Tests that validation of pre-release identifiers works
        /// as is expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void IdentifierValidation()
        {
            // Pre-release identifiers only differ from metadata items
            // in that they cannot have leading zeroes in their numeric
            // items. This simplifies our tests somewhat.

            // A lone zero does not count as a leading zero, and must be
            // considered valid.
            Assert.IsTrue(SemanticVersion.IsValidIdentifier("0"),
                          "Unexpected rejection: single zero.");

            // Leading zeroes don't matter if the identifier is not a
            // numeric identifier.
            Assert.IsTrue(SemanticVersion.IsValidIdentifier("00nonnumber"),
                          "Unexpected rejection: non-numeric leading zero.");

            // If the identifier is numeric, then we can't have any leading
            // zeroes.
            Assert.IsFalse(SemanticVersion.IsValidIdentifier("0150"),
                           "Unexpected acceptance: numeric leading zero.");
        }

        /// <summary>
        /// <para>
        /// Tests that <see cref="SemanticVersion.ToString"/> produces
        /// the expected result.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Stringifying()
        {
            // Make sure that the output is correct for correct input.
            Assert.AreEqual(new SemanticVersion(1, 2, 3).ToString(), "1.2.3",
                            "Unexpected [ToString] result (0).");

            Assert.AreEqual(
                new SemanticVersion(
                    major:          1,
                    minor:          2,
                    patch:          3,
                    identifiers:    new[] { "rc", "1" },
                    metadata:       new[] { "201509" }).ToString(),
                "1.2.3-rc.1+201509",
                "Unexpected [ToString] result (0).");
        }

        /// <summary>
        /// <para>
        /// Tests the <see cref="SemanticVersion"/> constructor to make sure it
        /// acts as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Constructing()
        {
            // Make sure that the expected exceptions are thrown for
            // invalid input.

            // Negative three-part components
            new Action(() => new SemanticVersion(-1, 0, 0))
                .AssertThrows<ArgumentOutOfRangeException>(
                    "Did not throw on invalid input (0).");
            new Action(() => new SemanticVersion(0, -1, 0))
                .AssertThrows<ArgumentOutOfRangeException>(
                    "Did not throw on invalid input (1).");
            new Action(() => new SemanticVersion(0, 0, -1))
                .AssertThrows<ArgumentOutOfRangeException>(
                    "Did not throw on invalid input (2).");


            // Null identifier/metadata collections
            new Action(delegate
            {
                new SemanticVersion(0, 0, 0, null, Enumerable.Empty<string>());
            }).AssertThrows<ArgumentNullException>(
                "Did not throw on invalid input (3).");

            new Action(delegate
            {
                new SemanticVersion(0, 0, 0, Enumerable.Empty<string>(), null);
            }).AssertThrows<ArgumentNullException>(
                "Did not throw on invalid input (4).");


            // Identifier/metadata collections *containing* null.
            new Action(delegate
            {
                new SemanticVersion(
                    major:          0,
                    minor:          0,
                    patch:          0,
                    identifiers:    new string[] { null },
                    metadata:       Enumerable.Empty<string>()
                    );
            }).AssertThrows<ArgumentNullException>(
                "Did not throw on invalid input (5).");

            new Action(delegate
            {
                new SemanticVersion(
                    major:          0,
                    minor:          0,
                    patch:          0,
                    identifiers:    Enumerable.Empty<string>(),
                    metadata:       new string[] { null }
                    );
            }).AssertThrows<ArgumentNullException>(
                "Did not throw on invalid input (6).");


            // Identifier/metadata collections containing an invalid value.
            new Action(delegate
            {
                new SemanticVersion(
                    major:          0,
                    minor:          0,
                    patch:          0,
                    identifiers:    new string[] { "0150" }, // Leading zero
                    metadata:       Enumerable.Empty<string>()
                    );
            }).AssertThrowsExact<ArgumentException>(
                "Did not throw on invalid input (7).");

            new Action(delegate
            {
                new SemanticVersion(
                    major:          0,
                    minor:          0,
                    patch:          0,
                    identifiers:    Enumerable.Empty<string>(),
                    metadata:       new string[] { "Löffel" } // Invalid character
                    );
            }).AssertThrowsExact<ArgumentException>(
                "Did not throw on invalid input (8).");
        }

        /// <summary>
        /// <para>
        /// Tests <see cref="SemanticVersion"/>'s implementation of
        /// <see cref="IComparable{T}"/> to ensure it acts as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Comparing()
        {
            Assert.IsTrue(typeof(IComparable<SemanticVersion>)
                            .IsAssignableFrom(typeof(SemanticVersion)),
                          "SemanticVersion is not IComparable<SemanticVersion>.");

            #region Definitions
            var cmp = new SemanticVersion[]
            {
                null,

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "alpha" },
                                    metadata:       Enumerable.Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "alpha", "1" },
                                    metadata:       Enumerable.Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "alpha", "beta" },
                                    metadata:       Enumerable.Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "beta" },
                                    metadata:       Enumerable.Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "beta", "2" },
                                    metadata:       Enumerable.Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "beta", "11" },
                                    metadata:       Enumerable.Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    new[] { "rc", "1" },
                                    metadata:       Enumerable.Empty<string>()),

                new SemanticVersion(major:          1,
                                    minor:          0,
                                    patch:          0,
                                    identifiers:    Enumerable.Empty<string>(),
                                    metadata:       Enumerable.Empty<string>()),
            };
            #endregion

            // Test the [CompareTo] method.
            Assert.AreEqual(cmp[1].CompareTo<SemanticVersion>(cmp[0]),
                            Ordering.Greater,
                            "Comparison failed (0).");
            Assert.AreEqual(cmp[1].CompareTo<SemanticVersion>(cmp[2]),
                            Ordering.Lesser,
                            "Comparison failed (1).");
            Assert.AreEqual(cmp[2].CompareTo<SemanticVersion>(cmp[3]),
                            Ordering.Lesser,
                            "Comparison failed (2).");
            Assert.AreEqual(cmp[3].CompareTo<SemanticVersion>(cmp[4]),
                            Ordering.Lesser,
                            "Comparison failed (3).");
            Assert.AreEqual(cmp[4].CompareTo<SemanticVersion>(cmp[5]),
                            Ordering.Lesser,
                            "Comparison failed (4).");
            Assert.AreEqual(cmp[5].CompareTo<SemanticVersion>(cmp[6]),
                            Ordering.Lesser,
                            "Comparison failed (5).");
            Assert.AreEqual(cmp[6].CompareTo<SemanticVersion>(cmp[7]),
                            Ordering.Lesser,
                            "Comparison failed (6).");
            Assert.AreEqual(cmp[7].CompareTo<SemanticVersion>(cmp[8]),
                            Ordering.Lesser,
                            "Comparison failed (7).");

            // To be extra sure, stick them in a collection, sort it, and
            // check the order they come out of the collection in. We jumble
            // up the items by ordering them using a newly-generated GUID.
            var sl = new List<SemanticVersion>(cmp.OrderBy(ks => Guid.NewGuid()));
            sl.Sort();

            // [cmp] is already in the correct lowest-to-highest order.
            Assert.IsTrue(sl.SequenceEqual(cmp), "Comparison failed (8).");
        }
    }
}
