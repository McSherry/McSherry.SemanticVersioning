// Copyright (c) 2015-17 Liam McSherry
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace McSherry.SemanticVersioning.Internals
{
    /// <summary>
    /// <para>
    /// Provides tests for the <see cref="PseudoBigInt"/> class.
    /// </para>
    /// </summary>
    [TestClass]
    public class PseudoBigIntTests
    {
        private const string Category = "Pseudo-BigInteger";

        /// <summary>
        /// <para>
        /// Tests that <see cref="PseudoBigInt.Compare(string, string)"/>
        /// throws an <see cref="ArgumentNullException"/> when expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void PseudoBigInt_NullArguments()
        {
            new Action(() => PseudoBigInt.Compare(null, "555"))
                .AssertThrows<ArgumentNullException>(
                    "Did not throw on null subject."
                    );

            new Action(() => PseudoBigInt.Compare("555", null))
                .AssertThrows<ArgumentNullException>(
                    "Did not throw on null against."
                    );

            new Action(() => PseudoBigInt.Compare(null, null))
                .AssertThrows<ArgumentNullException>(
                    "Did not throw on both null."
                    );
        }

        /// <summary>
        /// <para>
        /// Tests that <see cref="PseudoBigInt.Compare(string, string)"/>
        /// throws a <see cref="FormatException"/> when expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void PseudoBigInt_InvalidFormat()
        {
            var badStrings = new[]
            {
                // 0    Zero-length input
                "",
                // 1    Leading zeroes on number
                "01",
                // 2    Leading zeroes on number
                "001",
                // 3    Leading zero on zero
                "00",
                // 4    Invalid character, negative sign
                "-1",
                // 5    Invalid character, close bottom of to numeric range
                "(1",
                // 6    Invalid character, close to top of numeric range
                "@1",
                // 7    Invalid character
                "f1",
                // 8    Invalid character, no numbers
                "uw",
                // 9    Invalid character, decimal point
                "10.2",
                // 10   Invalid character, decimal comma
                "10,2",
                // 11   Invalid character, E notation
                "1e3",
            };

            for (int i = 0; i < badStrings.Length; i++)
            {
                new Action(() => PseudoBigInt.Compare(badStrings[i], "555"))
                    .AssertThrows<FormatException>(
                        $"Did not throw on invalid subject (input #{i})."
                        );

                new Action(() => PseudoBigInt.Compare("555", badStrings[i]))
                    .AssertThrows<FormatException>(
                        $"Did not throw on invalid against (input #{i})."
                        );

                new Action(() => PseudoBigInt.Compare(badStrings[i], badStrings[i]))
                    .AssertThrows<FormatException>(
                        $"Did not throw on both invalid (input #{i})."
                        );
            }
        }

        /// <summary>
        /// <para>
        /// Tests that <see cref="PseudoBigInt.Compare(string, string)"/>
        /// produces the correct result for given inputs.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void PseudoBigInt_Valid()
        {
            var vals = new (string Subject, string Against, bool? Result)[]
            {
                // 0    Basic equality
                ("0", "0", null),
                // 1    Basic equality
                ("127", "127", null),

                // 2    Basic inequality, greater
                ("1", "0", true),
                // 3    Basic inequality, greater
                ("391", "87", true),
                // 4    Basic inequality, greater
                ("2017", "2015", true),

                // 5    Basic inequality, lesser
                ("0", "1", false),
                // 6    Basic inequality, lesser
                ("21", "863", false),
                // 7    Basic inequality, lesser
                ("2015", "2017", false),

                // 8    BigInt equality
                ("36893488147419103232", "36893488147419103232", null),
                // 9    BigInt equality
                ("1180591620717411303424", "1180591620717411303424", null),

                // 10   BigInt inequality, greater
                ("1180591620717411303424", "1180591620717411300000", true),
                // 11   BigInt inequality, greater
                ("1180591620717411303424", "1180591620517411300000", true),
                // 12   BigInt inequality, greater
                ("1180591620717411303424", "5823", true),

                // 13   BigInt inequality, lesser
                ("1180591620717411303424", "37778931862957161709568", false),
                // 14   BigInt inequality, lesser
                ("1180591620717411303424", "1190000000000000000000", false),
                // 15   BigInt inequality, lesser
                ("76392", "37778931862957161709568", false),
            };

            for (int i = 0; i < vals.Length; i++)
            {
                var actualValue = PseudoBigInt.Compare(
                    subject: vals[i].Subject, 
                    against: vals[i].Against
                    );

                Assert.AreEqual(
                    expected:   vals[i].Result,
                    actual:     actualValue,
                    message:    $@"Failed on input #{i}: got ""{actualValue}"", " +
                                $@"expected ""{vals[i].Result}""."
                    );
            }
        }
    }
}
