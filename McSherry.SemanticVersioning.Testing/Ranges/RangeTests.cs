// Copyright (c) 2015-19 Liam McSherry
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace McSherry.SemanticVersioning.Ranges
{
    /// <summary>
    /// <para>
    /// Provides tests for the base <see cref="VersionRange"/> functionality.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class RangeTests
    {
        private const string Category = "Version Range Base";


        /// <summary>
        /// <para>
        /// Tests the version range equals operator, including the implicit
        /// equals operator.
        /// Ensures that <see cref="VersionRange"/> implements the interface
        /// <see cref="VersionRange.IComparator"/>.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ImplementsIComparator()
        {
            var vr = new VersionRange("=1.0.0");

            Assert.IsInstanceOfType(vr, typeof(VersionRange.IComparator));
        }

        /// <summary>
        /// <para>
        /// Tests version range comparison using the basic operators with
        /// a single comparator in each range.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_Equals()
        {
            VersionRange im = new VersionRange("1.2.3"),
                         ex = new VersionRange("=1.2.3");

            void Test(string TID, VersionRange vr)
            {
                (string VID, SemanticVersion Version, bool Expected)[] vectors =
                {
                    ("T1", (SemanticVersion)"1.2.3",              true),
                    ("T2", (SemanticVersion)"1.2.3+meta.data",    true),

                    ("F1", (SemanticVersion)"1.2.2",          false),
                    ("F2", (SemanticVersion)"1.2.3-alpha",    false),
                    ("F3", (SemanticVersion)"1.2.4",          false),
                    ("F4", (SemanticVersion)"1.1.3",          false),
                    ("F5", (SemanticVersion)"1.3.3",          false),
                    ("F6", (SemanticVersion)"0.2.3",          false),
                    ("F7", (SemanticVersion)"2.2.3",          false),
                };

                foreach (var vector in vectors)
                {
                    Assert.AreEqual(
                        expected:   vector.Expected,
                        actual:     vr.SatisfiedBy(vector.Version),
                        message:    $"Failure: {TID}, vector {vector.VID}"
                        );
                }
            }

            Test("Implicit", im);
            Test("Explicit", ex);
        }
        /// <summary>
        /// <para>
        /// Tests the version range greater-than operator.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_GreaterThan()
        {
            // Tests for ranges not including pre-release identifiers
            var vr1 = new VersionRange(">1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.4",          true),
                ("T1.2", (SemanticVersion)"1.3.0",          true),
                ("T1.3", (SemanticVersion)"2.0.0",          true),
                ("T1.4", (SemanticVersion)"2.0.0+ab.cd",    true),


                ("F1.1", (SemanticVersion)"1.2.3",          false),
                ("F1.2", (SemanticVersion)"1.2.2",          false),
                ("F1.3", (SemanticVersion)"1.1.0",          false),
                ("F1.4", (SemanticVersion)"0.2.4",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F1.5", (SemanticVersion)"1.2.4-alpha",    false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers
            var vr2 = new VersionRange(">1.2.3-alpha.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.6",      true),
                ("T2.2", (SemanticVersion)"1.2.3-alpha.6.7",    true),
                ("T2.3", (SemanticVersion)"1.2.3-beta",         true),
                ("T2.4", (SemanticVersion)"1.2.3-alpha.a",      true),
                ("T2.5", (SemanticVersion)"1.2.4",              true),


                ("F2.1", (SemanticVersion)"1.2.3-alpha.4",  false),
                ("F2.2", (SemanticVersion)"1.2.3-alpha",    false),
                ("F2.3", (SemanticVersion)"1.2.4-alpha.2",  false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests the version range less-than operator.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_LessThan()
        {
            // Tests for ranges not including pre-release identifiers
            var vr1 = new VersionRange("<1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.2",          true),
                ("T1.2", (SemanticVersion)"1.1.0",          true),
                ("T1.3", (SemanticVersion)"0.5.0",          true),
                ("T1.4", (SemanticVersion)"0.5.0+ab.cd",    true),


                ("F1.1", (SemanticVersion)"1.2.3",          false),
                ("F1.2", (SemanticVersion)"1.2.4",          false),
                ("F1.3", (SemanticVersion)"1.3.0",          false),
                ("F1.4", (SemanticVersion)"2.5.0",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F1.5", (SemanticVersion)"1.2.4-alpha",    false),
                ("F1.6", (SemanticVersion)"1.2.3-alpha",    false),
                ("F1.7", (SemanticVersion)"1.2.2-alpha",    false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers
            var vr2 = new VersionRange("<1.2.3-alpha.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.4",  true),
                ("T2.2", (SemanticVersion)"1.2.3-alph",     true),
                ("T2.3", (SemanticVersion)"1.2.3-5678",     true),
                ("T2.4", (SemanticVersion)"1.2.3-5678+abc", true),
                ("T2.5", (SemanticVersion)"1.2.2",          true),

                ("F2.2", (SemanticVersion)"1.2.3-alpha.6",      false),
                ("F2.3", (SemanticVersion)"1.2.3-alpha.5.6",    false),
                ("F2.3", (SemanticVersion)"1.2.3-beta.2",       false),
                ("F2.4", (SemanticVersion)"1.2.2-beta.2",       false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests the version range greater-than-or-equal operator.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_GreaterThanOrEqual()
        {
            // Tests for ranges not including pre-release identifiers
            var vr1 = new VersionRange(">=1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.3",          true),
                ("T1.2", (SemanticVersion)"1.2.4",          true),
                ("T1.3", (SemanticVersion)"1.3.0",          true),
                ("T1.4", (SemanticVersion)"2.0.0",          true),
                ("T1.5", (SemanticVersion)"2.0.0+ab.cd",    true),


                ("F1.1", (SemanticVersion)"1.2.2",          false),
                ("F1.2", (SemanticVersion)"1.1.0",          false),
                ("F1.3", (SemanticVersion)"0.2.4",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F1.4", (SemanticVersion)"1.2.4-alpha",    false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers
            var vr2 = new VersionRange(">=1.2.3-alpha.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.5",      true),
                ("T2.2", (SemanticVersion)"1.2.3-alpha.6",      true),
                ("T2.3", (SemanticVersion)"1.2.3-alpha.6.7",    true),
                ("T2.4", (SemanticVersion)"1.2.3-alpha.a",      true),
                ("T2.5", (SemanticVersion)"1.2.4",              true),

                ("F2.1", (SemanticVersion)"1.2.3-alpha.4",      false),
                ("F2.2", (SemanticVersion)"1.2.3-alpha",        false),
                ("F2.3", (SemanticVersion)"1.2.4-alpha.2",      false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests the version range less-than-or-equal operator.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Operator_LessThanOrEqual()
        {
            // Tests for ranges not including pre-release identifiers
            var vr1 = new VersionRange("<=1.2.3");

            (string VID, SemanticVersion Version, bool Expected)[] vectors1 =
            {
                ("T1.1", (SemanticVersion)"1.2.3",          true),
                ("T1.2", (SemanticVersion)"1.2.2",          true),
                ("T1.3", (SemanticVersion)"1.1.0",          true),
                ("T1.4", (SemanticVersion)"0.5.0",          true),
                ("T1.5", (SemanticVersion)"0.5.0+ab.cd",    true),


                ("F1.1", (SemanticVersion)"1.2.4",          false),
                ("F1.2", (SemanticVersion)"1.3.0",          false),
                ("F1.3", (SemanticVersion)"2.5.0",          false),

                // Comparison rules for 'node-semver' differ from the typical
                // rules for Semantic Versioning: a pre-release version can
                // only satisfy a comparator set if at least one comparator in
                // the set has a matching major-minor-patch trio as well as
                // also having pre-release identifiers.
                ("F1.4", (SemanticVersion)"1.2.4-alpha",    false),
                ("F1.5", (SemanticVersion)"1.2.3-alpha",    false),
                ("F1.6", (SemanticVersion)"1.2.2-alpha",    false),
            };

            foreach (var vector in vectors1)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr1.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for ranges including pre-release identifiers
            var vr2 = new VersionRange("<=1.2.3-alpha.5");

            (string VID, SemanticVersion Version, bool Expected)[] vectors2 =
            {
                ("T2.1", (SemanticVersion)"1.2.3-alpha.5",  true),
                ("T2.2", (SemanticVersion)"1.2.3-alpha.4",  true),
                ("T2.3", (SemanticVersion)"1.2.3-alph",     true),
                ("T2.4", (SemanticVersion)"1.2.3-5678",     true),
                ("T2.5", (SemanticVersion)"1.2.3-5678+abc", true),
                ("T2.6", (SemanticVersion)"1.2.2",          true),

                ("F2.2", (SemanticVersion)"1.2.3-alpha.6",      false),
                ("F2.3", (SemanticVersion)"1.2.3-alpha.5.6",    false),
                ("F2.3", (SemanticVersion)"1.2.3-beta.2",       false),
                ("F2.4", (SemanticVersion)"1.2.2-beta.2",       false),
            };

            foreach (var vector in vectors2)
            {
                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     vr2.SatisfiedBy(vector.Version),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }

        /// <summary>
        /// <para>
        /// Tests version range comparison using a single comparator set with
        /// multiple comparators in it.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void MultipleComparators()
        {
            // This test is taken from the README of the 'node-semver'
            // repository, because what better source than the horse's
            // mouth?
            //
            // We only have a single test because we have another test
            // for the individual comparators, we just want to make sure
            // that multiple comparators are treated as expected.
            VersionRange vr0 = new VersionRange(">=1.2.7 <1.3.0")
                        ;

            SemanticVersion sv0 = new SemanticVersion(1, 2, 7), // These three are
                            sv1 = new SemanticVersion(1, 2, 8), // matches.
                            sv2 = new SemanticVersion(1, 2, 99),
                            sv3 = new SemanticVersion(1, 2, 6), // And these are
                            sv4 = new SemanticVersion(1, 3, 0), // mismatches.
                            sv5 = new SemanticVersion(1, 1, 0)
                            ;

            Assert.IsTrue(vr0.SatisfiedBy(sv0), "Incorrect rejection (0).");
            Assert.IsTrue(vr0.SatisfiedBy(sv1), "Incorrect rejection (1).");
            Assert.IsTrue(vr0.SatisfiedBy(sv2), "Incorrect rejection (2).");

            Assert.IsFalse(vr0.SatisfiedBy(sv3), "Incorrect acceptance (0).");
            Assert.IsFalse(vr0.SatisfiedBy(sv4), "Incorrect acceptance (1).");
            Assert.IsFalse(vr0.SatisfiedBy(sv5), "Incorrect acceptance (2).");
        }
        /// <summary>
        /// <para>
        /// Tests version range comparison using multiple comparator sets.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void MultipleComparatorSets()
        {
            // Like in the [MultipleComparators] test, this test is straight
            // from the 'node-semver' README, and there's only a single test
            // because individual comparators and multiple comparators are
            // already tested.
            VersionRange vr0 = new VersionRange("1.2.7 || >=1.2.9 <2.0.0");

            SemanticVersion sv0 = new SemanticVersion(1, 2, 7), // These match.
                            sv1 = new SemanticVersion(1, 2, 9),
                            sv2 = new SemanticVersion(1, 4, 6),
                            sv3 = new SemanticVersion(1, 2, 8), // These don't.
                            sv4 = new SemanticVersion(2, 0, 0)
                                ;

            Assert.IsTrue(vr0.SatisfiedBy(sv0), "Incorrect rejection (0).");
            Assert.IsTrue(vr0.SatisfiedBy(sv1), "Incorrect rejection (1).");
            Assert.IsTrue(vr0.SatisfiedBy(sv2), "Incorrect rejection (2).");

            Assert.IsFalse(vr0.SatisfiedBy(sv3), "Incorrect acceptance (0).");
            Assert.IsFalse(vr0.SatisfiedBy(sv4), "Incorrect acceptance (1).");
        }

        /// <summary>
        /// Tests that <see cref="VersionRange.MemoizationAgent"/> is used when
        /// assigned to, and that the correct values are assigned.
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Memoize_General()
        {
            var dict = new Dictionary<SemanticVersion, bool>();
            var vr = new VersionRange(">1.5.0");

            vr.MemoizationAgent = dict;


            // True evaluation
            var sv0 = SemanticVersion.Parse("1.5.1");
            var rs0 = vr.SatisfiedBy(sv0);

            Assert.IsTrue(dict.ContainsKey(sv0));
            Assert.IsTrue(rs0);
            Assert.AreEqual(rs0, dict[sv0]);
            Assert.AreEqual(1, dict.Count);


            // False evaluation
            var sv1 = SemanticVersion.Parse("1.4.0");
            var rs1 = vr.SatisfiedBy(sv1);

            Assert.IsTrue(dict.ContainsKey(sv1));
            Assert.IsFalse(rs1);
            Assert.AreEqual(rs1, dict[sv1]);
            Assert.AreEqual(2, dict.Count);
        }
    }
}
