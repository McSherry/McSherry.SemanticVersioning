// Copyright (c) 2015-20 Liam McSherry
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using McSherry.SemanticVersioning.Internals.Shims;

namespace McSherry.SemanticVersioning
{
    using SVF = SemanticVersionFormat;

    /// <summary>
    /// <para>
    /// Lists the format identifiers accepted by the 
    /// <see cref="SemanticVersion"/> class's implementation of
    /// <see cref="IFormattable"/>.
    /// </para>
    /// </summary>
    [CLSCompliant(true)]
    public static class SemanticVersionFormat
    {
#pragma warning disable 618

        /// <summary>
        /// <para>
        /// The default way to format a semantic version.
        /// </para>
        /// </summary>
        public static string Default            => "G";
        /// <summary>
        /// <para>
        /// The default way to format a semantic version, prefixed
        /// with a letter "v".
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// For details on how this option formats a semantic version,
        /// see <see cref="Default"/>.
        /// </para>
        /// </remarks>
        [Obsolete("Use custom format strings instead.", error: false)]
        public static string PrefixedDefault    => "g";

        /// <summary>
        /// <para>
        /// A way to concisely format a semantic version. Omits metadata
        /// and only includes the <see cref="SemanticVersion.Patch"/> version 
        /// if it is non-zero.
        /// </para>
        /// </summary>
        public static string Concise            => "C";
        /// <summary>
        /// <para>
        /// A concise way to format a semantic version, prefixed with a
        /// letter "v".
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// For details on how this option formats a semantic version, see
        /// <see cref="Concise"/>.
        /// </para>
        /// </remarks>
        [Obsolete("Use custom format strings instead.", error: false)]
        public static string PrefixedConcise    => "c";

        /// <summary>
        /// <para>
        /// The format used for monotonic version strings. Aliases
        /// <see cref="Concise"/>.
        /// </para>
        /// </summary>
        public static string Monotonic          => Concise;
        /// <summary>
        /// <para>
        /// The format used for monotonic version strings, prefixed
        /// with a letter "v". Aliases <see cref="PrefixedConcise"/>.
        /// </para>
        /// </summary>
        [Obsolete("Use custom format strings instead.", error: false)]
        public static string PrefixedMonotonic  => PrefixedConcise;

#pragma warning restore 618
    }

    // Documentation/attributes/interfaces/etc are in the main
    // implementation file, "SemanticVersion.cs"
    public sealed partial class SemanticVersion
    {
        /// <summary>
        /// <para>
        /// Encapsulates the formatting routines provided by the
        /// <see cref="SemanticVersion"/> class.
        /// </para>
        /// </summary>
        private static class Formatter
        {
            // If we encounter a standard format specifier when parsing a custom
            // format pattern, we'll expand it to these patterns.
            private const string FMT_GENERAL        = "M.m.pRRDD";
            private const string FMT_GENERAL_PREFIX = "vG";
            private const string FMT_CONCISE        = "M.mppRR";
            private const string FMT_CONCISE_PREFIX = "vC";


            /// <summary>
            /// <para>
            /// Exposes the formatter for <see cref="SemanticVersion"/>
            /// instances.
            /// </para>
            /// </summary>
            /// <param name="semver">
            /// The <see cref="SemanticVersion"/> to be formatted.
            /// </param>
            /// <param name="format">
            /// The format string specifying how the <see cref="SemanticVersion"/>
            /// should be formatted.
            /// </param>
            /// <returns></returns>
            public static string Format(SemanticVersion semver, string format)
            {
                var sb = new StringBuilder();


                // We'll probably be passed the general format specifier most of
                // the time, so we can return that format without getting into
                // parsing the format string.
                if (String.IsNullOrEmpty(format) || format == SVF.Default)
                {
                    // The basics are always present
                    sb.AppendFormat("{0}.{1}.{2}", semver.Major, semver.Minor, semver.Patch);

                    // If there are pre-release identifiers, they're next
                    if (semver.Identifiers.Count > 0)
                    {
                        sb.Append($"-{semver.Identifiers[0]}");

                        semver.Identifiers
                            .Skip(1)
                            .Aggregate(sb, (_, i) => sb.AppendFormat(".{0}", i));
                    }

                    // And the same with metadata items
                    if (semver.Metadata.Count > 0)
                    {
                        sb.Append($"+{semver.Metadata[0]}");

                        semver.Metadata
                            .Skip(1)
                            .Aggregate(sb, (_, i) => sb.AppendFormat(".{0}", i));
                    }

                    return sb.ToString();
                }
                

                IEnumerator<char> iter = null;
                char? current = null;
                int pos = -1;

                // If it isn't the general format specifier, we need to interpret
                // the pattern and build the string as we go.
                RecurseOver(format);


                return sb.ToString();


                // Recursively parses the custom format pattern, adding to the
                // contents of the [StringBuilder] as it proceeds.
                void RecurseOver(string str)
                {
                    var storedIter = iter;
                    var storedCurrent = current;
                    var storedPos = pos;

                    iter = str.GetEnumerator();
                    iter.MoveNext();

                    current = iter.Current;

                    while (current.HasValue)
                        Evaluate();

                    iter = storedIter;
                    current = storedCurrent;
                    pos = storedPos;
                }

                // Evaluates the current input character
                void Evaluate()
                {
                    // 'M' is for the major version component
                    if (Take('M'))
                        Major();

                    // 'm' is for the minor version component
                    else if (Take('m'))
                        Minor();

                    // 'p' is for the patch version component, however...
                    else if (Take('p'))
                    {
                        // If we have 'pp', we only include the patch component
                        // in the formatted string if it's non-zero.
                        if (Take('p'))
                        {
                            if (semver.Patch != 0)
                            {
                                sb.Append('.');
                                Patch();
                            }
                        }
                        // But, if we have just 'p', we always include it.
                        else
                        {
                            Patch();
                        }
                    }

                    // 'R' is for standalone identifiers, but 'RR' means we
                    // have to prefix them with the hyphen separator.
                    else if (Take('R'))
                    {
                        // If we have identifiers, they'll be included.
                        if (semver.Identifiers.Count > 0)
                        {
                            // If the specifier is 'RR', we have to include the
                            // hyphen separator before the identifiers.
                            if (Take('R'))
                                sb.Append('-');

                            Identifiers();
                        }
                        // If we don't have identifiers, we consume a following 'R'
                        // if it's present and proceed without including anything.
                        else
                        {
                            Take('R');
                        }
                    }

                    // A character 'r' can either be the first part of an indexed
                    // pre-release identifier specifier 'r#', or the first part of
                    // the reserved specifier 'rr'.
                    else if (Take('r'))
                    {
                        if (Take('r'))
                            Error(@"Format specifier ""rr"" is reserved and must not be used.");

                        else
                        {
                            var intBdr = new StringBuilder();

                            // A single 'r' followed by numbers is an index into
                            // the pre-release identifiers, which we have to turn
                            // into a number and use to find the correct identifier.
                            if (TakeNumerals(intBdr))
                            {
                                // If we can parse it, then we include the identifier
                                // at the specified index.
                                if (Int32.TryParse(intBdr.ToString(), out var idx))
                                {
                                    if (idx >= semver.Identifiers.Count)
                                        Error("Pre-release identifier index out of bounds.");

                                    sb.Append(semver.Identifiers[idx]);
                                }
                                else
                                    Error("Could not convert index to Int32.");
                            }
                            // If the single 'r' isn't followed by numbers, then we
                            // store it and whatever came after it.
                            else
                            {
                                sb.Append('r');
                                Store();
                            }
                        }
                    }

                    // The 'D' and 'DD' specifiers are for standalone and prefixed
                    // metadata item groups, like with 'R' and 'RR'.
                    else if (Take('D'))
                    {
                        // As with 'R' and 'RR', we include nothing if there are
                        // no metadata items.
                        if (semver.Metadata.Count > 0)
                        {
                            // Including the separator for 'DD'.
                            if (Take('D'))
                                sb.Append('+');

                            Metadata();
                        }
                        // And consuming the second 'D' without doing anything
                        // if we don't have any metadata items.
                        else
                        {
                            Take('D');
                        }
                    }

                    // As with 'r', 'd' can be followed by a number or another 'd',
                    // and it's handled in exactly the same way.
                    else if (Take('d'))
                    {
                        if (Take('d'))
                            Error(@"Format specifier ""dd"" is reserved and must not be used.");

                        else
                        {
                            var intBdr = new StringBuilder();

                            if (TakeNumerals(intBdr))
                            {
                                if (Int32.TryParse(intBdr.ToString(), out var idx))
                                {
                                    if (idx >= semver.Metadata.Count)
                                        Error("Metadata item index index out of bounds.");

                                    sb.Append(semver.Metadata[idx]);
                                }
                                else
                                    Error("Could not convert index to Int32.");
                            }
                            else
                            {
                                sb.Append('d');
                                Store();
                            }
                        }
                    }

                    // The 'G' specifier expands to the general format
                    else if (Take('G'))
                        RecurseOver(FMT_GENERAL);

                    // While 'g' is the same, but prefixed.
                    else if (Take('g'))
                        RecurseOver(FMT_GENERAL_PREFIX);

                    // The 'C' specifier is for the concise format, where patch
                    // and metadata components can be excluded.
                    else if (Take('C'))
                        RecurseOver(FMT_CONCISE);

                    // And, as above, 'c' is the prefixed form of 'C'.
                    else if (Take('c'))
                        RecurseOver(FMT_CONCISE_PREFIX);

                    // To aid in formatting, we allow our caller to specify that
                    // portions of the custom format pattern should be included
                    // verbatim by surrounding them by double braces.
                    else if (Take('{'))
                    {
                        // If we get two braces, then we need to continue until
                        // we find the two closing braces.
                        if (Take('{'))
                        {
                            TakeUntilBraces();


                            void TakeUntilBraces()
                            {
                                // If we reach the end of the string without a closing
                                // brace pair, that's an error.
                                if (!current.HasValue)
                                    Error("Verbatim block not terminated.");

                                // If we find a single closing brace, we need
                                // to check for a second.
                                if (Take('}'))
                                {
                                    // If we don't get the second brace, we need
                                    // to store this character and try again.
                                    if (!Take('}'))
                                    {
                                        Store();
                                        TakeUntilBraces();
                                    }

                                    // Otherwise, store and recurse.
                                }

                                // If it's something else, then we'll store it verbatim
                                // and continue looking.
                                else
                                {
                                    Store();
                                    TakeUntilBraces();
                                }
                            }
                        }
                        // Otherwise, we store a single brace and move on.
                        else
                        {
                            sb.Append('{');
                        }
                    }

                    // If we don't have any particular logic for this character,
                    // include it in the final string as-is.
                    else
                        Store();
                }

                // Consumes an input character if it equals the argument, returning
                // true when a character has been consumed.
                bool Take(char c)
                {
                    if (current == c)
                    {
                        Consume();

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                
                // Unconditionally consumes an input character and stores it in
                // the [StringBuilder].
                void Store()
                {
                    sb.Append(current.Value);
                    Consume();
                }
                
                // Unconditionally consumes an input character.
                void Consume()
                {
                    current = iter.MoveNext() ? iter.Current : default(char?);
                    pos++;
                }
                
                // Produces an error with the specified message.
                void Error(string message)
                {
                    throw new FormatException(
                        $"Invalid format string (at position {pos}): {message}"
                        );
                }

                // Consumes input characters while they are numeric, storing them
                // in the provided [StringBuilder]. Returns true if any characters
                // were consumed.
                bool TakeNumerals(StringBuilder builder)
                {
                    // If we reach the end of the input, our success depends on
                    // whether we consumed any input.
                    if (!current.HasValue)
                        return builder.Length > 0;

                    // If this isn't the end, we continue while we're consuming
                    // numeric characters only.
                    else if ('0' <= current.Value && current.Value <= '9')
                    {
                        builder.Append(current.Value);
                        Consume();

                        TakeNumerals(builder);

                        return true;
                    }

                    // And, as soon as we reach a non-numeric, we end.
                    else
                    {
                        return false;
                    }
                }

                void Major() => sb.Append(semver.Major);
                void Minor() => sb.Append(semver.Minor);
                void Patch() => sb.Append(semver.Patch);
                void Identifiers()
                {
                    // The custom format pattern parser should ensure we never
                    // come here when it would be invalid.

                    sb.Append(semver.Identifiers[0]);

                    foreach (var id in semver.Identifiers.Skip(1))
                        sb.AppendFormat(".{0}", id);
                }
                void Metadata()
                {
                    sb.Append(semver.Metadata[0]);

                    foreach (var md in semver.Metadata.Skip(1))
                        sb.AppendFormat(".{0}", md);
                }
            }
        }

        // [ToString(string, IFormatProvider)] is implemented explicitly because
        // the [IFormatProvider] parameter is always ignored, so there isn't much 
        // sense in exposing it to the user, who will never use the parameter.

        /// <summary>
        /// <para>
        /// Formats the value of the current <see cref="SemanticVersion"/>
        /// as specified.
        /// </para>
        /// </summary>
        /// <param name="format">
        /// The format to use, or null for the default format.
        /// </param>
        /// <param name="provider">
        /// The format provider to use, or null for the default provider. 
        /// This parameter is ignored.
        /// </param>
        /// <returns>
        /// A string representation of the current <see cref="SemanticVersion"/>,
        /// formatted as specified.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown when the format specifier given in <paramref name="format"/>
        /// is not recognised or is invalid.
        /// </exception>
        /// <remarks>
        /// <para>
        /// The format of a Semantic Version is not dependent on culture
        /// information, and so the value of <paramref name="provider"/>
        /// is ignored.
        /// </para>
        /// <para>
        /// The value of <paramref name="format"/> should contain one of
        /// the below-listed format specifiers. Custom format patterns
        /// are not supported. If <paramref name="format"/> is null, the
        /// default format specifier, "G", is used in its place.
        /// </para>
        /// <para>
        /// The list of recognised format specifiers is given in the
        /// below table.
        /// </para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Format Specifier</term>
        ///         <term>Description</term>
        ///         <term>Example</term>
        ///     </listheader>
        /// 
        ///     <item>
        ///         <term><c>"c"</c></term>
        ///         <term>
        ///             Prefixed concise format. Identical
        ///             to the concise format (<c>"C"</c>),
        ///             except prefixed with a lowercase "v".
        ///         </term>
        ///         <term>
        ///             <para>v1.8</para>
        ///             <para>v1.15.1</para>
        ///             <para>v2.1-beta.3</para>
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term><c>"C"</c></term>
        ///         <term>
        ///             Concise format. Omits metadata items,
        ///             and only includes the <see cref="Patch"/>
        ///             version if it is non-zero.
        ///         </term>
        ///         <term>
        ///             <para>1.8</para>
        ///             <para>1.15.1</para>
        ///             <para>2.1-beta.3</para>
        ///         </term>
        ///     </item>
        /// 
        ///     <item>
        ///         <term><c>"g"</c></term>
        ///         <term>
        ///             Prefixed default format. Identical to
        ///             the default format (<c>"G"</c>), except
        ///             prefixed with a lowercase "v".
        ///         </term>
        ///         <term>
        ///             <para>v1.7.0-alpha.2+20150925.f8f2cb1a</para>
        ///             <para>v1.2.5</para>
        ///             <para>v2.0.1-rc.1</para>
        ///         </term>
        ///     </item>
        ///     <item>
        ///         <term><c>"G"</c>, <c>null</c></term>
        ///         <term>
        ///             The default format, as given by the
        ///             Semantic Versioning 2.0.0 specification.
        ///         </term>
        ///         <term>
        ///             <para>1.7.0-alpha.2+20150925.f8f2cb1a</para>
        ///             <para>1.2.5</para>
        ///             <para>2.0.1-rc.1</para>
        ///         </term>
        ///     </item>
        /// </list>
        /// </remarks>
        string IFormattable.ToString(string format, IFormatProvider provider)
        {
            return Formatter.Format(this, format);
        }
        /// <summary>
        /// <para>
        /// Formats the value of the current <see cref="SemanticVersion"/>
        /// as specified.
        /// </para>
        /// </summary>
        /// <param name="format">
        /// The format to use, or null for the default format.
        /// </param>
        /// <returns>
        /// A string representation of the current <see cref="SemanticVersion"/>,
        /// formatted as specified.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown when the format specifier given in <paramref name="format"/>
        /// is not recognised or is invalid.
        /// </exception>
        /// <remarks>
        /// <para>
        /// For information on the acceptable format specifiers, see the
        /// Remarks section of the 
        /// <see cref="IFormattable.ToString(string, IFormatProvider)"/>
        /// implementation for <see cref="SemanticVersion"/>.
        /// </para>
        /// </remarks>
        public string ToString(string format)
        {
            return Formatter.Format(this, format);
        }
    }
}
