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

namespace McSherry.SemanticVersioning.Ranges
{
    // Documentation and attributes are in 'Ranges\VersionRange.cs'
    public sealed partial class VersionRange
    {
        /// <summary>
        /// <para>
        /// Represents an implementation of a comparator.
        /// </para>
        /// </summary>
        internal interface IComparator
        {
            /*
                So, why bother with this interface if it's private and
                we've only got one class implementing it, and that class
                is little more than a shell?

                Answer: future proofing.

                Currently, we only support the basic features of version
                ranges specified by the 'node-semver' docs, but in future
                we're likely to support the advanced features, and the
                advanced features all decompose into a set of the basic
                features.

                By having this interface here from the start, we don't need
                to do a large amount of rework, we just need to make the
                [VersionRange] class implement the interface, we need to 
                add the appropriate [Operator] values for the advanced features,
                and we then make [Comparator.Create] return [VersionRange]s
                for advanced features, and that's that. It'll also be entirely
                transparent to the [VersionRange] consuming it, which is a big
                plus.
            */

            /// <summary>
            /// <para>
            /// Determines whether the comparator is satisfied
            /// by a specified <see cref="SemanticVersion"/>.
            /// </para>
            /// </summary>
            /// <param name="comparand">
            /// The <see cref="SemanticVersion"/> to check
            /// against the comparator.
            /// </param>
            /// <returns>
            /// True if the comparator is satisfied by the
            /// specified semantic version, false if otherwise.
            /// </returns>
            bool SatisfiedBy(SemanticVersion comparand);

            /// <summary>
            /// Whether a <see cref="SemanticVersion"/> with the pre-release
            /// identifers of the specified version can satisfy the comparator.
            /// </summary>
            /// <param name="comparand">
            /// The <see cref="SemanticVersion"/> of the kind to test.
            /// </param>
            /// <returns>
            /// True if, in line with version range comparison rules for
            /// versions with pre-release identifiers, the comparand is capable
            /// of satisfying the comparator. False if otherwise.
            /// </returns>
            bool ComparableTo(SemanticVersion comparand);
        }

        /// <summary>
        /// <para>
        /// Provides the <see cref="Parser"/> with implementations for unary
        /// operators.
        /// </para>
        /// </summary>
        internal sealed class UnaryComparator : IComparator
        {
            /// <summary>
            /// <para>
            /// Represents a method implementing a comparator.
            /// </para>
            /// </summary>
            /// <param name="comparand">
            /// The <see cref="SemanticVersion"/> that is being compared
            /// against the version specified in the range string.
            /// </param>
            /// <param name="comparator">
            /// The <see cref="SemanticVersion"/> that the version being
            /// checked is compared against. This is the version that is
            /// extracted from the range string by the parser.
            /// </param>
            /// <returns>
            /// True if the comparator is satisfied, false if otherwise.
            /// </returns>
            private delegate bool ComparatorImpl(SemanticVersion comparand,
                                                 SemanticVersion comparator);

            private static readonly IReadOnlyDictionary<Operator, ComparatorImpl>
                Comparers;


            /*
                The [VersionRange] class handles special rules for comparing
                against versions with pre-release identifiers, so these
                implementations need not be concerned with it.
            */

            private static bool OpEqual(SemanticVersion arg, 
                                        SemanticVersion comparator)
            {
                // We don't care about metadata when we're comparing with
                // version ranges, so we use [EquivalentTo] so that those
                // items are ignored.
                return arg.EquivalentTo(comparator);
            }

            private static bool OpLess(SemanticVersion arg,
                                       SemanticVersion comparator)
            {
                return arg < comparator;
            }

            private static bool OpGreater(SemanticVersion arg, 
                                          SemanticVersion comparator)
            {
                return arg > comparator;
            }

            private static bool OpLTEQ(SemanticVersion arg,
                                       SemanticVersion comparator)
            {
                return arg <= comparator;
            }

            private static bool OpGTEQ(SemanticVersion arg,
                                       SemanticVersion comparator)
            {
                return arg >= comparator;
            }

            static UnaryComparator()
            {
                Comparers = new Dictionary<Operator, ComparatorImpl>
                {
                    [Operator.Equal]                = OpEqual,
                    [Operator.LessThan]             = OpLess,
                    [Operator.GreaterThan]          = OpGreater,
                    [Operator.LessThanOrEqual]      = OpLTEQ,
                    [Operator.GreaterThanOrEqual]   = OpGTEQ,
                }.AsReadOnly();
            }

            /// <summary>
            /// <para>
            /// Creates a new <see cref="IComparator"/> using the specified
            /// operator and version.
            /// </para>
            /// </summary>
            /// <param name="op">
            /// The operator to create an equivalent <see cref="IComparator"/>
            /// for.
            /// </param>
            /// <param name="semver">
            /// The <see cref="SemanticVersion"/> the operator is associated with.
            /// </param>
            /// <returns>
            /// An <see cref="IComparator"/> instance that implements the
            /// comparison function represented by the specified
            /// <see cref="Operator"/>.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="op"/> is not a recognised
            /// <see cref="Operator"/> value.
            /// </exception>
            public static IComparator Create(Operator op, SemanticVersion semver)
            {
                if (!Comparers.TryGetValue(op, out var cmpImpl))
                {
                    throw new ArgumentException(
                        message: "Unrecognised operator.",
                        paramName: nameof(op));
                }

                return new UnaryComparator(sv => cmpImpl(sv, semver))
                {
                    Operator = op,
                    Version = semver
                };
            }

            private readonly Predicate<SemanticVersion> _cmpFn;

            /// <summary>
            /// <para>
            /// Creates a new <see cref="UnaryComparator"/> with the specified
            /// function as its comparison function.
            /// </para>
            /// </summary>
            /// <param name="impl">
            /// The function to use as the comparison function.
            /// </param>
            private UnaryComparator(Predicate<SemanticVersion> impl)
            {
                if (impl == null)
                {
                    throw new ArgumentNullException(
                        paramName: nameof(impl),
                        message: "Comparator implementation cannot " +
                                    "be null."
                        );
                }

                _cmpFn = impl;
            }

            /// <summary>
            /// The operator the comparator implements.
            /// </summary>
            public Operator Operator
            {
                get;
                private set;
            }
            /// <summary>
            /// <para>
            /// The version against which the comparator compares.
            /// </para>
            /// </summary>
            public SemanticVersion Version
            {
                get;
                private set;
            }

            bool IComparator.SatisfiedBy(SemanticVersion comparand)
            {
                return _cmpFn(comparand);
            }

            bool IComparator.ComparableTo(SemanticVersion comparand)
            {
                return this.Version.ComparableTo(comparand);
            }
        }
    }
}
