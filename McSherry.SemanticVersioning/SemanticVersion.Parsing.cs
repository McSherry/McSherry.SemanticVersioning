// Copyright (c) 2015-19 Liam McSherry
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

using McSherry.SemanticVersioning.Internals.Shims;

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// <para>
    /// Represents the parsing modes that the <see cref="SemanticVersion"/>
    /// parser may be configured to use.
    /// </para>
    /// </summary>
    [Flags]
    [CLSCompliant(true)]
    public enum ParseMode : int
    {
        /// <summary>
        /// <para>
        /// The default parser behaviour, with no set flags. This forces
        /// specification compliance.
        /// </para>
        /// </summary>
        Strict          = 0x00000000,
        /// <summary>
        /// <para>
        /// The opposite of <see cref="Strict"/>, with all parser flags
        /// set.
        /// </para>
        /// </summary>
        Lenient         = ~0,

        /// <summary>
        /// <para>
        /// The parser will accept a version prefixed with "v" or "V".
        /// </para>
        /// </summary>
        AllowPrefix     = 1 << 0,
        /// <summary>
        /// <para>
        /// The parser will accept versions with the 
        /// <see cref="SemanticVersion.Patch"/> version component omitted.
        /// </para>
        /// </summary>
        OptionalPatch   = 1 << 1,
    }

    // Documentation/attributes/interfaces/etc are in the main
    // implementation file, "SemanticVersion.cs"
    public sealed partial class SemanticVersion
    {
        /// <summary>
        /// <para>
        /// Represents the possible results of the <see cref="SemanticVersion"/>
        /// parser.
        /// </para>
        /// </summary>
        internal enum ParseResultType
        {
            /// <summary>
            /// <para>
            /// Parsing completed with no issues.
            /// </para>
            /// </summary>
            Success,

            /// <summary>
            /// <para>
            /// The string to parse was null or empty.
            /// </para>
            /// </summary>
            NullString,

            /// <summary>
            /// <para>
            /// An invalid character is present before the
            /// major-minor-patch trio.
            /// </para>
            /// </summary>
            PreTrioInvalidChar,

            /// <summary>
            /// <para>
            /// One or more items in the major-minor-patch
            /// trio contains an invalid character.
            /// </para>
            /// </summary>
            TrioInvalidChar,
            /// <summary>
            /// <para>
            /// One or more of the items in the major-minor-patch
            /// trio has a leading zero.
            /// </para>
            /// </summary>
            TrioItemLeadingZero,
            /// <summary>
            /// <para>
            /// One or more of the items in the major-minor-patch
            /// trio is missing.
            /// </para>
            /// </summary>
            TrioItemMissing,
            /// <summary>
            /// <para>
            /// One of the major, minor, or patch version components
            /// represents a number greater than <see cref="int.MaxValue"/>.
            /// </para>
            /// </summary>
            TrioItemOverflow,

            /// <summary>
            /// <para>
            /// An pre-release identifier was expected but not 
            /// found.
            /// </para>
            /// </summary>
            IdentifierMissing,
            /// <summary>
            /// <para>
            /// The version contains one or more invalid
            /// pre-release identifiers.
            /// </para>
            /// </summary>
            IdentifierInvalid,

            /// <summary>
            /// <para>
            /// A build metadata item was expected but
            /// not found.
            /// </para>
            /// </summary>
            MetadataMissing,
            /// <summary>
            /// <para>
            /// The version contains one or more invalid 
            /// build metadata items.
            /// </para>
            /// </summary>
            MetadataInvalid,
        }

        /// <summary>
        /// <para>
        /// Represents the different ways a major-minor-patch trio component
        /// can be specified in a version string.
        /// </para>
        /// </summary>
        internal enum ComponentState
        {
            /// <summary>
            /// <para>
            /// The component was present in the version string.
            /// </para>
            /// </summary>
            Present,

            /// <summary>
            /// <para>
            /// The component was omitted and the omission was acceptable due
            /// to the provided <see cref="ParseMode"/>.
            /// </para>
            /// </summary>
            Omitted,

            /// <summary>
            /// <para>
            /// A wildcard was present in the place of the component and this
            /// substitution was acceptable due to the provided
            /// <see cref="ParseMode"/>.
            /// </para>
            /// </summary>
            Wildcard,
        }

        /// <summary>
        /// <para>
        /// Represents internal <see cref="ParseMode"/> values which don't make
        /// sense to expose to users.
        /// </para>
        /// </summary>
        internal static class InternalModes
        {
            /// <summary>
            /// <para>
            /// A mask for the bits the internal mode bits occupy.
            /// </para>
            /// </summary>
            public const ParseMode Mask             = (ParseMode)(0b1111 << 28);


            /// <summary>
            /// <para>
            /// The internal parser modes are enabled.
            /// </para>
            /// </summary>
            public const ParseMode Enabled          = (ParseMode)(0b1000 << 28);

            /// <summary>
            /// <para>
            /// The parser will accept versions with the <see cref="Minor"/>
            /// version component omitted. Has no effect if
            /// <see cref="ParseMode.OptionalPatch"/> is not also specified.
            /// </para>
            /// </summary>
            public const ParseMode OptionalMinor    = (ParseMode)(0b0001 << 28);
            /// <summary>
            /// <para>
            /// The parser will accept a wildcard in place of the major, minor,
            /// or patch version component.
            /// </para>
            /// </summary>
            /// <remarks>
            /// If a wildcard is specified, any subordinate components must be
            /// omitted for the version to be considered valid. This parse mode
            /// will allow their omission without <see cref="OptionalMinor"/>
            /// or <see cref="ParseMode.OptionalPatch"/> being specified, and
            /// will set their <see cref="ComponentState"/> in
            /// <see cref="ParseMetadata"/> to <see cref="ComponentState.Wildcard"/>.
            /// </remarks>
            public const ParseMode AllowWildcard    = (ParseMode)(0b0010 << 28);


            /// <summary>
            /// <para>
            /// Determines whether an internal parse mode is present.
            /// </para>
            /// </summary>
            /// <param name="target">
            /// The <see cref="ParseMode"/> to check for the specified flags.
            /// </param>
            /// <param name="mode">
            /// The flags to check for.
            /// </param>
            /// <returns>
            /// True if the specified internal parse modes are present.
            /// </returns>
            public static bool Has(ParseMode target, ParseMode mode)
            {
                // These internal modes are currently used to enable version
                // ranges to be properly implemented, as some features from
                // the 'node-semver' specification require special handling in
                // the parser to work.
                //
                // To avoid unintended use through [ParseMode.Lenient], we require
                // that the sign bit be unset. As [Lenient] sets all bits high,
                // the sign bit will also be high when it is used and so internal
                // modes will not be used.
                return (target ^ Enabled) < 0 && (target & mode) == mode;
            }
            /// <summary>
            /// <para>
            /// Determines whether any internal parse mode is present.
            /// </para>
            /// </summary>
            /// <param name="mode">
            /// The <see cref="ParseMode"/> to check for internal modes.
            /// </param>
            /// <returns></returns>
            public static bool HasAny(ParseMode mode)
            {
                return (mode ^ Enabled) < 0 && (Mask & mode) > 0;
            }
        }

        /// <summary>
        /// <para>
        /// Represents the result produced by the <see cref="SemanticVersion"/>
        /// parser.
        /// </para>
        /// </summary>
        internal class ParseResult
        {
            /// <summary>
            /// <para>
            /// Creates a new successful <see cref="ParseResult"/> with
            /// the specified <see cref="SemanticVersion"/> as its payload.
            /// </para>
            /// </summary>
            /// <param name="version">
            /// The <see cref="SemanticVersion"/> to provide as the result.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="version"/> is null.
            /// </exception>
            public ParseResult(SemanticVersion version)
            {
                if (version == null)
                {
                    throw new ArgumentNullException(
                        paramName:  nameof(version),
                        message:    "The provided SemanticVersion cannot be null."
                        );
                }

                this.Type = ParseResultType.Success;
                this.Version = version;
            }
            /// <summary>
            /// <para>
            /// Creates a new failure-state <see cref="ParseResult"/> with
            /// the specified <see cref="ParseResultType"/> as its payload.
            /// </para>
            /// </summary>
            /// <param name="error">
            /// The error code to provide to consuming code.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="error"/> is equal to
            /// <see cref="ParseResultType.Success"/>, or is unrecognised.
            /// </exception>
            public ParseResult(ParseResultType error)
            {
                if (!Enum.IsDefined(typeof(ParseResultType), error))
                {
                    throw new ArgumentException(
                        message:    $"The parse result code {(int)error:X8} is " +
                                     "not recognised.",
                        paramName:  nameof(error));
                }

                if (error == ParseResultType.Success)
                {
                    throw new ArgumentException(
                        message:    "A failure-state SV.ParseResult cannot be " +
                                    "created with the [Success] status.",
                        paramName:  nameof(error));
                }

                this.Type = error;
                this.Version = null;
            }

            /// <summary>
            /// <para>
            /// The result code describing the parse result.
            /// </para>
            /// </summary>
            public ParseResultType Type
            {
                get;
            }
            /// <summary>
            /// <para>
            /// The produced <see cref="SemanticVersion"/>, if
            /// the parsing was successful.
            /// </para>
            /// </summary>
            public SemanticVersion Version
            {
                get;
            }

            /// <summary>
            /// <para>
            /// Creates the appropriate exception for the current
            /// instance's <see cref="Type"/> value.
            /// </para>
            /// </summary>
            /// <returns>
            /// An <see cref="Exception"/> appropriate for the value
            /// of the current instance's <see cref="Type"/> field.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="Type"/> has the value
            /// <see cref="ParseResultType.Success"/>.
            /// </exception>
            public Exception CreateException()
            {
                // REMEMBER: If changing the exceptions here, update the
                //           documentation for the [Parse] method(s).

                if (this.Type == ParseResultType.Success)
                {
                    throw new InvalidOperationException(
                        "Cannot create exception: operation was successful.");
                }

                var msg = this.GetErrorMessage();

                switch (this.Type)
                {
                    // [ArgumentNullException] thrown when the version string
                    // is empty.
                    case ParseResultType.NullString:
                    return new ArgumentNullException(message: msg, 
                                                     innerException: null);

                    // [ArgumentException] thrown when an expected component is
                    // not present.
                    case ParseResultType.TrioItemMissing:
                    case ParseResultType.IdentifierMissing:
                    case ParseResultType.MetadataMissing:
                    return new ArgumentException(message: msg);

                    // [FormatException] thrown when an invalid character is
                    // encountered.
                    case ParseResultType.PreTrioInvalidChar:
                    case ParseResultType.TrioInvalidChar:
                    case ParseResultType.TrioItemLeadingZero:
                    case ParseResultType.IdentifierInvalid:
                    case ParseResultType.MetadataInvalid:
                    return new FormatException(message: msg);

                    // [OverflowException] thrown when an integer overflow is
                    // encountered.
                    case ParseResultType.TrioItemOverflow:
                    return new OverflowException(message: msg);

                    default:
                    {
                        // Not using [ArgumentException] here so any time this
                        // is thrown (which should be never) won't accidentally
                        // be caught by user code catching [ArgumentException].
                        throw new NotSupportedException(
                            $"Unrecognised result code {(int)Type:X8} provided.");
                    }
                }
            }
            /// <summary>
            /// <para>
            /// Retrieves a human-friendly error message describing the
            /// value of <see cref="Type"/>.
            /// </para>
            /// </summary>
            /// <returns>
            /// A human-friendly error message describing the value of
            /// <see cref="Type"/>.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="Type"/> has the value
            /// <see cref="ParseResultType.Success"/>.
            /// </exception>
            public string GetErrorMessage()
            {
                switch (this.Type)
                {
                    case ParseResultType.NullString:
                    return "The provided version string was null or empty.";

                    case ParseResultType.PreTrioInvalidChar:
                    case ParseResultType.TrioInvalidChar:
                    return "The version string contained an invalid character.";

                    case ParseResultType.TrioItemLeadingZero:
                    return "The major, minor, and patch versions may not have " +
                           "leading zeroes.";

                    case ParseResultType.TrioItemMissing:
                    return "The version string was missing a version component.";

                    case ParseResultType.TrioItemOverflow:
                    return "One or more of the major, minor, or patch " +
                           "versions represented a number greater than the " +
                           "supported maximum.";

                    case ParseResultType.IdentifierMissing:
                    return "A pre-release identifier was not found where one " +
                           "was expected.";

                    case ParseResultType.IdentifierInvalid:
                    return "One or more pre-release identifiers were invalid.";

                    case ParseResultType.MetadataMissing:
                    return "A build metadata item was not found where one " +
                           "was expected";

                    case ParseResultType.MetadataInvalid:
                    return "One or more build metadata items were invalid.";

                    case ParseResultType.Success:
                    {
                        throw new InvalidOperationException(
                            "Cannot retrieve message: operation was successful.");
                    };
                    default:
                    {
                        throw new NotSupportedException(
                            $"Unrecognised result code {(int)Type:X8} provided.");
                    };
                }
            }
        }

        /// <summary>
        /// <para>
        /// Provides information about the semantic version that was parsed.
        /// </para>
        /// </summary>
        [Serializable]
        internal class ParseMetadata
        {
            public static ParseMetadata Default { get; } = new ParseMetadata(
                minor: ComponentState.Present,
                patch: ComponentState.Present
                );

            public ParseMetadata(ComponentState minor, ComponentState patch)
            {
                if (minor == ComponentState.Omitted && patch != ComponentState.Omitted)
                    throw new ArgumentException(
                        "A patch component cannot be specified as present if " +
                        "a minor component is not also present."
                        );

                this.MinorState = minor;
                this.PatchState = patch;
            }

            /// <summary>
            /// <para>
            /// The state of the <see cref="SemanticVersion.Minor"/> component
            /// in the parsed version string.
            /// </para>
            /// </summary>
            public ComponentState MinorState
            {
                get;
            }
            /// <summary>
            /// <para>
            /// The state of the <see cref="SemanticVersion.Patch"/> component
            /// in the parsed version string.
            /// </para>
            /// </summary>
            public ComponentState PatchState
            {
                get;
            }

            /// <summary>
            /// <para>
            /// Determines whether the instance contains only default values.
            /// </para>
            /// </summary>
            /// <returns></returns>
            public bool IsDefault()
            {
                return this.MinorState == Default.MinorState &&
                       this.PatchState == Default.PatchState
                       ;
            }
        }

        /// <summary>
        /// <para>
        /// Encapsulates the parser for the <see cref="SemanticVersion"/> class.
        /// </para>
        /// </summary>
        internal static class Parser
        {
            /// <summary>
            /// <para>
            /// The character used to start the sequence of pre-release
            /// identifiers (a hyphen).
            /// </para>
            /// </summary>
            private const char IdentifierStart = '-';
            /// <summary>
            /// <para>
            /// The character used to start the sequence of build metadata
            /// items (a plus sign).
            /// </para>
            /// </summary>
            private const char MetadataStart = '+';
            /// <summary>
            /// <para>
            /// The character used to separate various version
            /// compoents (a period).
            /// </para>
            /// </summary>
            private const char ComponentSeparator = '.';

            private static readonly object _lockbox = new object();

            /// <summary>
            /// <para>
            /// Attempts to normalise the version string provided as input
            /// to the parser.
            /// </para>
            /// </summary>
            /// <param name="input">
            /// The version string to normalise.
            /// </param>
            /// <param name="mode">
            /// Modes augmenting how parsing is performed.
            /// </param>
            /// <returns>
            /// A <see cref="ParseResult"/> indicating whether normalisation
            /// was successful or, if it wasn't successful, why it failed.
            /// </returns>
            internal static ParseResultType Normalise(ref string input, 
                                                      ParseMode mode)
            {
                // Normalisation should make our attempt at caching the result
                // of the parser (and so speeding up programs that use a lot of
                // parsed semantic versions) more effective.
                //
                // Once normalised, strings like ["v1.2.0"] and ["1.2.0"] would
                // appear identical, and leading and trailing whitespace is not
                // preserved.
                //
                // Normalisation is required for this because we're going to be
                // feeding the string representation into an [IDictionary<>] to
                // make a map of string representations to class instances.

                // We can't normalise a string with no content, so if it's
                // null, empty, or whitespace, indicate that it is null.
                if (String.IsNullOrWhiteSpace(input))
                    return ParseResultType.NullString;

                // Removing whitespace is the first step of normalisation. Other
                // [Parse] methods (such as on [Int32]) do it, so to maintain the
                // Principle of Least Astonishment we're doing it too.
                input = input.Trim();

                // Next, we're going to remove a prefixing "v" or "V" if it is
                // present and the flags we've been passed ([mode]) allow us to
                // do so.
                //
                // We should be safe to use the indexer because we've already
                // verified that the string is not entirely whitespace, so 
                // trimming should give us at least a single character.
                if (!Helper.IsNumber(input[0]))
                {
                    // If the first character isn't a number and we haven't
                    // been passed the [AllowPrefix] flag, there's no point in
                    // checking what character it is because it's definitely
                    // invalid.
                    //
                    // However, if we have been passed the [AllowPrefix] flag
                    // and the character is neither "v" nor "V", we have to
                    // return the same result because it's invalid.
                    if (!mode.HasFlag(ParseMode.AllowPrefix))
                        return ParseResultType.PreTrioInvalidChar;
                    else if (input[0] != 'v' && input[0] != 'V')
                        return ParseResultType.PreTrioInvalidChar;

                    // If we end up here, we know that we've been passed the
                    // [AllowPrefix] flag and that the first character is a
                    // "v" or "V". We don't want to pass the first character
                    // to the parser (because it isn't meaningful), so we need
                    // to substring the string.
                    input = input.Substring(startIndex: 1);
                }

                // If we haven't returned yet, it means everything should be
                // good and we can indicate success to the caller.
                return ParseResultType.Success;
            }

            /// <summary>
            /// <para>
            /// Implements <see cref="SemanticVersion"/> parsing.
            /// </para>
            /// </summary>
            /// <param name="versionString">
            /// The string representing the semantic version.
            /// </param>
            /// <param name="mode">
            /// Modes augmenting how parsing is performed.
            /// </param>
            /// <returns>
            /// A <see cref="ParseResult"/> describing whether the
            /// parse succeeded.
            /// </returns>
            private static ParseResult _parseVersion(string versionString, 
                                                     ParseMode mode)
            {
                // This could all probably be accomplished with a single
                // (probably ugly) regular expression, but we probably
                // wouldn't be able to provide as-specific error messages
                // if we did that.
                //
                // I'll take slower parsing for better error reporting
                // any day of the week.

                var chars = versionString.GetEnumerator();
                var builder = new StringBuilder();
                char? input = null;

                // Negative values will cause an exception if we pass them
                // to the [SemanticVersion] constructor.
                int major = -1, minor = -1, patch = -1;
                // Null values will too, as well as causing an NRE if we try
                // to use them before we've given them a proper value.
                ICollection<string> identifiers = new List<string>(),
                                    metadata = new List<string>();

                // Values to populate a [ParseMetadata] instance
                var minorState = ComponentState.Present;
                var patchState = ComponentState.Present;


                // Whether the patch version component can be omitted
                var optionalPatch = mode.HasFlag(ParseMode.OptionalPatch);
                // Whether the minor (and, consequently, the patch) version
                // component can be omitted
                var optionalMinor = optionalPatch &&
                                    InternalModes.Has(mode, InternalModes.OptionalMinor);

                // Get the party started
                return Parse();


                // Consumes a character from the input.
                char? Consume()
                {
                    if (chars.MoveNext())
                    {
                        input = chars.Current;
                    }
                    else
                    {
                        input = null;
                    }

                    return input;
                }

                ParseResult Parse()
                {
                    // Move the enumerator to the first character
                    Consume();

                    // The [ParseMajor] function calls all subsequent functions
                    // and relays back their results.
                    var res = ParseMajor();

                    // If they indicated success...
                    if (res == ParseResultType.Success)
                    {
                        // Return the version we parsed and metadata about its
                        // being parsed
                        return new ParseResult(new SemanticVersion(
                            major:       major,
                            minor:       minor,
                            patch:       patch,
                            identifiers: identifiers,
                            metadata:    metadata,
                            parseInfo:   new ParseMetadata(
                                minor:      minorState,
                                patch:      patchState
                                )
                            ));
                    }
                    // If they didn't indicate success, relay their error up.
                    else
                    {
                        return new ParseResult(res);
                    }
                }
                // Attempts to parse a major version component
                ParseResultType ParseMajor()
                {
                    var result = ParseComponent(ref major);

                    // If we managed to obtain a component value, we now need
                    // to carry out major version-specific error checking
                    if (result == ParseResultType.Success)
                    {
                        // If we've encountered a component separator, we expect
                        // that the minor version component will come next.
                        if (input == ComponentSeparator)
                        {
                            // Move past the separator.
                            Consume();

                            // Parse the minor version and subsequent parts
                            return ParseMinor();
                        }
                        // A major version must normally be followed by a minor
                        // version, but we can be configured to allow the omission
                        // of a minor version.
                        //
                        // In that case, encountering the end of the string or
                        // the start of the pre-release identifiers or metadata
                        // is not an error.
                        else if (!input.HasValue || input == IdentifierStart ||
                                 input == MetadataStart)
                        {
                            // If we haven't been so configured, error.
                            if (!optionalMinor)
                            {
                                return ParseResultType.TrioItemMissing;
                            }
                            else
                            {
                                // If the minor version is omitted, reset it and
                                // the patch version to zero and indicate that
                                // they were omitted.
                                minor = 0;
                                patch = 0;
                                minorState = ComponentState.Omitted;
                                patchState = ComponentState.Omitted;

                                // Then our next action depends on which of the
                                // characters we encountered.
                                //
                                // The end of the string means we've parsed all
                                // that we need to.
                                if (!input.HasValue)
                                {
                                    return ParseResultType.Success;
                                }
                                // The start of identifiers means we want to try
                                // and parse identifiers.
                                else if (input == IdentifierStart)
                                {
                                    // Move past the separator
                                    Consume();

                                    return ParseIdentifiers();
                                }
                                // And anything else means metadata.
                                else
                                {
                                    // Move past the separator
                                    Consume();

                                    return ParseMetadata();
                                }
                            }
                        }
                        // Anything else is a character we don't expect.
                        else
                        {
                            return ParseResultType.TrioInvalidChar;
                        }
                    }
                    // If parsing the component wasn't successful, let the error
                    // bubble up to our caller
                    else
                    {
                        return result;
                    }
                }
                // Attempts to parse a minor version component
                ParseResultType ParseMinor()
                {
                    var result = ParseComponent(ref minor);

                    // This part is somewhat similar to [ParseMajor], except
                    // that we're dealing with the patch version being omitted
                    // rather than the minor version.

                    if (result == ParseResultType.Success)
                    {
                        // A component separator means the patch version is
                        // present, so we can proceed to parse it.
                        if (input == ComponentSeparator)
                        {
                            // Move past the separator.
                            Consume();

                            return ParsePatch();
                        }
                        // Similarly to the major version, a minor version must
                        // normally be followed by a patch version but we can be
                        // configured to accept its omission.
                        //
                        // If we are so configured, encountering the end of the
                        // string or the start of the identifiers or metadata is
                        // not an error.
                        else if (!input.HasValue || input == IdentifierStart ||
                                 input == MetadataStart)
                        {
                            // If we haven't been so configured, error.
                            if (!optionalPatch)
                            {
                                return ParseResultType.TrioItemMissing;
                            }
                            else
                            {
                                // If the patch version is omitted, reset it to
                                // zero and store its state.
                                patch = 0;
                                patchState = ComponentState.Omitted;

                                // As with the major version, what we do next
                                // depends on which of the three characters we
                                // encountered.
                                //
                                // We can validly finish parsing when we reach
                                // the end of the input string.
                                if (!input.HasValue)
                                {
                                    return ParseResultType.Success;
                                }
                                // Or move on to parsing pre-release identifiers.
                                else if (input == IdentifierStart)
                                {
                                    // Move past the separator
                                    Consume();

                                    return ParseIdentifiers();
                                }
                                // Or move to parsing metadata.
                                else
                                {
                                    // Move past the separator
                                    Consume();

                                    return ParseMetadata();
                                }
                            }
                        }
                        // We don't recognise any other characters.
                        else
                        {
                            return ParseResultType.TrioInvalidChar;
                        }
                    }
                    // If we couldn't successfully parse the component, then
                    // we want to bubble the error up.
                    else
                    {
                        return result;
                    }
                }
                // Attempts to parse a patch version component.
                ParseResultType ParsePatch()
                {
                    var result = ParseComponent(ref patch);

                    if (result == ParseResultType.Success)
                    {
                        // If we've successfully parsed a patch version, we can
                        // finish parsing here under all circumstances. However,
                        // it is possible for pre-release identifiers or
                        // metadata to follow.
                        //
                        // If we reach the end of the string, we're all done.
                        if (!input.HasValue)
                        {
                            return ParseResultType.Success;
                        }
                        // If we encounter pre-release identifiers, parse them.
                        else if (input == IdentifierStart)
                        {
                            // Move past separator
                            Consume();

                            return ParseIdentifiers();
                        }
                        // Or metadata.
                        else if (input == MetadataStart)
                        {
                            // Move past separator
                            Consume();

                            return ParseMetadata();
                        }
                        // Anything else means we've encountered an invalid
                        // character, which is an error.
                        else
                        {
                            return ParseResultType.TrioInvalidChar;
                        }
                    }
                    // If parsing was unsuccessful, let the result bubble up.
                    else
                    {
                        return result;
                    }
                }
                // Provides default parsing for version components
                ParseResultType ParseComponent(ref int comp)
                {
                    // A version component can only contain numbers, so we want
                    // to accumulate all numeric characters we can.
                    while (input.HasValue && Helper.IsNumber(input.Value))
                    {
                        builder.Append(input.Value);

                        Consume();
                    }

                    // Our caller will deal with the first non-numeric character,
                    // so all we need to do is try to turn what we have into an
                    // actual value.
                    //
                    // If what caused us to stop accumulating isn't the end of
                    // the string or a separator, it's an invalid character.
                    if (input.HasValue && input != ComponentSeparator &&
                        input != IdentifierStart && input != MetadataStart)
                    {
                        return ParseResultType.TrioInvalidChar;
                    }

                    

                    // If we didn't accumulate anything, we don't have a version
                    // component, which is an error. We don't consider whether it
                    // can be omitted, and leave it to the caller to only call
                    // us if it expects a component to be present.
                    if (builder.Length == 0)
                        return ParseResultType.TrioItemMissing;

                    // Version components can't have leading zeroes.
                    if (builder.Length > 1 && builder[0] == '0')
                        return ParseResultType.TrioItemLeadingZero;

                    // We already know that we only have numeric characters, so
                    // the only reason parsing can fail is if they represent a
                    // number too great to be stored in an [Int32].
                    if (!Int32.TryParse(builder.ToString(), out var parseComp))
                        return ParseResultType.TrioItemOverflow;

                    // If all these tests pass, we have a valid result. Store it
                    // and clear the accumulator.
                    comp = parseComp;
                    builder.Clear();

                    return ParseResultType.Success;
                }

                // Attempts to parse a set of pre-release identifiers
                ParseResultType ParseIdentifiers()
                {
                    // Our caller consumed the separating hyphen, so we're free
                    // to assume that we're starting on the first character of
                    // what should be a pre-release identifier.
                    //
                    // As we're calling this method recursively, the same is
                    // true of any separating periods.

                    // Accumulate until we reach the end of input or a separator.
                    while (input.HasValue && input != ComponentSeparator &&
                           input != MetadataStart)
                    {
                        builder.Append(input.Value);

                        Consume();
                    }

                    // If we didn't accumulate anything, then the metadata item
                    // is missing. Error.
                    if (builder.Length == 0)
                        return ParseResultType.IdentifierMissing;

                    // If what we did accumulate isn't a valid identifier, error.
                    if (!Helper.IsValidIdentifier(builder.ToString()))
                        return ParseResultType.IdentifierInvalid;

                    // If we get here, we know we have a valid identifier. Store
                    // it and clear stored state.
                    identifiers.Add(builder.ToString());
                    builder.Clear();

                    // As before, what we do next depends on what character we
                    // encountered to end accumulation.
                    //
                    // If it's the end of the string, we've successfully parsed.
                    if (!input.HasValue)
                    {
                        return ParseResultType.Success;
                    }
                    // If it's a component separator, there are more identifiers
                    // that we need to parse.
                    else if (input == ComponentSeparator)
                    {
                        // Move past separator
                        Consume();

                        return ParseIdentifiers();
                    }
                    // And if it's the start of metadata, parse those.
                    else
                    {
                        // Move past separator
                        Consume();

                        return ParseMetadata();
                    }
                }
                // Attempts to parse a set of metadata items
                ParseResultType ParseMetadata()
                {
                    // Parsing metadata is largely the same as parsing identifiers,
                    // although the formats for each component are slightly different.
                    //
                    // As with [ParseIdentifiers], our caller will move us past
                    // any separating plus sign or full stop, so we can assume
                    // that we're on what should be the first character of an
                    // identifier.

                    // As with identifiers, accumulate until we reach the end
                    // of the string or a separator.
                    while (input.HasValue && input != ComponentSeparator)
                    {
                        builder.Append(input.Value);

                        Consume();
                    }

                    // Accumulating nothing means the metadata item is missing,
                    // which isn't valid.
                    if (builder.Length == 0)
                        return ParseResultType.MetadataMissing;

                    // If we have accumulated something, it has to be valid.
                    if (!Helper.IsValidMetadata(builder.ToString()))
                        return ParseResultType.MetadataInvalid;

                    // If it is valid, store it and clear stored state.
                    metadata.Add(builder.ToString());
                    builder.Clear();

                    // And, as with identifiers, determine what we do next
                    // based on what caused us to stop accumulating.
                    //
                    // The end of the string means our job is done.
                    if (!input.HasValue)
                    {
                        return ParseResultType.Success;
                    }
                    // Whereas a component separator means we expect another
                    // metadata item to follow.
                    else
                    {
                        // Move past separator
                        Consume();

                        return ParseMetadata();
                    }
                }
            }

            /// <summary>
            /// <para>
            /// Implements <see cref="SemanticVersion"/> parsing and
            /// input preprocessing.
            /// </para>
            /// </summary>
            /// <param name="input">
            /// The string representing the semantic version.
            /// </param>
            /// <param name="mode">
            /// Modes augmenting how parsing is performed.
            /// </param>
            /// <returns>
            /// A <see cref="ParseResult"/> describing whether the
            /// parse succeeded.
            /// </returns>
            public static ParseResult Parse(string input, ParseMode mode)
            {
                // First thing we do is normalise the string. This gives us
                // the string in a format we can understand properly.
                var normaliseResult = Parser.Normalise(ref input, mode);

                // If normalisation was not successful, then we pass on the
                // error code it returned to the caller.
                if (normaliseResult != ParseResultType.Success)
                    return new ParseResult(normaliseResult);

                SemanticVersion cacheResult = null;
                ParseResult result;

                // We don't know what the memoization agent actually is, and
                // whether a user intends on accessing it. By locking on the
                // property, it is possible for the parser and the user (who 
                // may be doing anything with their cache) to safely access
                // the cache.
                //
                // However, [MemoizationAgent] won't always have a value, so
                // we have a private object to lock on when this is the case.

                var lockObj = MemoizationAgent ?? _lockbox;
                lock (lockObj)
                {
                    // Whoever's using the library may or may not have configured
                    // a memoization agent for us. If they have, we're going to
                    // see if the input string is in the cache.
                    //
                    // If they haven't configured an agent, we'll go straight to
                    // parsing. We'll also do this if the input isn't in the
                    // cache.
                    if (MemoizationAgent?.TryGetValue(input, out cacheResult) != true)
                    {
                        // There is no cache, or the item isn't in the cache.
                        // This means we have to try and parse the input.
                        result = _parseVersion(input, mode);

                        // If parsing was successful, and if there is a cache
                        // configured, add the result to the cache.
                        //
                        // We don't want to store if any internal modes are
                        // passed, as these might leak out to an external caller
                        // who won't be expecting them or able to handle them.
                        if (result.Type == ParseResultType.Success &&
                            !InternalModes.HasAny(mode))
                            MemoizationAgent?.Add(input, result.Version);
                    }
                    // The item was in our cache. Our result is what we've
                    // retrieved from our cache.
                    else
                    {
                        // We know that 
                        result = new ParseResult(cacheResult);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// <para>
        /// Converts a version string to a <see cref="SemanticVersion"/>,
        /// taking into account a set of flags.
        /// </para>
        /// </summary>
        /// <param name="version">
        /// The version string to be converted to a <see cref="SemanticVersion"/>.
        /// </param>
        /// <param name="mode">
        /// A set of flags that augment how the version string is parsed.
        /// </param>
        /// <returns>
        /// A <see cref="SemanticVersion"/> equivalent to the provided version
        /// string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="version"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a component in the version string was expected but not
        /// found (for example, a missing minor or patch version).
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown when an invalid character or character sequence is encountered.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when an attempt to convert the major, minor, or patch version
        /// into an <see cref="int"/> resulted in an overflow.
        /// </exception>
        public static SemanticVersion Parse(string version, ParseMode mode)
        {
            var result = Parser.Parse(version, mode);

            // If the parsing was successful, return the created version.
            if (result.Type == ParseResultType.Success)
                return result.Version;

            // If it wasn't, create and throw the appropriate exception.
            throw result.CreateException();
        }
        /// <summary>
        /// <para>
        /// Converts a version string to a <see cref="SemanticVersion"/>, only
        /// accepting the format given in the Semantic Versioning specification.
        /// </para>
        /// </summary>
        /// <param name="version">
        /// The version string to be converted to a <see cref="SemanticVersion"/>.
        /// </param>
        /// <returns>
        /// A <see cref="SemanticVersion"/> equivalent to the provided version
        /// string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="version"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when a component in the version string was expected but not
        /// found (for example, a missing minor or patch version).
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown when an invalid character or character sequence is encountered.
        /// </exception>
        /// <exception cref="OverflowException">
        /// Thrown when an attempt to convert the major, minor, or patch version
        /// into an <see cref="int"/> resulted in an overflow.
        /// </exception>
        /// <remarks>
        /// This method is equivalent to calling 
        /// <see cref="Parse(string, ParseMode)"/> and passing the value
        /// <see cref="ParseMode.Strict"/>.
        /// </remarks>
        public static SemanticVersion Parse(string version)
        {
            return SemanticVersion.Parse(version, ParseMode.Strict);
        }

        /// <summary>
        /// <para>
        /// Attempts to convert a version string to a 
        /// <see cref="SemanticVersion"/>, taking into account a set of flags.
        /// </para>
        /// </summary>
        /// <param name="version">
        /// The version string to be converted to a <see cref="SemanticVersion"/>.
        /// </param>
        /// <param name="mode">
        /// A set of flags that augment how the version string is parsed.
        /// </param>
        /// <param name="semver">
        /// When the method returns, this parameter is either set to the created
        /// <see cref="SemanticVersion"/> (if parsing was sucessful), or is given
        /// an undefined value (if parsing was unsuccessful).
        /// </param>
        /// <returns>
        /// True if parsing succeeded, false if otherwise.
        /// </returns>
        public static bool TryParse(string version, ParseMode mode,
                                    out SemanticVersion semver)
        {
            var result = Parser.Parse(version, mode);
            
            // We don't need to perform any checks here. Either we had
            // a success and [Version] has a value, or we didn't and it's
            // null.
            semver = result.Version;

            // Only [ParseResultType.Success] indicates a successful parsing
            // and the producing of a [SemanticVersion] instance.
            return result.Type == ParseResultType.Success;
        }
        /// <summary>
        /// <para>
        /// Attempts to convert a version string to a 
        /// <see cref="SemanticVersion"/>.
        /// </para>
        /// </summary>
        /// <param name="version">
        /// The version string to be converted to a <see cref="SemanticVersion"/>.
        /// </param>
        /// <param name="semver">
        /// When the method returns, this parameter is either set to the created
        /// <see cref="SemanticVersion"/> (if parsing was sucessful), or is given
        /// an undefined value (if parsing was unsuccessful).
        /// </param>
        /// <returns>
        /// True if parsing succeeded, false if otherwise.
        /// </returns>
        /// <remarks>
        /// This method is equivalent to calling
        /// <see cref="TryParse(string, ParseMode, out SemanticVersion)"/> and
        /// passing the value <see cref="ParseMode.Strict"/>.
        /// </remarks>
        public static bool TryParse(string version, out SemanticVersion semver)
        {
            return SemanticVersion.TryParse(version, ParseMode.Strict, out semver);
        }


        /// <summary>
        /// <para>
        /// The cache to use to memoize the results of the 
        /// <see cref="SemanticVersion"/> parsing methods.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Assign <c>null</c> to disable memoization. The value of this
        /// property is <c>null</c> by default.
        /// </para>
        /// <para>
        /// Accesses by the parser to the memoization agent are surrounded
        /// by <c>lock (MemoizationAgent)</c>.
        /// </para>
        /// </remarks>
        public static IDictionary<string, SemanticVersion> MemoizationAgent
        {
            get;
            set;
        }
    }
}
