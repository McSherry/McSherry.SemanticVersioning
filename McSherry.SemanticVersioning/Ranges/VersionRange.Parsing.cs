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
using static McSherry.SemanticVersioning.Ranges.VersionRange.ParseResultType;

using DD = System.Diagnostics.DebuggerDisplayAttribute;

namespace McSherry.SemanticVersioning.Ranges
{
    using ResultSet = IEnumerable<IEnumerable<VersionRange.IComparator>>;

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
            /// Used to check that the leftmost non-zero trio component of a
            /// version does not change.
            /// </para>
            /// </summary>
            Caret,
            /// <summary>
            /// <para>
            /// If a minor version is specified, used to check that only patch-level
            /// changes are made. Otherwise, used to check that only minor-level
            /// changes are made.
            /// </para>
            /// </summary>
            Tilde,
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
            /// <summary>
            /// <para>
            /// Checks that a <see cref="SemanticVersion"/> falls within an
            /// inclusive range of versions.
            /// </para>
            /// </summary>
            Hyphen,
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
        [DD("Type={Type}")]
        internal struct ParseResult
        {
            // Certain codes are required to be provided with an exception. 
            // This hashset will contain those specific codes so we can 
            // check for them.
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
            private readonly Lazy<Exception> _innerEx;

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
                _innerEx = new Lazy<Exception>(() => null);
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
            public ParseResult(ParseResultType error, 
                               Lazy<Exception> innerException)
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
            private Exception InnerException => VerifyResult(_innerEx.Value);

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
                    return new ArgumentNullException(msg, this.InnerException);

                    // [ArgumentException] for anything that means the version
                    // range string is invalid, but which is unrelated an invalid
                    // version string being present.
                    case InvalidCharacter:
                    case EmptySet:
                    case OrphanedOperator:
                    return new ArgumentException(msg, this.InnerException);

                    // [FormatException] for anything related to the parsing of a
                    // semantic version string failing.
                    case InvalidVersion:
                    return new FormatException(msg, this.InnerException);

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
            private enum State
            {
                /// <summary>
                /// <para>
                /// Initialises version range parsing.
                /// </para>
                /// </summary>
                Start,

                /// <summary>
                /// <para>
                /// Consumes a character from the input and transitions to the
                /// next queued state.
                /// </para>
                /// </summary>
                Consume,
                /// <summary>
                /// <para>
                /// Attempts to identify the state most appropriate parse state
                /// to transition to.
                /// </para>
                /// </summary>
                Identify,
                /// <summary>
                /// <para>
                /// Consumes all whitespace characters it encounters until it
                /// reaches a non-whitespace character.
                /// </para>
                /// </summary>
                CollapseWhitespace,

                /// <summary>
                /// <para>
                /// Parses a simple unary-operator comparator.
                /// </para>
                /// </summary>
                UnarySimple,
                /// <summary>
                /// <para>
                /// Parses a complex unary-operator comparator.
                /// </para>
                /// </summary>
                UnaryComplex,
                /// <summary>
                /// <para>
                /// Parses a logical OR operator. This operator is special as it
                /// indicates the start of a new comparator set rather than
                /// operating on version strings.
                /// </para>
                /// </summary>
                LogicalOR,

                /// <summary>
                /// <para>
                /// Attempts to parse a binary infix comparator.
                /// </para>
                /// </summary>
                TentativeBinary,

                /// <summary>
                /// <para>
                /// Parses a version string.
                /// </para>
                /// </summary>
                VersionString,

                /// <summary>
                /// <para>
                /// Attempts to identify the end of a comparator set, collect its
                /// comparators, and begin a new set.
                /// </para>
                /// </summary>
                CollectSet,

                /// <summary>
                /// <para>
                /// Terminates version range parsing if parsing was successful.
                /// </para>
                /// </summary>
                Terminate,
            }

            // Maps the strings we recognise as operators to the operators that
            // they represent.
            //
            // We only expect strings of one or two characters at this stage, so
            // implementing a trie is probably more effort than it's worth.
            private static readonly IReadOnlyDictionary<string, Operator> OperatorMap =
                new Dictionary<string, Operator>
                {
                    { "=",  Operator.Equal              },
                    { "<",  Operator.LessThan           },
                    { "^",  Operator.Caret              },
                    { "~",  Operator.Tilde              },
                    { ">",  Operator.GreaterThan        },
                    { "<=", Operator.LessThanOrEqual    },
                    { ">=", Operator.GreaterThanOrEqual },
                    { "-",  Operator.Hyphen             },
                };

            // Rather than directly typing the literals in the parser,
            // we're going to use a set of constants with appropriate
            // names.
            private const char  LeftChevron     = '<',
                                RightChevron    = '>',
                                EqualSign       = '=',
                                VerticalBar     = '|',
                                Caret           = '^',
                                Tilde           = '~';

            /// <summary>
            /// <para>
            /// Parses a set of comparator sets from a version range string.
            /// </para>
            /// </summary>
            /// <param name="rangeString">
            /// The string representing one or more comparator sets.
            /// </param>
            /// <returns>
            /// An <see cref="ParseResult"/> indicating whether parsing
            /// was successful and containing the result if it was.
            /// </returns>
            private static ParseResult _parseString(string rangeString)
            {
                if (String.IsNullOrWhiteSpace(rangeString))
                    return new ParseResult(NullString);

                // This is where we push each comparator set once we've collected
                // each comparator in it.
                var setOfSets = new List<IEnumerable<IComparator>>();
                // And this is the list we use to build each comparator set before
                // we push it into [setOfSets].
                var cmpSet = new List<IComparator>();

                // The characters of the input string.
                var chars = rangeString.GetEnumerator();

                // A stack to keep track of the states we are to transition to.
                var stateStack = new Stack<State>();

                void PushState(params State[] states)
                {
                    foreach (var s in states)
                        stateStack.Push(s);
                }
                State PopState()
                {
                    try
                    {
                        return stateStack.Pop();
                    }
                    catch (InvalidOperationException ioex)
                    {
                        throw new InvalidOperationException(
                            "Version range parser exhausted the available states.",
                            innerException: ioex
                            );
                    }
                }

                var state = State.Start;
                char? input = null;
                var result = default(ParseResult);
                var @operator = (Operator)(-1);
                var builder = new StringBuilder();

                // Where we'll store the lefthand version of a binary infix
                // operator, if we encounter one.
                SemanticVersion binLeft = null;

                // The default modes for the parser, and temporary modes which
                // are reset after a version string is parsed.
                const ParseMode parseMode = ParseMode.AllowPrefix;
                var tempMode = ParseMode.Strict;

                do
                {
                    switch (state)
                    {
                        // Initialises parsing.
                        case State.Start:
                        {
                            PushState(State.Identify);
                            state = State.Consume;
                        } break;

                        // Consumes a character and transitions to the next
                        // queued state.
                        case State.Consume:
                        {
                            if (chars.MoveNext())
                            {
                                input = chars.Current;
                            }
                            else
                            {
                                // We're using [null] to indicate the end of
                                // the string, as this lets us avoid duplicating
                                // code outside of the loop.
                                input = null;
                            }

                            state = PopState();
                        } break;

                        // Consumes whitespace characters until reaching a
                        // non-whitespace character.
                        case State.CollapseWhitespace:
                        {
                            if (input.HasValue && Char.IsWhiteSpace(input.Value))
                                PushState(State.CollapseWhitespace, State.Consume);

                            state = PopState();
                        } break;

                        // Attempts to identify the state most appropriate to
                        // transition to.
                        case State.Identify:
                        {
                            // If we have no value, then we want to collect any
                            // comparators we have and terminate.
                            if (!input.HasValue)
                                PushState(State.Terminate, State.CollectSet);
                            // If we encounter whitespace, we want to consume
                            // it all until we reach a character we can use.
                            else if (Char.IsWhiteSpace(input.Value))
                                PushState(State.Identify, State.CollapseWhitespace);
                            // Encountering anything else means we've reached
                            // input that we most likely want to use.
                            else switch (input.Value)
                            {
                                // Identify simple unary operators.
                                //
                                // These are single-character operators that
                                // are prefixed to version strings.
                                case EqualSign:
                                case Caret:
                                    PushState(State.UnarySimple);
                                    break;

                                // While [Tilde] is a simple unary operator, it
                                // requires a temporary parsing mode.
                                //
                                // We'll set that then piggybag onto the regular
                                // case for simple unary operators.
                                case Tilde:
                                {
                                    tempMode =
                                        ParseMode.OptionalPatch |
                                        SemanticVersion.InternalModes.OptionalMinor |
                                        SemanticVersion.InternalModes.IndicateOmits;

                                } goto case EqualSign;

                                // Identify complex unary operators.
                                //
                                // These are operators which are, or may be,
                                // composed of multiple characters and which
                                // are prefixed to version strings.
                                case LeftChevron:
                                case RightChevron:
                                    PushState(State.UnaryComplex);
                                    break;

                                // Two vertical bars ('||') indicate a logical
                                // OR with another comparator set, and so if
                                // we encounter a vertical bar we want to try
                                // and identify this operator.
                                //
                                // As we don't need to identify which operator
                                // we first encountered, we can also consume a
                                // character before transitioning.
                                case VerticalBar:
                                    PushState(State.LogicalOR, State.Consume);
                                    break;

                                // If the character hasn't been recognised
                                // above, chances are it's the start of a
                                // version string. We can deal with invalid
                                // characters in the state for those.
                                default:
                                    PushState(State.VersionString);
                                    break;
                            };

                            state = PopState();
                        } break;


                        // Parses simple unary operators.
                        case State.UnarySimple:
                        {
                            // If no valid operator value is set, we haven't
                            // yet determined which operator we're dealing with.
                            if (@operator < 0)
                            {
                                // Identify the operator
                                @operator = OperatorMap[new string(input.Value, 1)];
                                // Consume input and return
                                PushState(State.UnarySimple, State.Consume);
                            }
                            // If we've identified the operator, we need to move
                            // into identifying a version string. Before doing so,
                            // we have error checking to carry out.
                            else
                            {
                                // If we encounter an operator and nothing else,
                                // that obviously isn't valid.
                                //
                                // We only test for nothing and whitespace as we
                                // are leaving dealing with other characters to
                                // the state responsible for version strings.
                                if (!input.HasValue || Char.IsWhiteSpace(input.Value))
                                {
                                    result = new ParseResult(OrphanedOperator);

                                    PushState(State.Terminate);
                                }
                                // If there are no errors, parse a version string.
                                else
                                {
                                    PushState(State.VersionString);
                                }
                            }

                            state = PopState();
                        } break;

                        // Parses complex unary operators
                        case State.UnaryComplex:
                        {
                            // We need to try and identify an operator, which may
                            // or may not be made of multiple characters. If it
                            // is, some of it may be ambiguous with an operator
                            // that is a single-character, so we need to deal with
                            // that too.
                            //
                            // We do this by accumulating two characters straight
                            // away and seeing if we recognise them as a pair. If
                            // we don't, we try again with just the one.
                            //
                            // We only expect to deal with two-character operators,
                            // so it's fine to work like this.


                            // We know the first character is safe because we
                            // had to switch on it to get here.
                            if (builder.Length == 0)
                            {
                                // Accumulate
                                builder.Append(input.Value);

                                // Consume and repeat
                                PushState(State.UnaryComplex, State.Consume);
                            }
                            // After that, though, we're in uncharted territory.
                            else if (builder.Length == 1)
                            {
                                // If we encounter the end or a whitespace
                                // character, we'll indicate that we encountered
                                // an invalid character. We can't yet say if it
                                // is an operator.
                                if (!input.HasValue || Char.IsWhiteSpace(input.Value))
                                {
                                    // If the first character is a valid operator,
                                    // we want to return a different error message
                                    // so the problem is clearer.
                                    if (OperatorMap.ContainsKey(builder.ToString()))
                                        result = new ParseResult(OrphanedOperator);
                                    else
                                        result = new ParseResult(InvalidCharacter);

                                    PushState(State.Terminate);
                                }
                                // Otherwise, we're safe to accumulate and then
                                // go through this state again. We don't want
                                // to consume the current character as we don't
                                // yet know whether it's part of an operator.
                                else
                                {
                                    builder.Append(input.Value);

                                    PushState(State.UnaryComplex);
                                }
                            }
                            // But once we get here, we have two characters we
                            // can work with.
                            else
                            {
                                // Try to identify an operator using the two
                                // characters we've accumulated.
                                if (OperatorMap.TryGetValue(builder.ToString(), out var op))
                                {
                                    // If we manage to, great. We only store the
                                    // operator here as [TryGetValue] will default
                                    // its [out] parameter.
                                    @operator = op;

                                    // Clear stored state
                                    builder.Clear();

                                    // Consume the current character and begin
                                    // parsing a version string
                                    PushState(State.VersionString, State.Consume);
                                }
                                // If we don't manage to identify an operator
                                // with the two characters...
                                else
                                {
                                    // Get rid of the second
                                    builder.Remove(builder.Length - 1, 1);

                                    // Try to identify again
                                    if (OperatorMap.TryGetValue(builder.ToString(), out op))
                                    {
                                        // Store and clear if we succeed
                                        @operator = op;
                                        builder.Clear();

                                        // And proceed to trying to parse a
                                        // version string. The current character
                                        // wasn't part of an operator, so we
                                        // don't consume it.
                                        PushState(State.VersionString);
                                    }
                                    // And if we can't, we don't know what
                                    // the input is and so it must be invalid.
                                    else
                                    {
                                        result = new ParseResult(InvalidCharacter);

                                        PushState(State.Terminate);
                                    }
                                }
                            }

                            state = PopState();
                        } break;

                        // Attempts to identify the logical OR operator and, if
                        // it does, collects the current comparator set and starts
                        // a new one.
                        case State.LogicalOR:
                        {
                            // The first vertical bar will have been consumed,
                            // so we only need to verify that the current input
                            // character is one to continue.
                            if (input.HasValue && input.Value == VerticalBar)
                            {
                                // If it is, we can assemble any collected
                                // comparators into a set.
                                //
                                // Once a set is collected, we consume the current
                                // vertical bar and return to identifying input.
                                PushState(State.Identify, State.Consume, State.CollectSet);
                            }
                            // If it isn't a vertical bar, that's an error.
                            else
                            {
                                result = new ParseResult(InvalidCharacter);

                                PushState(State.Terminate);
                            }

                            state = PopState();
                        } break;


                        // Parses a version string.
                        case State.VersionString:
                        {
                            // If we've not reached a null character or a
                            // whitespace character (which indicate the end
                            // of a version string)...
                            if (input.HasValue && !Char.IsWhiteSpace(input.Value))
                            {
                                // Accumulate the character
                                builder.Append(input.Value);

                                // Consume and repeat
                                PushState(State.VersionString, State.Consume);
                            }
                            // If we have reached the end of a version string...
                            else
                            {
                                // And if there are characters accumulated...
                                if (builder.Length > 0)
                                {
                                    // Attempt to parse whatever we have
                                    var sv = CollectVersion();

                                    // Null indicates a failure to parse. Error.
                                    if (sv == null)
                                    {
                                        PushState(State.Terminate);
                                    }
                                    // A lack of an operator means we have to do
                                    // two things.
                                    //
                                    // First, we implicitly use the equals
                                    // for the comparator.
                                    //
                                    // Second, as it tells us that we haven't
                                    // tried to parse a binary version, we see
                                    // if we're able to do so.
                                    else if (@operator < 0)
                                    {
                                        // Implicitly use equals
                                        @operator = Operator.Equal;

                                        // Store for later
                                        binLeft = sv;

                                        // Determine whether we're dealing with
                                        // a binary infix operator.
                                        PushState(State.TentativeBinary);
                                    }
                                    // If we have an operator, then either one
                                    // was specified with the comparator or we're
                                    // trying to parse the second version in a
                                    // binary comparator.
                                    //
                                    // If we haven't stored a lefthand version,
                                    // we can't be parsing a binary comparator.
                                    else if (binLeft == null)
                                    {
                                        // Collect the unary comparator and add
                                        // it to our set of comparators.
                                        CollectUnary(sv);

                                        // And do it all again
                                        PushState(State.Identify);
                                    }
                                    // If we do have a lefthand version, we're
                                    // now parsing the righthand version.
                                    else
                                    {
                                        // Add the comparator
                                        cmpSet.Add(BinaryComparator.Create(
                                            op:  @operator,
                                            lhs: binLeft,
                                            rhs: sv
                                            ));

                                        // Reset our state
                                        @operator = (Operator)(-1);
                                        binLeft = null;

                                        // And do it all again
                                        PushState(State.Identify);
                                    }
                                }
                                // And if there are no accumulated characters...
                                else
                                {
                                    // We can only have entered this state by
                                    // encountering an operator first (as a
                                    // version string on its own, without an
                                    // operator, must accumulate at least one
                                    // character).
                                    //
                                    // As such, any operator we did encounter
                                    // cannot be associated with a version
                                    // string, which is an error.
                                    result = new ParseResult(OrphanedOperator);

                                    PushState(State.Terminate);
                                }
                            }

                            state = PopState();
                        } break;

                        // Attempts to determine whether a binary infix operator
                        // is present, and parse it if it is.
                        case State.TentativeBinary:
                        {
                            // If we reach the end of the string before an
                            // operator, we obviously don't have a binary comparator.
                            if (!input.HasValue)
                            {
                                // Store the comparator and quit parsing

                                CollectUnary(binLeft);

                                PushState(State.Terminate, State.CollectSet);
                            }
                            // If we end up on whitespace, then we want to skip
                            // to a useful character
                            else if (Char.IsWhiteSpace(input.Value))
                            {
                                PushState(
                                    State.TentativeBinary,
                                    State.CollapseWhitespace
                                    );
                            }
                            // If we're on a useful character and it represents
                            // an operator, we want to make sure it's a binary
                            // operator before handling it.
                            else if (OperatorMap.TryGetValue(
                                key: new string(input.Value, 1),
                                value: out var op))
                            {
                                switch (op)
                                {
                                    // If it's a binary operator...
                                    case Operator.Hyphen:
                                    {
                                        // Update our stored operator
                                        @operator = op;

                                        // Move past the operator, collapse
                                        // whatever whitespace follows, and try
                                        // to parse the right-hand version.
                                        PushState(
                                            State.VersionString,
                                            State.CollapseWhitespace,
                                            State.Consume
                                            );
                                    }
                                    break;

                                    // If it isn't a binary operator...
                                    default:
                                    {
                                        // Turn whatever we have into a
                                        // comparator
                                        CollectUnary(binLeft);

                                        // Clear state
                                        binLeft = null;

                                        // And try to identify it
                                        PushState(State.Identify);
                                    }
                                    break;
                                }
                            }
                            // If it doesn't represent an operator, then we
                            // want to turn whatever we have into a comparator
                            // and try to identify whatever the character is
                            else
                            {
                                CollectUnary(binLeft);

                                binLeft = null;

                                PushState(State.Identify);
                            }

                            state = PopState();
                        } break;


                        // Attempts to collect all the currently-identified
                        // comparators into a comparator set and begin a new
                        // comparator set.
                        case State.CollectSet:
                        {
                            // To collect a set, we must have comparators.
                            if (cmpSet.Any())
                            {
                                // Store the current set
                                setOfSets.Add(cmpSet.AsReadOnly());

                                // Begin a new one
                                cmpSet = new List<IComparator>();

                                // We don't push any state here. This is left
                                // to our caller, as this enables this state
                                // to be used both with the logical OR ('||')
                                // operator and for the final collection.
                            }
                            // A lack of comparators is an error.
                            else
                            {
                                result = new ParseResult(EmptySet);

                                PushState(State.Terminate);
                            }

                            state = PopState();
                        } break;
                    }
                } while (state != State.Terminate);


                // If the parser has not set a result, it indicates that no
                // errors were encountered in parsing and so that we are safe
                // to assemble a successful result.
                if (object.Equals(result, default(ParseResult)))
                {
                    result = new ParseResult(setOfSets.AsReadOnly());
                }
                
                return result;


                SemanticVersion CollectVersion()
                {
                    var verParse = SemanticVersion.Parser.Parse(
                        input:  builder.ToString(),
                        mode:   parseMode | tempMode
                        );

                    tempMode = ParseMode.Strict;

                    if (verParse.Type == SemanticVersion.ParseResultType.Success)
                    {
                        builder.Clear();

                        return verParse.Version;
                    }
                    else
                    {
                        result = new ParseResult(
                            InvalidVersion,
                            new Lazy<Exception>(verParse.CreateException)
                            );

                        return null;
                    }
                }

                void CollectUnary(SemanticVersion sv)
                {
                    // Add the comparator to the set
                    cmpSet.Add(UnaryComparator.Create(
                        op:     @operator,
                        semver: sv
                        ));

                    // Reset state
                    @operator = (Operator)(-1);
                }
            }

            /// <summary>
            /// <para>
            /// Implements <see cref="VersionRange"/> parsing.
            /// </para>
            /// </summary>
            /// <param name="rangeString">
            /// The string representing the version range to be parsed.
            /// </param>
            /// <returns>
            /// A <see cref="ParseResult"/> indicating whether parsing
            /// was successful and, if it was, containing a 
            /// <see cref="VersionRange"/> equivalent to the value of
            /// <paramref name="rangeString"/>.
            /// </returns>
            public static ParseResult Parse(string rangeString)
            {
                // Nothing here yet, but we might want to have some intermediary
                // code in future. Who knows.
                return _parseString(rangeString);
            }
        }

        /// <summary>
        /// <para>
        /// Parses a version range from a string.
        /// </para>
        /// </summary>
        /// <param name="range">
        /// The string representing the version range.
        /// </param>
        /// <returns>
        /// A <see cref="VersionRange"/> equivalent to the
        /// value of <paramref name="range"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="range"/> is null, empty,
        /// or contains only whitespace characters.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="range"/> is invalid for any
        /// reason unrelated to an invalid semantic version string.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown when <paramref name="range"/> contains an invalid
        /// semantic version string.
        /// </exception>
        public static VersionRange Parse(string range)
        {
            // This constructor does all the same stuff as this method would
            // do, so we might as well just call it instead of reimplementing.
            return new VersionRange(range);
        }
        /// <summary>
        /// <para>
        /// Attempts to parse a version range from a string.
        /// </para>
        /// </summary>
        /// <param name="range">
        /// The string representing the version range.
        /// </param>
        /// <param name="result">
        /// The <see cref="VersionRange"/> that, on success, is
        /// given a value equivalent to <paramref name="range"/>.
        /// </param>
        /// <returns>
        /// True on success, false on failure.
        /// </returns>
        public static bool TryParse(string range, out VersionRange result)
        {
            var parseResult = Parser.Parse(range);

            // If the parsing wasn't successful, set [result] to null and
            // return false to indicate failure.
            if (parseResult.Type != Success)
            {
                result = null;
                return false;
            }

            result = new VersionRange(parseResult.ComparatorSets);
            return true;
        }
    }
}
