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
    public enum ParseMode
    {
        /// <summary>
        /// <para>
        /// The default parser behaviour, with no set flags. This forces
        /// specification compliance.
        /// </para>
        /// </summary>
        Strict          = 0x0000,

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
        /// Represents the result produced by the <see cref="SemanticVersion"/>
        /// parser.
        /// </para>
        /// </summary>
        internal struct ParseResult
        {
            // This will be false if we're default-constructed.
            private readonly bool _successfulCreation;
            // Backing fields for properties
            private readonly ParseResultType _type;
            private readonly SemanticVersion _version;

            /// <summary>
            /// <para>
            /// Checks the <see cref="_successfulCreation"/> field,
            /// and throws if it is false.
            /// </para>
            /// </summary>
            /// <typeparam name="T">
            /// The type of the value to be returned on success.
            /// </typeparam>
            /// <param name="passthrough">
            /// A value to be returned from the function if the
            /// <see cref="ParseResult"/> was successfully constructed.
            /// </param>
            /// <returns>
            /// If <see cref="_successfulCreation"/> is true, returns
            /// <paramref name="passthrough"/>.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="_successfulCreation"/> is false.
            /// </exception>
            private T VerifyResult<T>(T passthrough)
            {
                if (_successfulCreation)
                    return passthrough;

                throw new InvalidOperationException(
                    "Attempt to use a [ParseResult] that was default-" +
                    "constructed.");
            }

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

                _successfulCreation = true;
                _type = ParseResultType.Success;
                _version = version;
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
                        message:    $"The parse result code 0x{error:X2} is " +
                                     "not recognised.",
                        paramName:  nameof(error));
                }

                if (error == ParseResultType.Success)
                {
                    throw new ArgumentException(
                        message:    "A failure-state ParseResult cannot be " +
                                    "created with the [Success] status.",
                        paramName:  nameof(error));
                }

                _type = error;
                _version = null;
                _successfulCreation = true;
            }

            /// <summary>
            /// <para>
            /// The result code describing the parse result.
            /// </para>
            /// </summary>
            public ParseResultType Type => VerifyResult(_type);
            /// <summary>
            /// <para>
            /// The produced <see cref="SemanticVersion"/>, if
            /// the parsing was successful.
            /// </para>
            /// </summary>
            public SemanticVersion Version => VerifyResult(_version);

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
                            $"Unrecognised result code 0x{Type:X2} provided.");
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
                            $"Unrecognised result code 0x{Type:X2} provided.");
                    };
                }
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
            /// Implements <see cref="SemanticVersion"/> parsing.
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
                throw new NotImplementedException();
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
        public static bool TryParse(string version, out SemanticVersion semver)
        {
            return SemanticVersion.TryParse(version, out semver);
        }
    }
}
