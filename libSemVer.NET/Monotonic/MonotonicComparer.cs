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

namespace McSherry.SemanticVersioning.Monotonic
{
    /// <summary>
    /// <para>
    /// Represents a semantic version comparison operation using
    /// monotonic versioning comparison rules.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This class compares monotonic versions as set out by the
    /// Monotonic Versioning Manifesto 1.2.
    /// </remarks>
    [CLSCompliant(true)]
    public sealed class MonotonicComparer
        : IComparer<SemanticVersion>
    {
        /// <summary>
        /// <para>
        /// A <see cref="MonotonicComparer"/> which compares using the
        /// standard rules for monotonic versions.
        /// </para>
        /// </summary>
        public static MonotonicComparer Standard
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private MonotonicComparer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <para>
        /// Compares two monotonic versions and returns an indication
        /// of their relative order.
        /// </para>
        /// </summary>
        /// <param name="x">
        /// A monotonic version to compare to <paramref name="y"/>.
        /// </param>
        /// <param name="y">
        /// A monotonic version to compare to <paramref name="x"/>.
        /// </param>
        /// <returns>
        /// <para>
        /// A value less than zero if <paramref name="x"/> precedes
        /// <paramref name="y"/> in sort order, or if
        /// <paramref name="x"/> is null and <paramref name="y"/>
        /// is not null.
        /// </para>
        /// <para>
        /// Zero if <paramref name="x"/> is equal to <paramref name="y"/>,
        /// including if both are null.
        /// </para>
        /// <para>
        /// A value greater than zero if <paramref name="x"/> follows
        /// <paramref name="y"/> in sort order, or if <paramref name="y"/>
        /// is null and <paramref name="x"/> is not null.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="x"/> or <paramref name="y"/> is not a valid
        /// monotonic version.
        /// </exception>
        public int Compare(SemanticVersion x, SemanticVersion y)
        {
            throw new NotImplementedException();
        }
    }
}
