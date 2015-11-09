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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McSherry.SemanticVersioning.Ranges
{

    /*
        The implementation of this class is split across
        multiple files.

        Ranges\VersionRange.cs:
            Main implementation.

        Ranges\VersionRange.Parsing.cs:
            Parsing ranges, comparators, etc, from strings.
    */

    /// <summary>
    /// <para>
    /// Represents a range of <see cref="SemanticVersion"/> values.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Version ranges use the <c>node-semver</c> syntax for ranges.
    /// Specifically, ranges are based on the specification as it was
    /// written for the v5.0.0 release of <c>node-semver</c>.
    /// </para>
    /// <para>
    /// Presently, only the "basic" ranges syntax is supported. That is,
    /// any features under the "Advanced Range Syntax" heading are not
    /// supported.
    /// </para>
    /// </remarks>
    [Serializable]
    [CLSCompliant(true)]
    public sealed partial class VersionRange
    {
        /// <summary>
        /// <para>
        /// Determines whether the current version range is
        /// satisfied by a specified <see cref="SemanticVersion"/>.
        /// </para>
        /// </summary>
        /// <param name="semver">
        /// The <see cref="SemanticVersion"/> to check against the
        /// current version range.
        /// </param>
        /// <returns>
        /// True if the current version range is satisfied by
        /// <paramref name="semver"/>, false if otherwise.
        /// </returns>
        public bool SatisfiedBy(SemanticVersion semver)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// <para>
        /// Determines whether the current version range is satisfied
        /// by all specified <see cref="SemanticVersion"/> instances.
        /// </para>
        /// </summary>
        /// <param name="semvers">
        /// The <see cref="SemanticVersion"/> instances to check
        /// against the current version range.
        /// </param>
        /// <returns>
        /// True if all <see cref="SemanticVersion"/> instances in
        /// <paramref name="semvers"/> satisfy the current version
        /// range, false if otherwise.
        /// </returns>
        public bool SatisfiedBy(params SemanticVersion[] semvers)
        {
            return semvers.All(this.SatisfiedBy);
        }
    }
}
