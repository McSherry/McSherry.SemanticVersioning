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

using static McSherry.SemanticVersioning.Ranges.VersionRange.ParseResultType;

namespace McSherry.SemanticVersioning.Ranges
{
    using ResultSet = IEnumerable<IEnumerable<VersionRange.ComparatorToken>>;

    // Documentation and attributes are in 'Ranges\VersionRange.cs'
    public sealed partial class VersionRange
    {
        /// <summary>
        /// <para>
        /// Represents the operators that the <see cref="VersionRange"/>
        /// parser recognises.
        /// </para>
        /// </summary>
        internal enum Operator
        {
            /// <summary>
            /// <para>
            /// Used to check version equality. Assumed when no operator
            /// is specified.
            /// </para>
            /// </summary>
            Equal,
            /// <summary>
            /// <para>
            /// Used in checking that a <see cref="SemanticVersion"/> has
            /// lesser precedence than the specified version.
            /// </para>
            /// </summary>
            LessThan,
            /// <summary>
            /// <para>
            /// Used in checking that a <see cref="SemanticVersion"/> has
            /// greater precedence than the specified version.
            /// </para>
            /// </summary>
            GreaterThan,
            /// <summary>
            /// <para>
            /// Used in checking that a <see cref="SemanticVersion"/> is
            /// equal or has lesser precedence than the specified version.
            /// </para>
            /// </summary>
            LessThanOrEqual,
            /// <summary>
            /// <para>
            /// Used in checking that a <see cref="SemanticVersion"/> is
            /// equal or has greater precedence than the specified version.
            /// </para>
            /// </summary>
            GreaterThanOrEqual,
        }

        /// <summary>
        /// <para>
        /// Represents an <see cref="Operator"/> and <see cref="SemanticVersion"/>
        /// grouping identified by the parser.
        /// </para>
        /// </summary>
        internal struct ComparatorToken
        {
            private readonly Operator _op;
            private readonly SemanticVersion _sv;
            private readonly bool _valid;

            /// <summary>
            /// <para>
            /// Ensures that the the current instance has been
            /// correctly constructed and is valid.
            /// </para>
            /// </summary>
            /// <typeparam name="T">
            /// The type of the value to pass through on valid
            /// construction.
            /// </typeparam>
            /// <param name="passthrough">
            /// A value to be passed through if the current
            /// instance is valid.
            /// </param>
            /// <returns>
            /// If the current instance is valid, returns the
            /// value given in <paramref name="passthrough"/>.
            /// Otherwise, throws an exception.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when the current instance is not valid.
            /// </exception>
            private T Validate<T>(T passthrough)
            {
                if (_valid)
                    return passthrough;

                throw new InvalidOperationException(
                    "An attempt was made to use a Comparator that was " +
                    "not correctly constructed."
                    );
            }

            /// <summary>
            /// <para>
            /// Creates a new <see cref="ComparatorToken"/> instance with
            /// the specified operator and semantic version.
            /// </para>
            /// </summary>
            /// <param name="op">
            /// The <see cref="VersionRange.Operator"/> to use.
            /// </param>
            /// <param name="semver">
            /// The <see cref="SemanticVersion"/> to use.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="semver"/> is null.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="op"/> is not a recognised
            /// <see cref="VersionRange.Operator"/>.
            /// </exception>
            public ComparatorToken(Operator op, SemanticVersion semver)
            {
                if (semver == null)
                {
                    throw new ArgumentNullException(
                        message:    "The specified version cannot be null.",
                        paramName:  nameof(semver)
                        );
                }

                if (!Enum.IsDefined(typeof(Operator), op))
                {
                    throw new ArgumentException(
                        message:    "The specified operator is not valid.",
                        paramName:  nameof(op)
                        );
                }

                _op = op;
                _sv = semver;
                _valid = true;
            }

            /// <summary>
            /// <para>
            /// The operator used during comparison.
            /// </para>
            /// </summary>
            public Operator Operator => Validate(_op);
            /// <summary>
            /// <para>
            /// The semantic version that is compared against.
            /// </para>
            /// </summary>
            public SemanticVersion Version => Validate(_sv);
        }

        /// <summary>
        /// <para>
        /// Represents the various possible return statuses of the
        /// <see cref="VersionRange"/> parser.
        /// </para>
        /// </summary>
        internal enum ParseResultType
        {
            /// <summary>
            /// <para>
            /// Parsing completed without issue.
            /// </para>
            /// </summary>
            Success,

            /// <summary>
            /// <para>
            /// The version range string is null, empty, or contains
            /// only whitespace characters.
            /// </para>
            /// </summary>
            NullString,
            /// <summary>
            /// <para>
            /// There are one or more invalid characters in the
            /// version range string.
            /// </para>
            /// </summary>
            InvalidCharacter,

            /// <summary>
            /// <para>
            /// One or more comparator sets contain no comparators.
            /// </para>
            /// </summary>
            EmptySet,
            /// <summary>
            /// <para>
            /// An operator is present with no attached version.
            /// </para>
            /// </summary>
            OrphanedOperator,
            /// <summary>
            /// <para>
            /// The version range string contains an invalid
            /// semantic version string.
            /// </para>
            /// </summary>
            InvalidVersion,
        }

        /// <summary>
        /// <para>
        /// Represents the result produced by the <see cref="VersionRange"/>
        /// parser.
        /// </para>
        /// </summary>
        internal struct ParseResult
        {
            // Certain codes are required to be provided with an exception. This
            // hashset will contain those specific codes so we can check for them.
            private static readonly HashSet<ParseResultType> _exCodes;

            static ParseResult()
            {
                _exCodes = new HashSet<ParseResultType>
                {
                    // [InvalidVersion] needs an exception so that the exact
                    // reason for the version string being invalid can be relayed.
                    InvalidVersion,
                };
            }

            // This will be false if we're default-constructed.
            private readonly bool _successfulCreation;
            // Property backing fields.
            private readonly ParseResultType _type;
            private readonly ResultSet _results;
            private readonly Exception _innerEx;

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
                    "Attempt to use a [VersionRange.ParseResult] that was " +
                    "default-constructed.");
            }

            /// <summary>
            /// <para>
            /// Creates a parse result indicating success and
            /// containing the specified results.
            /// </para>
            /// </summary>
            /// <param name="results">
            /// The results for the <see cref="ParseResult"/>
            /// to contain.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="results"/> or
            /// any of its items are null.
            /// </exception>
            public ParseResult(ResultSet results)
            {
                if (results?.Any(cs => cs == null) != false)
                {
                    throw new ArgumentNullException(
                        paramName:  nameof(results),
                        message:    "The result set cannot be null, and " +
                                    "cannot contain null items."
                        );
                }

                _type = ParseResultType.Success;
                _results = results;
                _innerEx = null;
                _successfulCreation = true;
            }
            /// <summary>
            /// <para>
            /// Creates a parse result indicating failure and
            /// with the specified result code.
            /// </para>
            /// </summary>
            /// <param name="error">
            /// The result code for the <see cref="ParseResult"/>
            /// to report.
            /// </param>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="error"/> is invalid
            /// or unrecognised.
            /// </exception>
            public ParseResult(ParseResultType error)
            {
                if (error == Success)
                {
                    throw new ArgumentException(
                        message:    "A failure-state [VR.ParseResult] cannot " +
                                    "be created with the [Success] status.",
                        paramName:  nameof(error));
                }

                if (!Enum.IsDefined(typeof(ParseResultType), error))
                {
                    throw new ArgumentException(
                        message:    $"The parse result code {(int)error:X8} is " +
                                     "not recognised.",
                        paramName:  nameof(error));
                }

                // Some status codes must be provided with an exception. We
                // need to check for these in this constructor, as this ctor
                // doesn't take an exception argument.
                if (_exCodes.Contains(error))
                {
                    throw new ArgumentException(
                        message:    $"The parse result code {(int)error:X8} is " +
                                     "required to be passed with an exception.",
                        paramName:  nameof(error));
                }

                _type = error;
                _results = null;
                _innerEx = null;
                _successfulCreation = true;
            }
            /// <summary>
            /// <para>
            /// Creates a parse result indicating failure and with
            /// the specified error code and inner exception.
            /// </para>
            /// </summary>
            /// <param name="error">
            /// The result code for the <see cref="ParseResult"/>
            /// to report.
            /// </param>
            /// <param name="innerException">
            /// The inner exception to store in the parse result.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="innerException"/> is null.
            /// </exception>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="error"/> is invalid
            /// or unrecognised.
            /// </exception>
            public ParseResult(ParseResultType error, Exception innerException)
                : this(error)
            {
                if (error == Success)
                {
                    throw new ArgumentException(
                        message:    "A failure-state [VR.ParseResult] cannot " +
                                    "be created with the [Success] status.",
                        paramName:  nameof(error));
                }

                if (innerException == null)
                {
                    throw new ArgumentNullException(
                        paramName:  nameof(innerException),
                        message:    "The provided inner exception cannot " +
                                    "be null."
                        );
                }

                if (!Enum.IsDefined(typeof(ParseResultType), error))
                {
                    throw new ArgumentException(
                        message:    $"The parse result code {(int)error:X8} is " +
                                     "not recognised.",
                        paramName:  nameof(error));
                }

                // We want to make sure that the code we've been passed is one of
                // the ones that has to be passed with an exception. If it isn't,
                // then we want to throw an exception.
                if (!_exCodes.Contains(error))
                {
                    throw new ArgumentException(
                        message:   $"The parse result code {(int)error:X8} must" +
                                    " not be passed with an exception.",
                        paramName: nameof(error));
                }

                _type = error;
                _results = null;
                _innerEx = innerException;
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
            /// The collection of comparator sets produced by the
            /// parser.
            /// </para>
            /// </summary>
            public ResultSet ComparatorSets => VerifyResult(_results);
            /// <summary>
            /// <para>
            /// The exception that is stored in the parse result to
            /// provide additional error information.
            /// </para>
            /// </summary>
            private Exception InnerException => VerifyResult(_innerEx);

            /// <summary>
            /// <para>
            /// Retrieves a human-friendly error message describing the
            /// error represented by the current <see cref="ParseResult"/>.
            /// </para>
            /// </summary>
            /// <returns>
            /// A string representing the error represented by the
            /// current <see cref="ParseResult"/>.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="ParseResult.Type"/> has the
            /// value <see cref="ParseResultType.Success"/>.
            /// </exception>
            public string GetErrorMessage()
            {
                if (this.Type == Success)
                {
                    throw new InvalidOperationException(
                        "Cannot retrieve error message: operation was " +
                        "successful."
                        );
                }

                switch (this.Type)
                {
                    case NullString:
                    return "The version range string cannot be null, empty, or " +
                           "composed entirely of whitespace characters.";

                    case InvalidCharacter:
                    return "The version range string contains one or more " +
                           "invalid characters.";

                    case EmptySet:
                    return "A version range string cannot contain a set with " +
                           "no comparators.";

                    case OrphanedOperator:
                    return "The version range string contains an operator " +
                           "with no associated version (check whitespace).";

                    case InvalidVersion:
                    return "The version range string contains one or more " +
                           "invalid semantic version strings.";

                    default:
                    {
                        throw new NotSupportedException(
                            $"Unrecognised result code {(int)Type:X8} provided."
                            );
                    }
                }
            }
            /// <summary>
            /// <para>
            /// Creates an <see cref="Exception"/> instance appropriate
            /// for the status represented by the current instance.
            /// </para>
            /// </summary>
            /// <returns>
            /// An <see cref="Exception"/> instance appropriate for the
            /// status represented by the current instance.
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// Thrown when <see cref="ParseResult.Type"/> has the
            /// value <see cref="ParseResultType.Success"/>.
            /// </exception>
            public Exception CreateException()
            {
                if (this.Type == Success)
                {
                    throw new InvalidOperationException(
                        "Cannot retrieve exception: operation was successful."
                        );
                }

                var msg = this.GetErrorMessage();

                switch (this.Type)
                {
                    // [ArgumentNullException] for an empty version range string.
                    case NullString:
                    return new ArgumentNullException(message: msg,
                                                     innerException: _innerEx);

                    // [ArgumentException] for anything that means the version
                    // range string is invalid, but which is unrelated an invalid
                    // version string being present.
                    case InvalidCharacter:
                    case EmptySet:
                    case OrphanedOperator:
                    return new ArgumentException(message: msg,
                                                 innerException: _innerEx);

                    // [FormatException] for anything related to the parsing of a
                    // semantic version string failing.
                    case InvalidVersion:
                    return new FormatException(message: msg,
                                               innerException: _innerEx);

                    default:
                    {
                        throw new NotSupportedException(
                            $"Unrecognised result code {(int)Type:X8} provided."
                            );
                    }
                }
            }
        }

        /// <summary>
        /// <para>
        /// Implements parsing for version ranges.
        /// </para>
        /// </summary>
        internal static class Parser
        {

        }
    }
}
