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

namespace McSherry.SemanticVersioning
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
                "Unexpected [ToString] result (1).");
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
            new Action(() => new SemanticVersion(0, 0, 0, null))
                .AssertThrows<ArgumentNullException>(
                    "Did not throw on invalid input (3).");

            new Action(delegate
            {
                new SemanticVersion(0, 0, 0, Enumerable.Empty<string>(), null);
            }).AssertThrows<ArgumentNullException>(
                "Did not throw on invalid input (4).");


            // Identifier/metadata collections *containing* null.
            new Action(() => new SemanticVersion(0, 0, 0, new string[] { null }))
                .AssertThrows<ArgumentNullException>(
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
            new Action(() => new SemanticVersion(0, 0, 0, new[] { "0150" }))
                .AssertThrowsExact<ArgumentException>(
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
        public void Comparison()
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

            // These tests are mentioned by the MSDN docs, so we're going to
            // use them here just to make sure everything is working fine.
            Assert.AreEqual(cmp[1].CompareTo(cmp[1]), 0, 
                            "Comparison failed (9).");
            Assert.AreEqual(cmp[1].CompareTo(cmp[2]),
                            -cmp[2].CompareTo(cmp[1]),
                            "Comparison failed (10).");


            // Now we have to do practically the same tests again, but this time
            // testing the comparison operators rather than the [CompareTo]
            // method.
            Assert.IsTrue(cmp[1] > cmp[0], "Operator comparison failed (0).");
            Assert.IsTrue(cmp[2] > cmp[1], "Operator comparison failed (1).");
            Assert.IsTrue(cmp[3] > cmp[2], "Operator comparison failed (2).");
            Assert.IsTrue(cmp[4] > cmp[3], "Operator comparison failed (3).");
            Assert.IsTrue(cmp[5] > cmp[4], "Operator comparison failed (4).");
            Assert.IsTrue(cmp[6] > cmp[5], "Operator comparison failed (5).");
            Assert.IsTrue(cmp[7] > cmp[6], "Operator comparison failed (6).");
            Assert.IsTrue(cmp[8] > cmp[7], "Operator comparison failed (7).");

            // Same tests, but with the other operator.
            Assert.IsTrue(cmp[0] < cmp[1], "Operator comparison failed (8).");
            Assert.IsTrue(cmp[1] < cmp[2], "Operator comparison failed (9).");
            Assert.IsTrue(cmp[2] < cmp[3], "Operator comparison failed (10).");
            Assert.IsTrue(cmp[3] < cmp[4], "Operator comparison failed (11).");
            Assert.IsTrue(cmp[4] < cmp[5], "Operator comparison failed (12).");
            Assert.IsTrue(cmp[5] < cmp[6], "Operator comparison failed (13).");
            Assert.IsTrue(cmp[6] < cmp[7], "Operator comparison failed (14).");
            Assert.IsTrue(cmp[7] < cmp[8], "Operator comparison failed (15).");

            // These are the ones mentioned by the MSDN docs.
            Assert.IsFalse(cmp[1] > cmp[1], "Operator comparison failed (16).");
            Assert.IsFalse(cmp[1] < cmp[1], "Operator comparison failed (17).");
            Assert.IsTrue((cmp[1] > cmp[2]) == !(cmp[2] > cmp[1]),
                          "Operator comparison failed (18).");
            Assert.IsTrue((cmp[1] < cmp[2]) == !(cmp[2] < cmp[1]),
                          "Operator comparison failed (19).");
        }
        /// <summary>
        /// <para>
        /// Tests that <see cref="SemanticVersion"/>'s implementation
        /// of <see cref="IEquatable{T}"/> works as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Equality()
        {
            var sv0 = new SemanticVersion(1, 5, 0);
            var sv1 = new SemanticVersion(1, 5, 0, Enumerable.Empty<string>());
            var sv2 = new SemanticVersion(1, 5, 0, Enumerable.Empty<string>(),
                                                   Enumerable.Empty<string>());
            var sv3 = new SemanticVersion(1, 6, 0);
            var sv4 = new SemanticVersion(1, 6, 0, new[] { "rc", "1" });
            var sv5 = new SemanticVersion(1, 6, 0, Enumerable.Empty<string>(),
                                                   new[] { "d116bf47" });

            Assert.AreEqual(sv0, sv1, "Equality check failed (0).");
            Assert.AreEqual(sv0, sv2, "Equality check failed (1).");
            Assert.AreEqual(sv1, sv2, "Equality check failed (2).");
            Assert.AreNotEqual(sv0, sv3, "Equality check failed (3).");

            // Now we test the equality operators.
            //
            // Our [SemanticVersion] class is immutable, so it makes sense
            // to overload these to check value equality instead of reference
            // equality.
            Assert.IsTrue(sv0 == sv1, "Operator check failed (0).");
            Assert.IsTrue(sv0 == sv2, "Operator check failed (1).");
            Assert.IsTrue(sv1 == sv2, "Operator check failed (2).");
            Assert.IsTrue(sv0 != sv3, "Operator check failed (3).");

            // Now the tests mentioned in the guidelines for overloading
            // [Equals].
            Assert.IsTrue(sv0.Equals(sv1), "Guideline test failed (0).");
            Assert.IsTrue(sv1.Equals(sv0), "Guideline test failed (1).");
            Assert.IsTrue(sv1.Equals(sv0) && sv1.Equals(sv2) && sv0.Equals(sv2),
                          "Guideline test failed (2).");
            Assert.IsFalse(sv0.Equals(null), "Guideline test failed (3).");

            // The guideline tests again, but for our operators.
            Assert.IsTrue(sv0 == sv1, "Guideline test failed (4).");
            Assert.IsTrue(sv1 == sv0, "Guideline test failed (5).");
            Assert.IsTrue(sv1 == sv0 && sv1 == sv2 && sv0 == sv2,
                          "Guideline test failed (6).");
            Assert.IsFalse(sv0 == null, "Guideline test failed (7).");

            // Two null [SemanticVersion]s should be equal, so we're going to
            // test for that, too.
            SemanticVersion nsv0 = null, nsv1 = null;
            Assert.IsTrue(nsv0 == nsv1, "Null equality check failed (0).");

            // [Equals] and the equality operator must take into account metadata
            // and pre-release identifiers.
            Assert.AreNotEqual(sv3, sv4, "Inequality check failed (0).");
            Assert.AreNotEqual(sv3, sv5, "Inequality check failed (1).");
            Assert.AreNotEqual(sv4, sv5, "Inequality check failed (2).");

            Assert.IsFalse(sv3 == sv4, "Inequality check failed (3).");
            Assert.IsFalse(sv3 == sv5, "Inequality check failed (4).");
            Assert.IsFalse(sv4 == sv5, "Inequality check failed (5).");
        }
        /// <summary>
        /// <para>
        /// Tests that <see cref="SemanticVersion.EquivalentTo(SemanticVersion)"/>
        /// works as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Equivalence()
        {
            // [EquivalentTo] ignores any information that isn't relevant to
            // determining the compatibility of two versions. Basically, it
            // ignores any metadata.

            var sv0 = new SemanticVersion(1, 7, 0);
            var sv1 = new SemanticVersion(1, 7, 0, new[] { "beta", "3" });
            var sv2 = new SemanticVersion(1, 7, 0, Enumerable.Empty<string>(),
                                                   new[] { "d116bf47" });
            var sv3 = new SemanticVersion(1, 7, 0, new[] { "beta", "3" },
                                                   new[] { "d116bf47" });

            Assert.IsTrue(sv0.EquivalentTo(sv2), "Equivalence check failed (0).");
            Assert.IsTrue(sv1.EquivalentTo(sv3), "Equivalence check failed (1).");

            Assert.IsFalse(sv0.EquivalentTo(sv1), "Equivalence incorrect (0).");
            Assert.IsFalse(sv1.EquivalentTo(sv2), "Equivalence incorrect (1).");
            Assert.IsFalse(sv2.EquivalentTo(sv3), "Equivalence incorrect (2).");

            // Now we want to make sure that equivalence is working in both
            // directs (e.g. A = B and B = A).
            Assert.IsTrue(sv0.EquivalentTo(sv2) && sv2.EquivalentTo(sv0),
                          "Equivalence not commutative (0).");
            Assert.IsTrue(sv1.EquivalentTo(sv3) && sv3.EquivalentTo(sv1),
                          "Equivalence not commutative (1).");

            // And we might as well check the same for inequivalence.
            Assert.IsTrue(!sv0.EquivalentTo(sv1) && !sv1.EquivalentTo(sv0),
                          "Inequivalence not commutative (0).");
            Assert.IsTrue(!sv1.EquivalentTo(sv2) && !sv2.EquivalentTo(sv1),
                          "Inequivalence not commutative (1).");
            Assert.IsTrue(!sv2.EquivalentTo(sv3) && !sv3.EquivalentTo(sv2),
                          "Inequivalence not commutative (2).");
        }
    }
}
