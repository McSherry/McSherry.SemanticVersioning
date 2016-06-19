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
    /// The types of change which may be made to a monotonic-versioned
    /// software package.
    /// </para>
    /// </summary>
    public enum MonotonicChange
    {
        /// <summary>
        /// <para>
        /// A backwards-compatible change, where the change would not
        /// break existing uses of the software package's API.
        /// </para>
        /// </summary>
        Compatible,
        /// <summary>
        /// <para>
        /// A breaking change, where the change will break existing uses
        /// of the software package's API.
        /// </para>
        /// </summary>
        Breaking,
    }

    /// <summary>
    /// <para>
    /// Provides a method of working with <see cref="SemanticVersion"/>s as
    /// monotonic versions.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Monotonic Versioning is a simplified versioning scheme that
    /// is compatible with Semantic Versioning 2.0.0. The scheme uses
    /// two components: compatibility, indicating a "line of compatibility"
    /// where all versions with the same component are compatible; and
    /// release, which is incremented with every release, regardless of
    /// the line of compatibility.
    /// </para>
    /// <para>
    /// For example, a first release would be "1.0". A backwards-compatible
    /// update to this would be "1.1". If a breaking change was made, that
    /// release would be "2.2". However, if the first line of compatibility
    /// was updated again, it would be "1.3".
    /// </para>
    /// <para>
    /// The full manifesto is available from the Applied Computer Science
    /// Lab website. This class is based on the 1.2 manifesto.
    /// </para>
    /// </remarks>
    [CLSCompliant(true)]
    public sealed partial class MonotonicVersioner
    {
        /// <summary>
        /// <para>
        /// Sorts a collection of <see cref="SemanticVersion"/>s using 
        /// monotonic versioning sorting rules.
        /// </para>
        /// </summary>
        /// <param name="versions">
        /// The versions to be sorted.
        /// </param>
        /// <returns>
        /// A sorted collection of <see cref="SemanticVersion"/>s, with the
        /// highest-precedence version first in the collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="versions"/> or an item therein is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One of more of the items within <paramref name="versions"/> is
        /// not a valid monotonic version.
        /// </exception>
        public static IEnumerable<SemanticVersion> Sort(
            IEnumerable<SemanticVersion> versions
            )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <para>
        /// Creates a new <see cref="MonotonicVersioner"/> instance.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The <see cref="Compatibility"/> number sequence produced
        /// by an instance which was created using this constructor
        /// starts at one. If a zero-based sequence is required, use
        /// <see cref="MonotonicVersioner(bool)"/>.
        /// </remarks>
        public MonotonicVersioner()
            : this(startAtOne: true)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// <para>
        /// Creates a new <see cref="MonotonicVersioner"/> instance.
        /// Refer to remarks for differences from 
        /// <see cref="MonotonicVersioner()"/>.
        /// </para>
        /// </summary>
        /// <param name="startAtOne">
        /// If true, the produced <see cref="Compatibility"/> number
        /// sequence starts at one. If false, zero.
        /// </param>
        /// <remarks>
        /// <para>
        /// The Monotonic Versioning Manifesto 1.2 does not specify
        /// whether the <see cref="Compatibility"/> component of versions 
        /// are to start at one or zero. It is assumed that either is valid 
        /// as neither is specifically recommended nor prohibited.
        /// </para>
        /// <para>
        /// If the <see cref="Compatibility"/> components are to start at
        /// one, <see cref="MonotonicVersioner()"/> may be used.
        /// </para>
        /// </remarks>
        public MonotonicVersioner(bool startAtOne)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// <para>
        /// Creates a new <see cref="MonotonicVersioner"/> with the
        /// specified version number history.
        /// </para>
        /// </summary>
        /// <param name="chronology">
        /// A collection of version numbers providing the version
        /// history to use for this instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="chronology"/> or an item thereof is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="chronology"/> contains a version which is
        /// not a valid monotonic version.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>
        /// <paramref name="chronology"/> provides an incomplete
        /// version history. The chronology may:
        /// </para>
        /// <list type="bullet">
        ///     <item>
        ///     <description>
        ///     Not provide a contiguous sequence of <see cref="Compatibility"/>
        ///     numbers;
        ///     </description>
        ///     </item>
        ///     <item>
        ///     <description>
        ///     Not provide a contiguous sequence of <see cref="Release"/>
        ///     numbers;
        ///     </description>
        ///     </item>
        ///     <item>
        ///     <description>
        ///     Not contain a <see cref="Compatibility"/> starting at either
        ///     zero or one; or
        ///     </description>
        ///     </item>
        ///     <item>
        ///     <description>
        ///     Be empty.
        ///     </description>
        ///     </item>
        /// </list>
        /// </exception>
        public MonotonicVersioner(IEnumerable<SemanticVersion> chronology)
            : this()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <para>
        /// Returns the next version number when a specified change is
        /// made to the latest version.
        /// </para>
        /// </summary>
        /// <param name="change">
        /// The type of change being made to the latest version.
        /// </param>
        /// <returns>
        /// <para>
        /// The next version number produced when the specified change
        /// is made to the latest version.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Compatible"/>, the release number
        /// is incremented but the compatibility number remains the same.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Breaking"/>, both the release and
        /// compatibility numbers are incremented.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// <paramref name="change"/> is not a recognised type of change.
        /// </para>
        /// </exception>
        public SemanticVersion Next(MonotonicChange change)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// <para>
        /// Returns the next version number when a specified change is
        /// made to the latest version.
        /// </para>
        /// </summary>
        /// <param name="change">
        /// The type of change being made to the latest version.
        /// </param>
        /// <param name="metadata">
        /// The metadata to be included with the new version.
        /// </param>
        /// <returns>
        /// <para>
        /// The next version number produced when the specified change
        /// is made to the latest version.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Compatible"/>, the release number
        /// is incremented but the compatibility number remains the same.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Breaking"/>, both the release and
        /// compatibility numbers are incremented.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// <paramref name="change"/> is not a recognised type of change.
        /// </para>
        /// </exception>        
        /// <exception cref="ArgumentNullException">
        /// <paramref name="metadata"/> or an item therein is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or more items within <paramref name="metadata"/> is not a
        /// valid metadata string.
        /// </exception>
        public SemanticVersion Next(
            MonotonicChange change, IEnumerable<string> metadata
            )
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// <para>
        /// Returns the next version number when a specified change is
        /// made to a given line of compatibility.
        /// </para>
        /// </summary>
        /// <param name="line">
        /// The line of compatibility to which the change is being
        /// made.
        /// </param>
        /// <param name="change">
        /// The type of change being made to <paramref name="line"/>.
        /// </param>
        /// <returns>
        /// <para>
        /// The next version number produced when the specified change
        /// is made to the specified line of compatibility.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Compatible"/>, the release number
        /// is incremented but the compatibility number remains the same.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Breaking"/>, both the release and
        /// compatibility numbers are incremented.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// <paramref name="line"/> is negative.
        /// </para>
        /// <para>
        /// <paramref name="line"/> is not a current line of compatibility.
        /// </para>
        /// <para>
        /// <paramref name="change"/> is not a recognised type of change.
        /// </para>
        /// </exception>
        public SemanticVersion Next(int line, MonotonicChange change)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// <para>
        /// Returns the next version number when a specified change is
        /// made to a given line of compatibility.
        /// </para>
        /// </summary>
        /// <param name="line">
        /// The line of compatibility to which the change is being
        /// made.
        /// </param>
        /// <param name="change">
        /// The type of change being made to <paramref name="line"/>.
        /// </param>
        /// <param name="metadata">
        /// The metadata to be included with the new version.
        /// </param>
        /// <returns>
        /// <para>
        /// The next version number produced when the specified change
        /// is made to the specified line of compatibility.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Compatible"/>, the release number
        /// is incremented but the compatibility number remains the same.
        /// </para>
        /// <para>
        /// If <paramref name="change"/> is equal to
        /// <see cref="MonotonicChange.Breaking"/>, both the release and
        /// compatibility numbers are incremented.
        /// </para>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>
        /// <paramref name="line"/> is negative.
        /// </para>
        /// <para>
        /// <paramref name="line"/> is not a current line of compatibility.
        /// </para>
        /// <para>
        /// <paramref name="change"/> is not a recognised type of change.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="metadata"/> or an item therein is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// One or more items within <paramref name="metadata"/> is not a
        /// valid metadata string.
        /// </exception>
        public SemanticVersion Next(
            int line,
            MonotonicChange change,
            IEnumerable<string> metadata
            )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <para>
        /// Returns a <see cref="MonotonicVersioner"/> with an identical
        /// chronology, but which can advance its versions separately.
        /// </para>
        /// </summary>
        /// <returns>
        /// A <see cref="MonotonicVersioner"/> with an identical chronology
        /// up to the moment this method was called, but which is able
        /// to separately advance its version numbers.
        /// </returns>
        public MonotonicVersioner Clone()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// <para>
        /// The chronologically-latest version number in this versioning
        /// sequence.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// The chronologically-latest version is the version with the
        /// greatest value as its release component, regardless of the
        /// line of compatibility.
        /// </para>
        /// </remarks>
        public SemanticVersion Latest
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// <para>
        /// The latest versions in each line of compatibility, where the
        /// key is the line of compatibility.
        /// </para>
        /// </summary>
        public IReadOnlyDictionary<int, SemanticVersion> LatestVersions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// <para>
        /// The highest compatibility number. This component indicates which
        /// releases are compatible with each other.
        /// </para>
        /// </summary>
        public int Compatibility
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// <para>
        /// The current release number. This component indicates when a release
        /// was made relative to other releases.
        /// </para>
        /// </summary>
        public int Release
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// <para>
        /// The monotonic versions this instance has produced, in 
        /// chronological order.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// For monotonic versions, chronological order means that the
        /// versions are ordered by ascending release number.
        /// </para>
        /// </remarks>
        public IEnumerable<SemanticVersion> Chronology
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
