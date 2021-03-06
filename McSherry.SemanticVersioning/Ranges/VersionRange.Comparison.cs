﻿// Copyright (c) 2015-19 Liam McSherry
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
    using ComponentState = SemanticVersion.ComponentState;

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
            /// Represents a method implementing a unary comparator.
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
            private delegate bool ComparatorImpl(
                SemanticVersion comparand,
                SemanticVersion comparator
                );
            /// <summary>
            /// <para>
            /// Represents a method capable of generating a delegate which
            /// implements a unary comparator.
            /// </para>
            /// </summary>
            /// <param name="comparator">
            /// The <see cref="SemanticVersion"/> that the version being checked
            /// is compared against. This is the version that is extracted from
            /// the range string by the parser.
            /// </param>
            /// <returns>
            /// A delegate which implements the comparison.
            /// </returns>
            private delegate Predicate<SemanticVersion> ComparatorFactory(
                SemanticVersion comparator
                );

            // The 'node-semver' advanced version range syntax includes operators
            // which decompose to a comparator set in the basic syntax. Here,
            // simple comparers represent those basic-syntax operators and the
            // complex comparers the advanced-syntax operators.
            //
            // It's probably desirable to have these separate, as otherwise the
            // advanced-syntax operators will need to create [SemanticVersion]
            // instances each time they're called. This will be expensive if a
            // lot of comparisons are made. Doing it this way means the cost is
            // incurred during parsing (an already expensive operation).
            private static readonly IReadOnlyDictionary<Operator, ComparatorImpl>
                SimpleComparers;
            private static readonly IReadOnlyDictionary<Operator, ComparatorFactory>
                ComplexComparers;


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


            private static Predicate<SemanticVersion> OpCaretFactory(
                SemanticVersion comparator
                )
            {
                SemanticVersion upper;

                // The caret operator allows changes that don't modify the
                // leftmost non-zero version component, so we need to identify
                // which non-zero component is leftmost.
                if (comparator.Major != 0)
                {
                    upper = new SemanticVersion(
                        major: comparator.Major + 1,
                        minor: 0,
                        patch: 0
                        );
                }
                else if (comparator.Minor != 0)
                {
                    upper = new SemanticVersion(
                        major: 0,
                        minor: comparator.Minor + 1,
                        patch: 0
                        );
                }
                else
                {
                    upper = new SemanticVersion(
                        major: 0,
                        minor: 0,
                        patch: comparator.Patch + 1
                        );
                }

                return new Predicate<SemanticVersion>(
                    (arg) => (arg >= comparator) && (arg < upper)
                    );
            }

            private static Predicate<SemanticVersion> OpTildeFactory(
                SemanticVersion comparator
                )
            {
                SemanticVersion lower, upper;

                // The behaviour of tilde-operator comparators varies depending
                // on how the provided semantic version was written, so we must
                // rely on parse metadata to produce the correct bounds.
                //
                // If no minor version is specified, the tilde operator allows
                // minor- and patch-level changes.
                if (comparator.ParseInfo.MinorState == ComponentState.Omitted)
                {
                    lower = new SemanticVersion(
                        major:       comparator.Major,
                        minor:       0,
                        patch:       0,
                        identifiers: comparator.Identifiers,
                        metadata:    comparator.Metadata
                        );

                    upper = new SemanticVersion(
                        major:       lower.Major + 1,
                        minor:       0,
                        patch:       0,
                        identifiers: lower.Identifiers,
                        metadata:    lower.Metadata
                        );
                }
                // If one is specified, the tilde operator allows only changes
                // at the patch level.
                else
                {
                    // We want to maintain a patch version if we have it, or
                    // reset it to zero if we don't.
                    var patch = comparator.ParseInfo.PatchState == ComponentState.Omitted
                        ? 0 
                        : comparator.Patch;

                    lower = new SemanticVersion(
                        major:       comparator.Major,
                        minor:       comparator.Minor,
                        patch:       patch,
                        identifiers: comparator.Identifiers,
                        metadata:    comparator.Metadata
                        );

                    upper = new SemanticVersion(
                        major:       lower.Major,
                        minor:       lower.Minor + 1,
                        patch:       0,
                        identifiers: lower.Identifiers,
                        metadata:    lower.Metadata
                        );
                }

                return (sv) => (sv >= lower) && (sv < upper);
            }

            private static Predicate<SemanticVersion> OpWildcardFactory(
                SemanticVersion comparator
                )
            {
                SemanticVersion lower, upper;

                // The wildcard operator only accepts changes at the same level
                // as the wildcard or at a level subordinate it to it, so our
                // behaviour changes depending on which component is a wildcard.
                //
                // A wildcard major version allows changes at the major version
                // level and below. That's the same as allowing any change, so
                // this will always return true (barring any non-comparability
                // due to pre-release identifiers, but we leave handling that
                // to [VersionRange]).
                if (comparator.ParseInfo.MajorState == ComponentState.Wildcard)
                {
                    return (sv) => true;
                }
                else if (comparator.ParseInfo.MinorState == ComponentState.Wildcard)
                {
                    // Wildcard parameters should default to zero, so we should
                    // be safe in assigning this here.
                    lower = comparator;

                    // Wildcard versions can't have pre-release identifiers, so
                    // we won't bother copying them over.
                    upper = new SemanticVersion(
                        major: lower.Major + 1,
                        minor: 0,
                        patch: 0
                        );
                }
                else
                {
                    // Same as above, except with patch-level changes
                    lower = comparator;

                    upper = new SemanticVersion(
                        major: lower.Major,
                        minor: lower.Minor + 1,
                        patch: 0
                        );
                }

                return (sv) => (sv >= lower) && (sv < upper);
            }

            static UnaryComparator()
            {
                SimpleComparers = new Dictionary<Operator, ComparatorImpl>
                {
                    [Operator.Equal]                = OpEqual,
                    [Operator.LessThan]             = OpLess,
                    [Operator.GreaterThan]          = OpGreater,
                    [Operator.LessThanOrEqual]      = OpLTEQ,
                    [Operator.GreaterThanOrEqual]   = OpGTEQ,
                }.AsReadOnly();

                ComplexComparers = new Dictionary<Operator, ComparatorFactory>
                {
                    [Operator.Caret]                = OpCaretFactory,
                    [Operator.Tilde]                = OpTildeFactory,
                    [Operator.Wildcard]             = OpWildcardFactory,
                }.AsReadOnly();
            }

            /// <summary>
            /// <para>
            /// Creates a new <see cref="IComparator"/> using the specified
            /// version and unary operator.
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
            /// <see cref="Operator"/> value, or is not a unary operator.
            /// </exception>
            public static IComparator Create(Operator op, SemanticVersion semver)
            {
                Predicate<SemanticVersion> impl;

                if (SimpleComparers.TryGetValue(op, out var cmpImpl))
                {
                    impl = (sv) => cmpImpl(sv, semver);
                }
                else if (ComplexComparers.TryGetValue(op, out var cmpFactory))
                {
                    impl = cmpFactory(semver);
                }
                else
                {
                    throw new ArgumentException(
                        message: $"Unrecognised unary operator (value: {op:N}).",
                        paramName: nameof(op)
                        );
                }

                return new UnaryComparator(impl)
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

        /// <summary>
        /// <para>
        /// Provides the parser with implementations for binary operators.
        /// </para>
        /// </summary>
        internal sealed class BinaryComparator : IComparator
        {
            /// <summary>
            /// <para>
            /// Represents a method capable of generating a delegate which
            /// implements a binary comparator.
            /// </para>
            /// </summary>
            /// <param name="lhs">
            /// The <see cref="SemanticVersion"/> which appears on the left
            /// hand side of the operator.
            /// </param>
            /// <param name="rhs">
            /// The <see cref="SemanticVersion"/> which appears on the right
            /// hand side of the operator.
            /// </param>
            /// <returns></returns>
            private delegate Predicate<SemanticVersion> Factory(
                SemanticVersion lhs, SemanticVersion rhs);

            private static readonly IReadOnlyDictionary<Operator, Factory>
                ComparerFactories;


            private static Predicate<SemanticVersion> OpHyphenFactory(
                SemanticVersion lhs, SemanticVersion rhs)
            {
                // Anything omitted in the left-hand version resets to zero.
                var lower = new SemanticVersion(
                    major:       lhs.Major,
                    minor:       lhs.ParseInfo.MinorState == ComponentState.Omitted ? 0 : lhs.Minor,
                    patch:       lhs.ParseInfo.PatchState == ComponentState.Omitted ? 0 : lhs.Patch,
                    identifiers: lhs.Identifiers,
                    metadata:    lhs.Metadata
                    );

                // In the right-hand version, behaviour depends on which of
                // the components is omitted.
                //
                // If the minor version is omitted, the major is incremented by
                // one, the others are reset to zero, and it's a less-than
                // comparison only.
                if (rhs.ParseInfo.MinorState == ComponentState.Omitted)
                {
                    var upper = new SemanticVersion(
                        major: rhs.Major + 1,
                        minor: 0,
                        patch: 0,
                        identifiers: rhs.Identifiers,
                        metadata: rhs.Metadata
                        );

                    return (sv) => (sv >= lower) && (sv < upper);
                }
                // If the patch version is omitted it's similar, but the minor
                // version is incremented by one instead.
                else if (rhs.ParseInfo.PatchState == ComponentState.Omitted)
                {
                    var upper = new SemanticVersion(
                        major:       rhs.Major,
                        minor:       rhs.Minor + 1,
                        patch:       0,
                        identifiers: rhs.Identifiers,
                        metadata:    rhs.Metadata
                        );

                    return (sv) => (sv >= lower) && (sv < upper);
                }
                // And if nothing is omitted, then it's a simple inclusive
                // range comparison against the right-hand version
                else
                {
                    return (sv) => (sv >= lower) && (sv <= rhs);
                }
            }


            static BinaryComparator()
            {
                ComparerFactories = new Dictionary<Operator, Factory>
                {
                    [Operator.Hyphen]   = OpHyphenFactory,
                }.AsReadOnly();
            }


            /// <summary>
            /// <para>
            /// Creates a new <see cref="IComparator"/> using the specified
            /// versions and binary operator.
            /// </para>
            /// </summary>
            /// <param name="op">
            /// The operator to create an equivalent <see cref="IComparator"/>
            /// for.
            /// </param>
            /// <param name="lhs">
            /// The <see cref="SemanticVersion"/> that appears on the left-hand
            /// side of the operator.
            /// </param>
            /// <param name="rhs">
            /// The <see cref="SemanticVersion"/> that appears on the right-hand
            /// side of the operator.
            /// </param>
            /// <returns>
            /// An <see cref="IComparator"/> that implements the comparison
            /// function represented by <paramref name="op"/>.
            /// </returns>
            /// <exception cref="ArgumentException">
            /// Thrown when <paramref name="op"/> is not a recognised
            /// <see cref="Operator"/> value, or is not a binary operator.
            /// </exception>
            public static IComparator Create(
                Operator op, SemanticVersion lhs, SemanticVersion rhs)
            {
                if (ComparerFactories.TryGetValue(op, out var fact))
                {
                    return new BinaryComparator(fact(lhs, rhs))
                    {
                        LeftVersion = lhs,
                        RightVersion = rhs,
                    };
                }
                else
                {
                    throw new ArgumentException(
                        message: $"Unrecognised binary operator (value: {op:N}).",
                        paramName: nameof(op)
                        );
                }
            }


            private readonly Predicate<SemanticVersion> _cmp;

            private BinaryComparator(Predicate<SemanticVersion> impl)
            {
                _cmp = impl;
            }


            /// <summary>
            /// <para>
            /// The <see cref="SemanticVersion"/> on the left-hand side of the
            /// operator.
            /// </para>
            /// </summary>
            public SemanticVersion LeftVersion
            {
                get;
                private set;
            }
            /// <summary>
            /// <para>
            /// The <see cref="SemanticVersion"/> on the right-hand side of the
            /// operator.
            /// </para>
            /// </summary>
            public SemanticVersion RightVersion
            {
                get;
                private set;
            }


            bool IComparator.SatisfiedBy(SemanticVersion comparand)
            {
                return _cmp(comparand);
            }

            bool IComparator.ComparableTo(SemanticVersion comparand)
            {
                return this.LeftVersion.ComparableTo(comparand) ||
                       this.RightVersion.ComparableTo(comparand);
            }
        }
    }
}
