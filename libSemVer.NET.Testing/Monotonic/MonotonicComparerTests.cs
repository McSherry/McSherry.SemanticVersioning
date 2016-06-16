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
    /// Tests to ensure that <see cref="MonotonicComparer"/> produces the
    /// results we expect when comparing <see cref="SemanticVersion"/>s.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class MonotonicComparerTests
    {
        private const string Category = "Monotonic Versioning - Comparer";

        private Ordering cmp(SemanticVersion x, SemanticVersion y)
            => MonotonicComparer.Standard.UseToCompare(x, y);

        // 0 > x -> (x > y || (x == null && y != null))
        // 0 = x -> (x == y)
        // 0 < x -> (y > x || (x != null && y == null))

        /// <summary>
        /// <para>
        /// Tests that basic monotonic versions produce the comparison
        /// results expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Basic()
        {
            var m0 = new SemanticVersion(1, 0);
            var m1 = new SemanticVersion(1, 1);

            Assert.AreEqual(Ordering.Lesser, cmp(m0, m1));

            Assert.AreEqual(Ordering.Greater, cmp(m1, m0));

            Assert.AreEqual(Ordering.Equal, cmp(m0, m0));

            Assert.AreEqual(Ordering.Equal, cmp(m1, m1));
        }
        /// <summary>
        /// <para>
        /// Tests that the correct precedence is reported even when
        /// the comparands have a different compatibility component.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_DifferentCompatibilities()
        {
            // Each of these should be higher precedence than the last.
            var m0 = new SemanticVersion(1, 0);
            var m1 = new SemanticVersion(2, 1);
            var m2 = new SemanticVersion(1, 2);

            Assert.AreEqual(Ordering.Lesser, cmp(m0, m1));
            Assert.AreEqual(Ordering.Lesser, cmp(m1, m2));

            Assert.AreEqual(Ordering.Lesser, cmp(m0, m2));


            Assert.AreEqual(Ordering.Greater, cmp(m2, m1));
            Assert.AreEqual(Ordering.Greater, cmp(m1, m0));

            Assert.AreEqual(Ordering.Greater, cmp(m2, m0));
        }
        /// <summary>
        /// <para>
        /// Tests that null values receive the correct precedence
        /// (i.e. that they are top-precedence).
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Nulls()
        {
            var m0 = new SemanticVersion(1, 0);
            var m1 = new SemanticVersion(1000, 1000);
            var m2 = (SemanticVersion)null;

            Assert.AreEqual(Ordering.Greater, cmp(m2, m0));
            Assert.AreEqual(Ordering.Greater, cmp(m2, m1));

            Assert.AreEqual(Ordering.Lesser, cmp(m0, m2));
            Assert.AreEqual(Ordering.Lesser, cmp(m1, m2));

            Assert.AreEqual(Ordering.Equal, cmp(m2, m2));
        }
        /// <summary>
        /// <para>
        /// Tests that versions with metadata are given correct precedence.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Metadata()
        {
            // Precedence with metadata is determined through lexically
            // sorting the first metadata items which differ, where the
            // "later" item has precedence.
            //
            // For example, given:
            //      2.5+abc.def.ghi
            //      2.5+abc.def.jkl
            //
            // Precedence would be determined by lexically sorting 'ghi'
            // and 'jkl'. In this case, 'jkl' comes after 'ghi', so
            // the second version string would have precedence.
            //
            // Like semantic versions, metadata items are constrained to
            // ASCII alphanumerics plus a hyphen, so it's fairly easy to
            // sort them.
            //
            // Notably, purely numeric metadata items are still sorted
            // lexically, so given:
            //      1.7+abc.73
            //      1.7+abc.2195
            //
            // Lexically, '2' comes before '7', so the former version
            // would have precedence even though '2195' would numerically
            // sort after '73'.
            //
            // However, the metadata is irrelevant if the compatibility
            // or release components would otherwise make one version of
            // greater precedence than another. So, given:
            //      1.5+xyz
            //      2.5+abc
            //
            // Even though the former's metadata would have higher precedence,
            // the latter's compatibility component gives it overall greater
            // precedence.

            var m0 = new SemanticVersion(2, 5, 0, new[] { "abc", "def", "ghi" });
            var m1 = new SemanticVersion(2, 5, 0, new[] { "abc", "def", "jkl" });

            var m2 = new SemanticVersion(1, 7, 0, new[] { "abc", "73" });
            var m3 = new SemanticVersion(1, 7, 0, new[] { "abc", "2195" });

            var m4 = new SemanticVersion(1, 5, 0, new[] { "xyz" });
            var m5 = new SemanticVersion(2, 5, 0, new[] { "abc" });


            Assert.AreEqual(Ordering.Lesser, cmp(m0, m1));
            Assert.AreEqual(Ordering.Greater, cmp(m1, m0));

            Assert.AreEqual(Ordering.Greater, cmp(m2, m3));
            Assert.AreEqual(Ordering.Lesser, cmp(m3, m2));

            Assert.AreEqual(Ordering.Lesser, cmp(m4, m5));
            Assert.AreEqual(Ordering.Greater, cmp(m5, m4));
        }

        /// <summary>
        /// <para>
        /// Tests that <see cref="MonotonicComparer"/> will refuse to
        /// compare versions that are not monotonic.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Invalid_NotMonotonic()
        {
            var m0 = new SemanticVersion(1, 6); // Is monotonic
            var m1 = new SemanticVersion(1, 7, 1); // Isn't monotonic

            new Action(() => cmp(m0, m1)).AssertThrows<ArgumentException>();
            new Action(() => cmp(m1, m0)).AssertThrows<ArgumentException>();
            new Action(() => cmp(m1, m1)).AssertThrows<ArgumentException>();
        }
    }
}
