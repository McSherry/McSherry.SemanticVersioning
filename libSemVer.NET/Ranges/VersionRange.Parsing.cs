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

namespace McSherry.SemanticVersioning.Ranges
{
    // Documentation and attributes are in 'Ranges\VersionRange.cs'
    public sealed partial class VersionRange
    {
        /// <summary>
        /// <para>
        /// Represents the operators that the <see cref="VersionRange"/>
        /// parser recognises.
        /// </para>
        /// </summary>
        private enum Operator
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
        private struct Comparator
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
            /// Creates a new <see cref="Comparator"/> instance with
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
            public Comparator(Operator op, SemanticVersion semver)
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
        /// Implements parsing for version ranges.
        /// </para>
        /// </summary>
        private static class Parser
        {

        }
    }
}
