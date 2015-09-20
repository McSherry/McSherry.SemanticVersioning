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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace McSherry.SemVer
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
    public sealed class SemanticVersion
        : IEquatable<SemanticVersion>
    {
        private static readonly Regex _metaRegex;

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

            // We're going to save ourselves some trouble and just use a
            // regular expression to check each individual build metadata
            // item.
            _metaRegex = new Regex("^[0-9A-Za-z-]+$");
        }

        /// <summary>
        /// <para>
        /// Checks whether a provided <see cref="string"/> is a valid
        /// pre-release identifier.
        /// </para>
        /// </summary>
        /// <param name="identifier">
        /// The pre-release identifier to check.
        /// </param>
        /// <returns>
        /// True if <paramref name="identifier"/> is a valid pre-release
        /// identifier, false if otherwise.
        /// </returns>
        public static bool IsValidIdentifier(string identifier)
        {
            // The rules for identifiers are a superset of the rules
            // for metadata items, so if it isn't a build metadata
            // item the it can't be an identifier.
            if (!SemanticVersion.IsValidMetadata(identifier))
                return false;

            // If the identifier is numeric (i.e. entirely numbers), then
            // it cannot have a leading zero.
            //
            // We have to check the length is greater than [1] because a
            // single ['0'] character as an identifier is valid.
            if (identifier.Length > 1 && identifier.First() == '0' &&
                identifier.All(c => c >= '0' && c <= '9'))
                return false;

            // It's passed the tests, so it's a valid identifier.
            return true;
        }
        /// <summary>
        /// <para>
        /// Checks whether a provided <see cref="string"/> is a valid
        /// build metadata item.
        /// </para>
        /// </summary>
        /// <param name="metadata">
        /// The build metadata item to check.
        /// </param>
        /// <returns>
        /// True if <paramref name="metadata"/> is a valid pre-release
        /// identifier, false if otherwise.
        /// </returns>
        public static bool IsValidMetadata(string metadata)
        {
            // Metadata items cannot be empty, and cannot contain
            // whitespace.
            if (String.IsNullOrWhiteSpace(metadata))
                return false;

            // A metadata item may only contain characters that are
            // alphanumeric or the hyphen character.
            if (!_metaRegex.IsMatch(metadata))
                return false;

            // It's passed the tests, so it's a valid metadata item.
            return true;
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
            if (!identifiers.All(IsValidIdentifier))
            {
                throw new ArgumentException(
                    message:    "One or more pre-release identifiers is invalid.",
                    paramName:  nameof(identifiers));
            }

            if (!metadata.All(IsValidMetadata))
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

            return sv != null && this.Equals(semver: sv);
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
    }
}
