// Copyright (c) 2015-18 Liam McSherry
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
        /// Tests version range comparison using the basic operators with
        /// a single comparator in each range.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void BasicComparison()
        {
            VersionRange    vr0 = new VersionRange("1.2.3"),
                            vr1 = new VersionRange("=1.2.3"),
                            vr2 = new VersionRange(">1.2.3"),
                            vr3 = new VersionRange("<1.2.3"),
                            vr4 = new VersionRange(">=1.2.3"),
                            vr5 = new VersionRange("<=1.2.3")
                            ;

            SemanticVersion sv0 = new SemanticVersion(1, 2, 3),
                            sv1 = new SemanticVersion(1, 2, 2),
                            sv2 = new SemanticVersion(1, 2, 3, new[] { "alpha" }),
                            sv3 = new SemanticVersion(1, 2, 4),
                            sv4 = new SemanticVersion(1, 2, 4, new[] { "alpha" })
                            ;

            // Testing satisfaction
            Assert.IsTrue(vr0.SatisfiedBy(sv0), "Incorrect rejection (0).");
            Assert.IsTrue(vr1.SatisfiedBy(sv0), "Incorrect rejection (1).");
            Assert.IsTrue(vr2.SatisfiedBy(sv3), "Incorrect rejection (2).");

            Assert.IsTrue(vr4.SatisfiedBy(sv0), "Incorrect rejection (4).");
            Assert.IsTrue(vr4.SatisfiedBy(sv3), "Incorrect rejection (5).");

            Assert.IsTrue(vr5.SatisfiedBy(sv0), "Incorrect rejection (6).");
            Assert.IsTrue(vr5.SatisfiedBy(sv1), "Incorrect rejection (7).");

            Assert.IsTrue(vr2.SatisfiedBy(sv4), "Incorrect rejection (8).");
            Assert.IsTrue(vr4.SatisfiedBy(sv4), "Incorrect rejection (9).");

            // And testing dissatisfaction
            Assert.IsFalse(vr0.SatisfiedBy(sv1), "Incorrect acceptance (0).");
            Assert.IsFalse(vr0.SatisfiedBy(sv2), "Incorrect acceptance (1).");
            Assert.IsFalse(vr0.SatisfiedBy(sv3), "Incorrect acceptance (2).");
            Assert.IsFalse(vr1.SatisfiedBy(sv1), "Incorrect acceptance (3).");
            Assert.IsFalse(vr1.SatisfiedBy(sv2), "Incorrect acceptance (4).");
            Assert.IsFalse(vr1.SatisfiedBy(sv3), "Incorrect acceptance (5).");
            
            Assert.IsFalse(vr3.SatisfiedBy(sv2), "Incorrect acceptance (6).");
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
