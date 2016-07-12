// Copyright (c) 2015-16 Liam McSherry
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

namespace McSherry.SemanticVersioning.Monotonic
{
    /// <summary>
    /// <para>
    /// Provides tests for each of the methods provided by
    /// <see cref="MonotonicExtensions"/>.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class MonotonicExtensionsTests
    {
        private const string Category = "Monotonic Versioning - Extensions";

        /// <summary>
        /// <para>
        /// Tests whether the method
        /// <see cref="MonotonicExtensions.IsMonotonic(SemanticVersion)"/>
        /// will produce the expected result for correct cases.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void IsMonotonic_Valid()
        {
            // A semantic version is a valid monotonic version if it:
            //      a) Has a patch component of zero; and
            //      b) Has no pre-release identifiers.
            //
            // If either of these conditions is not fulfilled, a semantic
            // version is not a valid monotonic version.

            var m0 = new SemanticVersion(1, 2);
            var m1 = new SemanticVersion(1, 0, 0, new string[0], new[] { "x" });

            Assert.IsTrue(m0.IsMonotonic());
            Assert.IsTrue(m1.IsMonotonic());
        }
        /// <summary>
        /// <para>
        /// Tests that the method
        /// <see cref="MonotonicExtensions.IsMonotonic(SemanticVersion)"/>
        /// rejects semantic versions which are not monotonic versions.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void IsMonotonic_Invalid()
        {
            // Has a non-zero patch component
            var m0 = new SemanticVersion(2, 5, 1);
            // Has pre-release identifier components
            var m1 = new SemanticVersion(1, 0, 0, new[] { "abc" });
            // Is null.
            var m2 = (SemanticVersion)null;

            Assert.IsFalse(m0.IsMonotonic());
            Assert.IsFalse(m1.IsMonotonic());

            new Action(() => m2.IsMonotonic())
                .AssertThrows<ArgumentNullException>();
        }
    }
}
