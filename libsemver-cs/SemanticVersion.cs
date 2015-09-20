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
    }
}
