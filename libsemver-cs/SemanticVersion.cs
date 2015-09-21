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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// <para>
    /// Represents an immutable Semantic Version. This class cannot be inherited.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class represents a Semantic Version compliant with version
    /// 2.0.0 of the Semantic Versioning specification.
    /// </para>
    /// <para>
    /// Although the specification itself imposes no limit on version
    /// numbers, this class has the following limitations:
    /// </para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             The major, minor, and patch versions are represented
    ///             using an <see cref="int"/>, and so each component's
    ///             maximum value is 2,147,483,647.
    ///         </description>
    ///     </item>
    /// </list>
    /// </remarks>
    [Serializable]
    public sealed class SemanticVersion
        : IEquatable<SemanticVersion>, IComparable<SemanticVersion>
    {
        private const int CompareTo_Greater =  1,
                          CompareTo_Equal   =  0,
                          CompareTo_Lesser  = -1;

        private static IDictionary<string, SemanticVersion> _memDict;

        static SemanticVersion()
        {
            // The easiest way to represent a [SemanticVersion] is with a
            // string (e.g. "1.1.0-alpha.7"). Each time a string is used,
            // however, it must be parsed before it can be used, and this
            // is an expensive operation.
            //
            // To avoid having to parse each time a string is used, we're
            // going to build a cache of strings and versions and check it
            // each time we enter the parse method.
            //
            // We're using a [ConcurrentDictionary] because the cache is
            // static and may be used across threads.
            _memDict = new ConcurrentDictionary<string, SemanticVersion>();
        }

        /// <summary>
        /// <para>
        /// Determines whether the two specified <see cref="SemanticVersion"/>s
        /// are equal in value.
        /// </para>
        /// </summary>
        /// <param name="l">
        /// The first <see cref="SemanticVersion"/> to compare.
        /// </param>
        /// <param name="r">
        /// The second <see cref="SemanticVersion"/> to compare.
        /// </param>
        /// <returns>
        /// True if the provided <see cref="SemanticVersion"/>s are
        /// equal in value. False if otherwise.
        /// </returns>
        public static bool operator ==(SemanticVersion l, SemanticVersion r)
        {
            // If only one of the two operands is null, they are not equal.
            if (ReferenceEquals(l, null) ^ ReferenceEquals(r, null))
                return false;
            // If we get here, we know that either both or neither of the
            // operands are null. To determine if it's both, we just need
            // to test whether one is null. If one is, both are, and if
            // both are null, then they are equal.
            else if (ReferenceEquals(l, null))
                return true;
            // If we're here, then both have values, so we delegate the work
            // to the implementation of [IEquatable<T>].
            else
                return l.Equals(r);
        }
        /// <summary>
        /// <para>
        /// Determines whether the two specified <see cref="SemanticVersion"/>s
        /// are not equal in value.
        /// </para>
        /// </summary>
        /// <param name="l">
        /// The first <see cref="SemanticVersion"/> to compare.
        /// </param>
        /// <param name="r">
        /// The second <see cref="SemanticVersion"/> to compare.
        /// </param>
        /// <returns>
        /// True if the provided <see cref="SemanticVersion"/>s are
        /// not equal in value. False if otherwise.
        /// </returns>
        public static bool operator !=(SemanticVersion l, SemanticVersion r)
        {
            return !(l == r);
        }

        /// <summary>
        /// <para>
        /// Determines whether one <see cref="SemanticVersion"/> has greater
        /// precedence than another.
        /// </para>
        /// </summary>
        /// <param name="l">
        /// The <see cref="SemanticVersion"/> to check for greater precedence.
        /// </param>
        /// <param name="r">
        /// The <see cref="SemanticVersion"/> to compare against.
        /// </param>
        /// <returns>
        /// True if <paramref name="l"/> has greater precedence than
        /// <paramref name="r"/>. False if otherwise.
        /// </returns>
        public static bool operator >(SemanticVersion l, SemanticVersion r)
        {
            // If they're equal, then one cannot have greater precedence than
            // the other. Similarly, if [l] is null, it cannot have greater
            // precedence than [r] (because null always has lowest precedence).
            if (l == r || l == null)
                return false;

            // We know that [l] is not null, so we're free to call its [CompareTo]
            // method and let that compare it to [r]. We use [>=] because the
            // method is free to return any value greater than or equal to [1].
            return l.CompareTo(r) >= CompareTo_Greater;
        }
        /// <summary>
        /// <para>
        /// Determines whether one <see cref="SemanticVersion"/> has lesser
        /// precedence than another.
        /// </para>
        /// </summary>
        /// <param name="l">
        /// The <see cref="SemanticVersion"/> to check for lesser precedence.
        /// </param>
        /// <param name="r">
        /// The <see cref="SemanticVersion"/> to compare against.
        /// </param>
        /// <returns>
        /// True if <paramref name="l"/> has lesser precedence than
        /// <paramref name="r"/>. False if otherwise.
        /// </returns>
        public static bool operator <(SemanticVersion l, SemanticVersion r)
        {
            // No need for extra code, we can just swap the operands around and
            // use our already-defined [operator>] implementation.
            return r > l;
        }
        /// <summary>
        /// <para>
        /// Determines whether the precedence of one <see cref="SemanticVersion"/>
        /// is equal to or greater than the precedence of another.
        /// </para>
        /// </summary>
        /// <param name="l">
        /// The <see cref="SemanticVersion"/> to check for equal or greater
        /// precedence.
        /// </param>
        /// <param name="r">
        /// The <see cref="SemanticVersion"/> to compare against.
        /// </param>
        /// <returns>
        /// True if the precedence of <paramref name="l"/> is equal to or
        /// greater than the precedence of <paramref name="r"/>. False if
        /// otherwise.
        /// </returns>
        public static bool operator >=(SemanticVersion l, SemanticVersion r)
        {
            return (l == r) || (l > r);
        }
        /// <summary>
        /// <para>
        /// Determines whether the precedence of one <see cref="SemanticVersion"/>
        /// is equal to or less than the precedence of another.
        /// </para>
        /// </summary>
        /// <param name="l">
        /// The <see cref="SemanticVersion"/> to check for equal or lesser
        /// precedence.
        /// </param>
        /// <param name="r">
        /// The <see cref="SemanticVersion"/> to compare against.
        /// </param>
        /// <returns>
        /// True if the precedence of <paramref name="l"/> is equal to or less
        /// than the precedence of <paramref name="r"/>. False if otherwise.
        /// </returns>
        public static bool operator <=(SemanticVersion l, SemanticVersion r)
        {
            return (l == r) || (l < r);
        }


        private readonly int _major, _minor, _patch;
        private readonly List<string> _prIds, _metadata;

        /// <summary>
        /// <para>
        /// Creates a new <see cref="SemanticVersion"/> using the
        /// provided version components.
        /// </para>
        /// </summary>
        /// <param name="major">
        /// The semantic version's major version.
        /// </param>
        /// <param name="minor">
        /// The semantic version's minor version.
        /// </param>
        /// <param name="patch">
        /// The semantic version's patch version.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any of <paramref name="major,"/>
        /// <paramref name="minor"/>, and <paramref name="patch"/>
        /// is negative.
        /// </exception>
        public SemanticVersion(int major, int minor, int patch)
        {
            #region Check components aren't negative
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName:  nameof(major),
                    message:    "The major version component cannot be negative."
                    );
            }

            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName:  nameof(minor),
                    message:    "The minor version component cannot be negative."
                    );
            }

            if (patch < 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName:  nameof(patch),
                    message:    "The patch version component cannot be negative."
                    );
            }
            #endregion

            _major = major;
            _minor = minor;
            _patch = patch;

            _prIds = new List<string>();
            _metadata = new List<string>();

            // We want the user to be able to work with the pre-release
            // identifiers / build metadata, but we don't want them to
            // be able to modify any of them directly.
            //
            // We use [AsReadOnly] because it creates a wrapper, so the
            // user won't be able to cast back to [List<string>] and
            // make modifications.
            this.Identifiers = _prIds.AsReadOnly();
            this.Metadata = _metadata.AsReadOnly();
        }
        /// <summary>
        /// <para>
        /// Creates a new <see cref="SemanticVersion"/> using the
        /// provided version components and pre-release identifiers.
        /// </para>
        /// </summary>
        /// <param name="major">
        /// The semantic version's major version.
        /// </param>
        /// <param name="minor">
        /// The semantic version's minor version.
        /// </param>
        /// <param name="patch">
        /// The semantic version's patch version.
        /// </param>
        /// <param name="identifiers">
        /// The semantic version's pre-release identifiers.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any of <paramref name="major,"/>
        /// <paramref name="minor"/>, and <paramref name="patch"/>
        /// is negative.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="identifiers"/> or any of its
        /// items are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the items in <paramref name="identifiers"/>
        /// are not valid pre-release identifiers.
        /// </exception>
        public SemanticVersion(int major, int minor, int patch,
                               IEnumerable<string> identifiers)
            : this(major, minor, patch, identifiers, Enumerable.Empty<string>())
        {

        }
        /// <summary>
        /// <para>
        /// Creates a new <see cref="SemanticVersion"/> using the
        /// provided version components, pre-release identifiers,
        /// and metadata items.
        /// </para>
        /// </summary>
        /// <param name="major">
        /// The semantic version's major version.
        /// </param>
        /// <param name="minor">
        /// The semantic version's minor version.
        /// </param>
        /// <param name="patch">
        /// The semantic version's patch version.
        /// </param>
        /// <param name="identifiers">
        /// The semantic version's pre-release identifiers.
        /// </param>
        /// <param name="metadata">
        /// The semantic version's build metadata items.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any of <paramref name="major,"/>
        /// <paramref name="minor"/>, and <paramref name="patch"/>
        /// is negative.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the items in <paramref name="identifiers"/>
        /// or <paramref name="metadata"/> are null, or if the collections
        /// themselves are null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the items in <paramref name="identifiers"/>
        /// are not valid pre-release identifiers, or when any of the items
        /// in <paramref name="metadata"/> are not valid build metadata items.
        /// </exception>
        public SemanticVersion(int major, int minor, int patch,
                               IEnumerable<string> identifiers,
                               IEnumerable<string> metadata)
            : this(major, minor, patch)
        {
            #region Null check
            // No, the "!= false" isn't redundant. The [?.] operator
            // makes this return a [bool?], so the result is going to
            // be [true] (if any items are null), [false] (if everything
            // is fine), or [null] (if the collection is null).
            if (identifiers?.Any(id => id == null) != false)
            {
                throw new ArgumentNullException(
                    paramName:  nameof(identifiers),
                    message:    "The pre-release identifier collection cannot " +
                                "be null or contain null items."
                    );
            }

            if (metadata?.Any(meta => meta == null) != false)
            {
                throw new ArgumentNullException(
                    paramName:  nameof(metadata),
                    message:    "The build metadata item collection cannot " +
                                "be null or contain null items."
                    );
            }
            #endregion
            #region Validity check
            if (!identifiers.All(Helper.IsValidIdentifier))
            {
                throw new ArgumentException(
                    message:    "One or more pre-release identifiers is invalid.",
                    paramName:  nameof(identifiers));
            }

            if (!metadata.All(Helper.IsValidMetadata))
            {
                throw new ArgumentException(
                    message:    "One or more build metadata items is invalid.",
                    paramName:  nameof(metadata));
            }
            #endregion

            // We know that all of our pre-release identifiers/metadata
            // items are valid, so we can add them to the appropriate
            // collections and finish construction.
            _prIds.AddRange(identifiers);
            _metadata.AddRange(metadata);
        }

        /// <summary>
        /// <para>
        /// The semantic version's major version component.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// This version component is incremented each time a version with
        /// a breaking change in it is released. If this is zero, then the
        /// version number represents an unstable, pre-release version that
        /// may have breaking changes made at any time without an increment.
        /// </para>
        /// </remarks>
        public int Major => _major;
        /// <summary>
        /// <para>
        /// The semantic version's minor version component.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// This version component is incremented each time a version with at
        /// least one new feature in it is released. This component is reset
        /// to zero with every <see cref="Major"/> version increment.
        /// </para>
        /// </remarks>
        public int Minor => _minor;
        /// <summary>
        /// <para>
        /// The semantic version's patch version component.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// This version component is incremented each time a version with a
        /// backwards-compatible bug fix is released. This component is reset
        /// to zero with every <see cref="Major"/> or <see cref="Minor"/>
        /// version increment.
        /// </para>
        /// </remarks>
        public int Patch => _patch;

        /// <summary>
        /// <para>
        /// The pre-release identifier components of the semantic
        /// version.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Pre-release identifiers are used to indicate a release that
        /// is a pre-release. For example, <c>1.1.0-rc.1</c> for a release
        /// candidate.
        /// </para>
        /// </remarks>
        public IReadOnlyList<string> Identifiers
        {
            get;
            private set;
        }
        /// <summary>
        /// <para>
        /// The build metadata components of the semantic version.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Build metadata components provide additional information about
        /// a release, such as the time and date it was built and the commit
        /// identifier of the commit the release was built from.
        /// </para>
        /// </remarks>
        public IReadOnlyList<string> Metadata
        {
            get;
            private set;
        }

        /// <summary>
        /// <para>
        /// Determines whether the specified <see cref="SemanticVersion"/>
        /// is equivalent to the current version.
        /// </para>
        /// </summary>
        /// <param name="semver">
        /// The <see cref="SemanticVersion"/> to compare against.
        /// </param>
        /// <returns>
        /// True if the current <see cref="SemanticVersion"/> is equivalent
        /// to <paramref name="semver"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This differs from <see cref="Equals(SemanticVersion)"/> in that
        /// the value of <see cref="Metadata"/> is ignored.
        /// </para>
        /// </remarks>
        public bool EquivalentTo(SemanticVersion semver)
        {
            // If [semver] is null, we can't be equivalent to it.
            if (object.ReferenceEquals(semver, null))
                return false;

            return this.Major == semver.Major                           &&
                   this.Minor == semver.Minor                           &&
                   this.Patch == semver.Patch                           &&
                   this.Identifiers.SequenceEqual(semver.Identifiers);
        }

        // object Overrides
        /// <summary>
        /// <para>
        /// Determines whether the specified object is equal to
        /// the current object.
        /// </para>
        /// </summary>
        /// <param name="obj">
        /// The object to compare with the current object.
        /// </param>
        /// <returns>
        /// True if the specified and current objects are equal,
        /// false if otherwise.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method takes build metadata items into account when comparing,
        /// and so may return false for equivalent versions with differing build
        /// metadata.
        /// </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            var sv = obj as SemanticVersion;

            return !ReferenceEquals(sv, null) && this.Equals(semver: sv);
        }
        /// <summary>
        /// <para>
        /// Returns the hash code for this instance.
        /// </para>
        /// </summary>
        /// <returns>
        /// The hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            // Prime number as a starting value.
            int hash = 536870909;

            unchecked
            {
                // 1103 is another prime
                hash += 1103 * this.Major.GetHashCode();
                hash += 1103 * this.Minor.GetHashCode();
                hash += 1103 * this.Patch.GetHashCode();
                hash += 1103 * this.Identifiers.GetHashCode();
                hash += 1103 * this.Metadata.GetHashCode();
            }

            return hash;
        }
        /// <summary>
        /// <para>
        /// Returns a string that represents the current 
        /// <see cref="SemanticVersion"/>.
        /// </para>
        /// </summary>
        /// <returns>
        /// A string representing the current <see cref="SemanticVersion"/>.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();

            // The three numeric version components are always present,
            // so we can add them to the builder without any checks.
            sb.AppendFormat("{0}.{1}.{2}", this.Major, this.Minor, this.Patch);

            // Pre-release identifiers always come before metadata, but we need
            // to make sure there are identifiers to add first.
            if (this.Identifiers.Any())
            {
                // Identifiers are separated from the three-part version by
                // a hyphen character.
                sb.Append('-');

                // Each identifier is separated from the others by a period.
                this.Identifiers.Aggregate(
                    seed: sb,
                    func: (bdr, id) => bdr.AppendFormat("{0}.", id));

                // The way we concatenated the identifiers above, we'll be
                // left with a trailing period. We want to get rid of this.
                sb.Remove(
                    startIndex: sb.Length - 1, 
                    length:     1
                    );
            }

            // Like with the pre-release identifiers, we want to make sure there
            // is metadata to add before we attempt to add it.
            if (this.Metadata.Any())
            {
                // Metadata is separated from the three-part version/pre-release
                // identifiers by a plus character.
                sb.Append('+');

                // Like pre-release identifiers, each metadata item is separated
                // from other metadata items with a period.
                this.Metadata.Aggregate(
                    seed:   sb,
                    func:   (bdr, md) => bdr.AppendFormat("{0}.", md));

                // Like before, we're left with a trailing period.
                sb.Remove(
                    startIndex: sb.Length - 1,
                    length:     1
                    );
            }
            
            // We've constructed the string, so now we just need to return it.
            return sb.ToString();
        }

        // IEquatable<SemanticVersion> methods
        /// <summary>
        /// <para>
        /// Determines whether the specified <see cref="SemanticVersion"/>
        /// is equal to the current version.
        /// </para>
        /// </summary>
        /// <param name="semver">
        /// The <see cref="SemanticVersion"/> to compare with the current
        /// version.
        /// </param>
        /// <returns>
        /// True if the specified and current <see cref="SemanticVersion"/>s
        /// are equal, false if otherwise.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method takes build metadata items into account when comparing,
        /// and so may return false for equivalent versions with differing build
        /// metadata.
        /// </para>
        /// </remarks>
        public bool Equals(SemanticVersion semver)
        {
            // This is an instance method and so requires an instance to work,
            // so if we're passed [null] it can't be equal.
            if (object.ReferenceEquals(semver, null))
                return false;

            return this.Major == semver.Major                           &&
                   this.Minor == semver.Minor                           &&
                   this.Patch == semver.Patch                           &&
                   this.Identifiers.SequenceEqual(semver.Identifiers)   &&
                   this.Metadata.SequenceEqual(semver.Metadata);
        }

        // IComparable<SemanticVersion> methods
        /// <summary>
        /// <para>
        /// Compares the current <see cref="SemanticVersion"/> with
        /// another version to determine relative precedence.
        /// </para>
        /// </summary>
        /// <param name="semver">
        /// The <see cref="SemanticVersion"/> to compare to the current
        /// version.
        /// </param>
        /// <returns>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <term>Meaning</term>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <term>
        ///             The current <see cref="SemanticVersion"/> has
        ///             lesser precedence than <paramref name="semver"/>.
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <term>
        ///             The current <see cref="SemanticVersion"/> is
        ///             of equal precedence to <paramref name="semver"/>.
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <term>
        ///             The current <see cref="SemanticVersion"/> has
        ///             greater precedence than <paramref name="semver"/>.
        ///         </term>
        ///     </item>
        /// </list>
        /// </returns>
        public int CompareTo(SemanticVersion semver)
        {
            // If what we're comparing to is null, then it must
            // be the lowest precedence because it has no value.
            if (object.ReferenceEquals(semver, null))
                return CompareTo_Greater;

            #region Three-part Comparison
            // First thing to do is compare the major versions. If one
            // is greater than the other, then we don't need to perform
            // further checks.
            if (this.Major > semver.Major)
            {
                return CompareTo_Greater;
            }
            else if (this.Major < semver.Major)
            {
                return CompareTo_Lesser; // Lesser precedence
            }

            // If we're here, the major versions are equal, so we need
            // to perform the same comparison for the minor versions.
            if (this.Minor > semver.Minor)
            {
                return CompareTo_Greater;
            }
            else if (this.Minor < semver.Minor)
            {
                return CompareTo_Lesser;
            }

            // Again, if we're here, the minors are equal, and we do the
            // same comparison for patch versions.
            if (this.Patch > semver.Patch)
            {
                return CompareTo_Greater;
            }
            else if (this.Patch < semver.Patch)
            {
                return CompareTo_Lesser;
            }
            #endregion

            // If we *still* haven't returned from the method, then we need
            // to start checking the pre-release identifiers. Build metadata
            // isn't counted in a precedence comparison.

            // If only one of the versions has pre-release identifiers, then
            // the version with the identifiers is of lower precedence.
            if (this.Identifiers.Count == 0 ^ semver.Identifiers.Count == 0)
            {
                // We've got no pre-release identifiers, so the other one must
                // have them and is of lower precednece.
                if (this.Identifiers.Count == 0)
                {
                    return CompareTo_Greater;
                }
                // If we do have them, then we're of lower precedence.
                else
                {
                    return CompareTo_Lesser;
                }
            }
            // If both versions have equal three-part components and have no
            // pre-release identifiers, then they are of equal precedence.
            else if (this.Identifiers.Count == 0 && semver.Identifiers.Count == 0)
            {
                return CompareTo_Equal; // Equal precedence
            }
            // If the both versions have identical (content and ordering) sets
            // of pre-release identifiers, then they are of equal precedence.
            else if (this.Identifiers.SequenceEqual(semver.Identifiers))
            {
                return CompareTo_Equal;
            }

            // If both versions have pre-release identifiers, then this is where
            // things start to get more complicated. We now have to go through
            // the pre-release identifiers and compare them to determine which
            // version has higher precedence.

            var prEnumThis = this.Identifiers.GetEnumerator();
            var prEnumThat = semver.Identifiers.GetEnumerator();

            // We have to iterate through two collections at once, so we can't
            // use a [foreach] here.
            while (prEnumThis.MoveNext() & prEnumThat.MoveNext())
            {
                // We already know that the sets of pre-release identifiers are
                // not identical, so we need to find the first difference. To do
                // this, we just continue to the next iteration when the two
                // identifiers are equal.
                if (prEnumThis.Current == prEnumThat.Current)
                    continue;

                // We know that the items are different, so the first thing we
                // test for is whether the items are numeric.
                bool thisIsNumber = prEnumThis.Current.All(Helper.IsNumber),
                     thatIsNumber = prEnumThat.Current.All(Helper.IsNumber);

                // If both identifiers are numeric, then we perform a numeric
                // comparison of them.
                if (thisIsNumber && thatIsNumber)
                {
                    #region Number Comparison
                    // We're going to first try the comparison using longs.
                    try
                    {
                        long thisVal = long.Parse(prEnumThis.Current),
                             thatVal = long.Parse(prEnumThat.Current);

                        // If this version's pre-release identifier's value
                        // is greater, then this version is of greater
                        // precedence.
                        if (thisVal > thatVal)
                        {
                            return CompareTo_Greater;
                        }
                        // We already know they're not equal, so if ours
                        // isn't greater it must be lessser.
                        else
                        {
                            return CompareTo_Lesser;
                        }
                    }
                    // It's possible that the identifier will be too
                    // large for a [long], so if we get an [OverflowException]
                    // we're going to switch to a [BigInteger] and do the
                    // comparison again.
                    catch (OverflowException)
                    {
                        BigInteger thisVal = BigInteger.Parse(prEnumThis.Current),
                                   thatVal = BigInteger.Parse(prEnumThat.Current);

                        // Same as in the try part of this try-catch.
                        if (thisVal > thatVal)
                        {
                            return CompareTo_Greater;
                        }
                        else
                        {
                            return CompareTo_Lesser;
                        }
                    }
                    #endregion
                }
                // If only one is numeric, then the one that is not numeric has
                // higher precedence.
                else if (thisIsNumber)
                {
                    // This is a number, so we have lower precedence.
                    return CompareTo_Lesser;
                }
                else if (thatIsNumber)
                {
                    // That is a number, so we have higher precedence.
                    return CompareTo_Greater;
                }

                // If neither is numeric, then we have to compare differently.
                // Non-numeric identifiers are compared based on their ASCII
                // values.
                //
                // The simple way for us to do this is to use an ordinal
                // comparison (which takes the numeric values and compares
                // them).
                return String.CompareOrdinal(prEnumThis.Current, 
                                             prEnumThat.Current);
            }

            // When we end up here, we know that the identifier collections are
            // not of equal length. We also know that, since we're here, the items
            // in the longest collection are a superset of the items in the
            // shortest collection. We know this because we can only get here if
            // the above [while] loop hits the [continue] every time.

            // We now have to use the lengths of the collections to determine
            // which has higher precedence. Whichever collection has more items
            // has higher precedence.
            if (this.Identifiers.Count > semver.Identifiers.Count)
            {
                // We have more items, so we are of higher precedence.
                return CompareTo_Greater;
            }
            else
            {
                // We know they're not equal, so we must have fewer items
                // and so lower precedence.
                return CompareTo_Lesser;
            }
        }
    }
}
