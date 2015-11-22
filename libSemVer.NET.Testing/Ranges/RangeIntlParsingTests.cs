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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace McSherry.SemanticVersioning.Ranges
{
    using static VersionRange;

    /// <summary>
    /// <para>
    /// Provides the tests that test the internals of the version
    /// range parser.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class RangeIntlParsingTests
    {
        private const string Category = "Version Range Parsing Internals";

        /// <summary>
        /// <para>
        /// Tests the parser's identification of basic constructs, such as
        /// the basic operators and semantic version strings.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void BasicIdentification()
        {
            // Convenience function to extract the first comparator token
            // from the set we get returned by the parse method.
            var get = new Func<ParseResult, ComparatorToken>(
                x => x.ComparatorSets.First().First()
                );

            // Each operator is tested twice: once with a prefixing 'v',
            // and once without. We assume that, if testing with 'v' works,
            // testing with 'V' would also work.
            //
            // The [Equal] operator is tested four times, as it can be both
            // implicit and explicit in a range string.

            var r0 = Parser.Parse("1.0.0");     // Equal
            var r1 = Parser.Parse("v1.0.0");    // Equal
            var r2 = Parser.Parse("=1.0.0");    // Equal
            var r3 = Parser.Parse("=v1.0.0");   // Equal
            var r4 = Parser.Parse("<1.0.0");    // LessThan
            var r5 = Parser.Parse("<v1.0.0");   // LessThan
            var r6 = Parser.Parse("<=1.0.0");   // LessThanOrEqual
            var r7 = Parser.Parse("<=v1.0.0");  // LessThanOrEqual
            var r8 = Parser.Parse(">1.0.0");    // GreaterThan
            var r9 = Parser.Parse(">v1.0.0");   // GreaterThan
            var r10 = Parser.Parse(">=1.0.0");  // GreaterThanOrEqual
            var r11 = Parser.Parse(">=v1.0.0"); // GreaterThanOrEqual

            Assert.AreEqual(Operator.Equal, get(r0).Operator,
                            "Did not produce expected operator (0).");
            Assert.AreEqual(Operator.Equal, get(r1).Operator,
                            "Did not produce expected operator (1).");
            Assert.AreEqual(Operator.Equal, get(r2).Operator,
                            "Did not produce expected operator (2).");
            Assert.AreEqual(Operator.Equal, get(r3).Operator,
                            "Did not produce expected operator (3).");

            Assert.AreEqual(Operator.LessThan, get(r4).Operator,
                            "Did not produce expected operator (4).");
            Assert.AreEqual(Operator.LessThan, get(r5).Operator,
                            "Did not produce expected operator (5).");

            Assert.AreEqual(Operator.LessThanOrEqual, get(r6).Operator,
                            "Did not produce expected operator (6).");
            Assert.AreEqual(Operator.LessThanOrEqual, get(r7).Operator,
                            "Did not produce expected operator (7).");

            Assert.AreEqual(Operator.GreaterThan, get(r8).Operator,
                            "Did not produce expected operator (8).");
            Assert.AreEqual(Operator.GreaterThan, get(r9).Operator,
                            "Did not produce expected operator (9).");

            Assert.AreEqual(Operator.GreaterThanOrEqual, get(r10).Operator,
                            "Did not produce expected operator (10).");
            Assert.AreEqual(Operator.GreaterThanOrEqual, get(r11).Operator,
                            "Did not produce expected operator (11).");
        }

        /// <summary>
        /// <para>
        /// Tests the parser's ability to parse multiple comparators from
        /// a string.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void MultipleComparatorParsing()
        {
            var cmpSet = Parser.Parse(">1.0.0 <=2.0.0 3.1.5")
                               .ComparatorSets
                               .First()
                               .ToArray();

            Assert.AreEqual(Operator.GreaterThan, cmpSet[0].Operator,
                            "Did not produce expected operator (0).");
            Assert.AreEqual(new SemanticVersion(1, 0), cmpSet[0].Version,
                            "Did not produce expected version (0).");

            Assert.AreEqual(Operator.LessThanOrEqual, cmpSet[1].Operator,
                            "Did not produce expected operator (1).");
            Assert.AreEqual(new SemanticVersion(2, 0), cmpSet[1].Version,
                            "Did not produce expected version (1).");

            Assert.AreEqual(Operator.Equal, cmpSet[2].Operator,
                            "Did not produce expected operator (2).");
            Assert.AreEqual(new SemanticVersion(3,1, 5), cmpSet[2].Version,
                            "Did not produce expected version (2).");
        }
        /// <summary>
        /// <para>
        /// Tests the parser's ability to parse multiple comparator
        /// sets from a string.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void MultipleSetParsing()
        {
            var cmpSetSet = Parser.Parse("=1.0.0 || >2.0.0 || <3.0.0")
                                  .ComparatorSets
                                  .Select(set => set.ToArray())
                                  .ToArray();

            Assert.AreEqual(1, cmpSetSet[0].Length,
                            "Did not produce expected set length (0).");
            Assert.AreEqual(Operator.Equal, cmpSetSet[0][0].Operator,
                            "Did not produce expected operator (0).");
            Assert.AreEqual(new SemanticVersion(1, 0), cmpSetSet[0][0].Version,
                            "Did not produce expected version (0).");

            Assert.AreEqual(1, cmpSetSet[1].Length,
                            "Did not produce expected set length (1).");
            Assert.AreEqual(Operator.GreaterThan, cmpSetSet[1][0].Operator,
                            "Did not produce expected operator (1).");
            Assert.AreEqual(new SemanticVersion(2, 0), cmpSetSet[1][0].Version,
                            "Did not produce expected version (1).");

            Assert.AreEqual(1, cmpSetSet[2].Length,
                            "Did not produce expected set length (2).");
            Assert.AreEqual(Operator.LessThan, cmpSetSet[2][0].Operator,
                            "Did not produce expected operator (2).");
            Assert.AreEqual(new SemanticVersion(3, 0), cmpSetSet[2][0].Version,
                            "Did not produce expected version (2).");
        }

        /// <summary>
        /// <para>
        /// Tests that the parser reports errors correctly.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ParsingErrors()
        {
            // NullString
            Assert.AreEqual(ParseResultType.NullString,
                            Parser.Parse(null).Type,
                            "Did not produce expected result code (0).");
            Assert.AreEqual(ParseResultType.NullString,
                            Parser.Parse("").Type,
                            "Did not produce expected result code (1).");
            Assert.AreEqual(ParseResultType.NullString,
                            Parser.Parse("  \t ").Type,
                            "Did not produce expected result code (2).");

            // InvalidCharacter
            Assert.AreEqual(ParseResultType.InvalidCharacter,
                            Parser.Parse("1.0.0 | 2.0.0").Type,
                            "Did not produce expected result code (3).");

            // EmptySet
            Assert.AreEqual(ParseResultType.EmptySet,
                            Parser.Parse("||").Type,
                            "Did not produce expected result code (4).");
            Assert.AreEqual(ParseResultType.EmptySet,
                            Parser.Parse("1.0.0 ||").Type,
                            "Did not produce expected result code (5).");
            Assert.AreEqual(ParseResultType.EmptySet,
                            Parser.Parse("|| 1.0.0").Type,
                            "Did not produce expected result code (6).");
            Assert.AreEqual(ParseResultType.EmptySet,
                            Parser.Parse("1.0.0 || || 1.2.3").Type,
                            "Did not produce expected result code (7).");

            // OrphanedOperator
            Assert.AreEqual(ParseResultType.OrphanedOperator,
                            Parser.Parse("= 1.0.0").Type,
                            "Did not produce expected result code (8).");
            Assert.AreEqual(ParseResultType.OrphanedOperator,
                            Parser.Parse("> 1.0.0").Type,
                            "Did not produce expected result code (9).");
            Assert.AreEqual(ParseResultType.OrphanedOperator,
                            Parser.Parse("< 1.0.0").Type,
                            "Did not produce expected result code (10).");
            Assert.AreEqual(ParseResultType.OrphanedOperator,
                            Parser.Parse(">= 1.0.0").Type,
                            "Did not produce expected result code (11).");
            Assert.AreEqual(ParseResultType.OrphanedOperator,
                            Parser.Parse("<= 1.0.0").Type,
                            "Did not produce expected result code (12).");

            // InvalidVersion
            Assert.AreEqual(ParseResultType.InvalidVersion,
                            Parser.Parse("1.0.0-früh").Type,
                            "Did not produce expected result code (13).");
        }

        /// <summary>
        /// <para>
        /// Tests that <see cref="ParseResult"/>s correctly recognise
        /// the codes that must not be passed with inner exceptions.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ParseResult_NoExceptionCodes()
        {
            var noex_errors = new[]
            {
                ParseResultType.EmptySet,
                ParseResultType.InvalidCharacter,
                ParseResultType.NullString,
                ParseResultType.OrphanedOperator
            };

            // Make sure they don't throw with the code-only ctor.
            for (int i = 0; i < noex_errors.Length; i++)
            {
                try
                {
                    new ParseResult(noex_errors[i]);
                }
                catch (Exception)
                {
                    Assert.Fail($"Incorrectly threw on code-only ({i}).");
                }
            }

            // Make sure they do throw with the code+exception ctor.
            var ex = new Lazy<Exception>(() => new Exception());
            for (int i = 0; i < noex_errors.Length; i++)
            {
                new Action(() => new ParseResult(noex_errors[i], ex))
                    .AssertThrowsExact<ArgumentException>(
                        $"Incorrectly threw on code+exception ({i}).");
            }
        }
        /// <summary>
        /// <para>
        /// Tests that <see cref="ParseResult"/>s correctly recognise
        /// the codes that must be passed with inner exceptions.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ParseResult_OnlyExceptionCodes()
        {
            var exonly_errors = new[]
            {
                ParseResultType.InvalidVersion
            };

            for (int i = 0; i < exonly_errors.Length; i++)
            {
                new Action(() => new ParseResult(exonly_errors[i]))
                    .AssertThrowsExact<ArgumentException>(
                        $"Failed to throw when expected ({i})."
                        );
            }
        }
        /// <summary>
        /// <para>
        /// General tests related to <see cref="ParseResult"/>s
        /// not covered by the other tests.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ParseResult_General()
        {
            // Make sure that passing in the [Success] status always fails.
            var ex = new Lazy<Exception>(() => new Exception());
            new Action(() => new ParseResult(ParseResultType.Success))
                .AssertThrowsExact<ArgumentException>(
                    "Did not throw when passed [Success] code (0).");
            new Action(() => new ParseResult(ParseResultType.Success, ex))
                .AssertThrowsExact<ArgumentException>(
                    "Did not throw when passed [Success] code (1).");

            // Make sure that an invalid status fails.
            new Action(() => new ParseResult((ParseResultType)(-1)))
                .AssertThrowsExact<ArgumentException>(
                    "Did not throw on invalid status code (0).");
            new Action(() => new ParseResult((ParseResultType)(-1), ex))
                    .AssertThrowsExact<ArgumentException>(
                        "Did not throw on invalid status code (1).");

            // Make sure that a null exception provider fails.
            new Action(
                () => new ParseResult(ParseResultType.InvalidVersion, null))
                .AssertThrows<ArgumentNullException>(
                    "Did not throw on null exception provider.");

            // Tests that accessing the properties of a default-constructed
            // [ParseResult] fails with an exception.
            var defPr = new ParseResult();
            new Action(() => defPr.Type.GetHashCode())
                .AssertThrows<InvalidOperationException>(
                    "Did not throw on invalid access (0).");
            new Action(() => defPr.ComparatorSets.GetHashCode())
                .AssertThrows<InvalidOperationException>(
                    "Did not throw on invalid access (1).");
            new Action(() => defPr.GetErrorMessage())
                .AssertThrows<InvalidOperationException>(
                    "Did not throw on invalid access (2).");
            new Action(() => defPr.CreateException())
                .AssertThrows<InvalidOperationException>(
                    "Did not throw on invalid access (3).");

            // Makes sure that attempting to create an exception/retrieve an
            // error message with a success-state [ParseResult] fails.
            var sucPr = new ParseResult(new ComparatorToken[0][]);
            new Action(() => sucPr.GetErrorMessage())
                .AssertThrows<InvalidOperationException>(
                    "Did not throw on invalid [GetErrorMessage] call.");
            new Action(() => sucPr.CreateException())
                .AssertThrows<InvalidOperationException>(
                    "Did not throw on invalid [CreateException] call.");
        }
    }
}
