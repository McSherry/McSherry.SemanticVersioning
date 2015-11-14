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
    }
}
