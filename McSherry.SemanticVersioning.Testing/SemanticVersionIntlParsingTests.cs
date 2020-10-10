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
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static McSherry.SemanticVersioning.SemanticVersion;
using static McSherry.SemanticVersioning.SemanticVersion.Parser;
using static System.Linq.Enumerable;
using System.Text;

namespace McSherry.SemanticVersioning
{
    using CompSt = ComponentState;

    /// <summary>
    /// <para>
    /// Tests the internal <see cref="SemanticVersion"/> version string
    /// parser that is not exposed to users.
    /// </para>
    /// </summary>
    [TestClass]
    public class SemanticVersionIntlParsingTests
    {
        private const string Category = "Semantic Version Parsing Internals";

        /// <summary>
        /// <para>
        /// Tests that <see cref="SemanticVersion.MemoizationAgent"/> is used
        /// when assigned to.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Memoize()
        {
            var dict = new Dictionary<string, SemanticVersion>();

            SemanticVersion.MemoizationAgent = dict;

            var sv0 = SemanticVersion.Parse("1.6.0+abc.def");

            Assert.IsTrue(dict.ContainsKey("1.6.0+abc.def"));
            Assert.AreEqual(sv0, dict["1.6.0+abc.def"]);
            Assert.AreEqual(1, dict.Count);


            SemanticVersion.MemoizationAgent = null;

            var sv1 = SemanticVersion.Parse("2.3.1-beta.4");

            Assert.IsFalse(dict.ContainsKey("2.3.1-beta.4"));
            Assert.AreEqual(1, dict.Count);
        }

        /// <summary>
        /// <para>
        /// Tests the <see cref="SemanticVersion"/> parser's internal
        /// normalisation method to ensure it correctly handles whitespace.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Normalisation_WhiteSpace()
        {
            const string Seed = "1.2.3";

            (string VID, string Input)[] vectors =
            {
                ("V1.1", $"\t   {Seed}"),   // Leading whitespace
                ("V1.2", $"{Seed}\t   "),   // Trailing whitespace
                ("V1.3", $"\t {Seed}\t "),  // Leading and trailing
            };

            foreach (var vector in vectors)
            {
                string normalised = vector.Input;

                Assert.AreEqual(
                    expected:   ParseResultType.Success,
                    actual:     Normalise(ref normalised, ParseMode.Strict),
                    message:    $"Failure: Bad return, {vector.VID}"
                    );

                Assert.AreEqual(
                    expected:   Seed,
                    actual:     normalised,
                    message:    $"Failure: Normalisation, {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests that the <see cref="Normalise(ref string, ParseMode)"/> method
        /// responds correctly to bad input in <see cref="ParseMode.Strict"/>
        /// mode.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Normalisation_BadInput()
        {
            const ParseResultType NullString = ParseResultType.NullString;
            const ParseResultType PreTrioInvalidChar = ParseResultType.PreTrioInvalidChar;

            const ParseMode Strict = ParseMode.Strict;

            (string VID, string Input, ParseResultType Expected, ParseMode Mode)[] vectors =
            {
                ("V1.1",    null,           NullString,         Strict),
                ("V1.2",    String.Empty,   NullString,         Strict),
                ("V1.3",    " \t  ",        NullString,         Strict),
                ("V1.4",    "v1.2.3",       PreTrioInvalidChar, Strict),
                ("V1.5",    "V1.2.3",       PreTrioInvalidChar, Strict),
                ("V1.6",    "€1.2.3",       PreTrioInvalidChar, Strict),
            };

            foreach (var vector in vectors)
            {
                var input = vector.Input;

                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     Normalise(ref input, vector.Mode),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
        }
        /// <summary>
        /// <para>
        /// Tests that <see cref="Normalise(ref string, ParseMode)"/> behaves
        /// correctly when passed the <see cref="ParseMode.AllowPrefix"/> flag.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Normalisation_AllowPrefix()
        {
            const ParseResultType Success = ParseResultType.Success;
            const ParseResultType PreTrioInvalidChar = ParseResultType.PreTrioInvalidChar;

            (string VID, string Input, ParseResultType OutputType, string OutputString)[] vectors =
            {
                ("V1.1",    "1.2.3",    Success,            "1.2.3"),
                ("V1.2",    "v1.2.3",   Success,            "1.2.3"),
                ("V1.3",    "V1.2.3",   Success,            "1.2.3"),
                ("V1.4",    "€1.2.3",   PreTrioInvalidChar, null),
            };

            foreach (var vector in vectors)
            {
                string input = vector.Input;

                Assert.AreEqual(
                    expected:   vector.OutputType,
                    actual:     Normalise(ref input, ParseMode.AllowPrefix),
                    message:    $"Failure: Bad status, vector {vector.VID}"
                    );

                // If we're not expecting an error...
                if (vector.OutputString != null)
                {
                    Assert.AreEqual(
                        expected:   vector.OutputString,
                        actual:     input,
                        message:    $"Failure: Normalisation, vector {vector.VID}"
                        );
                }
            }
        }

        /// <summary>
        /// <para>
        /// Tests that the <see cref="ParseResult"/> constructor
        /// accepting an error code works as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ParseResult_ErrorCodes()
        {
            ParseResult
                pr0 = new ParseResult(ParseResultType.NullString),
                pr1 = new ParseResult(ParseResultType.PreTrioInvalidChar),
                pr2 = new ParseResult(ParseResultType.TrioInvalidChar),
                pr3 = new ParseResult(ParseResultType.TrioItemLeadingZero),
                pr4 = new ParseResult(ParseResultType.TrioItemMissing),
                pr5 = new ParseResult(ParseResultType.TrioItemOverflow),
                pr6 = new ParseResult(ParseResultType.IdentifierMissing),
                pr7 = new ParseResult(ParseResultType.IdentifierInvalid),
                pr8 = new ParseResult(ParseResultType.MetadataMissing),
                pr9 = new ParseResult(ParseResultType.MetadataInvalid);

            // The [ParseResult] struct has a method, [CreateException],
            // that will handily create the appropriate exception to throw
            // for us. We want to make sure that it's returning the correct
            // type of exception.
            Assert.IsTrue(pr0.CreateException() is ArgumentNullException,
                          "Incorrect exception type (0).");

            Assert.IsTrue(pr1.CreateException() is FormatException,
                          "Incorrect exception type (1).");
            Assert.IsTrue(pr2.CreateException() is FormatException,
                          "Incorrect exception type (2).");
            Assert.IsTrue(pr3.CreateException() is FormatException,
                          "Incorrect exception type (3).");

            Assert.IsTrue(pr4.CreateException() is ArgumentException,
                          "Incorrect exception type (4).");

            Assert.IsTrue(pr5.CreateException() is OverflowException,
                          "Incorrect exception type (5).");

            Assert.IsTrue(pr6.CreateException() is ArgumentException,
                          "Incorrect exception type (6).");

            Assert.IsTrue(pr7.CreateException() is FormatException,
                          "Incorrect exception type (7).");

            Assert.IsTrue(pr8.CreateException() is ArgumentException,
                          "Incorrect exception type (8).");

            Assert.IsTrue(pr9.CreateException() is FormatException,
                          "Incorrect exception type (9).");

            // Next, we want to make sure that an attempt to create a 
            // fail-state [ParseResult] with the [Success] status fails.
            new Action(() => new ParseResult(ParseResultType.Success))
                .AssertThrowsExact<ArgumentException>(
                    "Attempt to create result with status [Success] did " +
                    "not throw.");

            // Then we want to test that calling [CreateException] on a
            // [ParseResult] without an error code throws an exception.
            new Action(() => new ParseResult(new SemanticVersion(1, 0))
                                .CreateException())
                .AssertThrows<InvalidOperationException>(
                    "Call to [CreateException] with status [Success] did " +
                    "not throw.");

            // And, last but not least, we want to make sure that an
            // invalid error code throws an exception.
            new Action(() => new ParseResult((ParseResultType)(-4025820)))
                .AssertThrowsExact<ArgumentException>(
                    "Did not throw on invalid error code.");
        }
        /// <summary>
        /// <para>
        /// Tests that the <see cref="ParseResult.GetErrorMessage"/> method
        /// works as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ParseResult_ErrorMessages()
        {
            // We're not going to test the contents of the error
            // messages as that would be a pain when changing them,
            // we're just going to test that an error message is produced
            // when we pass in the right parameters.

            // These ones should all produce valid error messages.
            var prt_arr = new[]
            {
                 ParseResultType.NullString,
                 ParseResultType.PreTrioInvalidChar,
                 ParseResultType.TrioInvalidChar,
                 ParseResultType.TrioItemLeadingZero,
                 ParseResultType.TrioItemMissing,
                 ParseResultType.TrioItemOverflow,
                 ParseResultType.IdentifierMissing,
                 ParseResultType.IdentifierInvalid,
                 ParseResultType.MetadataMissing,
                 ParseResultType.MetadataInvalid,
            };

            for (int i = 0; i < prt_arr.Length; i++)
            {
                Assert.IsFalse(String.IsNullOrWhiteSpace(
                                new ParseResult(prt_arr[i]).GetErrorMessage()),
                               $"Did not return valid error message ({i}).");
            }

            // Like we did in our [CreateException] unit test, we want to make
            // sure that calling [GetErrorMessage] on a [ParseResult] without an
            // error message throws.
            new Action(() => new ParseResult(new SemanticVersion(1, 0))
                    .GetErrorMessage())
                .AssertThrows<InvalidOperationException>(
                    "Call to [GetErrorMessage] with status [Success] did " +
                    "not throw.");
        }


        /// <summary>
        /// <para>
        /// Tests that strictly parsing basic <see cref="SemanticVersion"/> strings works
        /// as expected when using valid version strings.
        /// </para>
        /// </summary>
        [DataRow("1.0.1", 1, 0, 1)]
        [DataRow("1.10.0", 1, 10, 0)]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_Basic(string verString, int major, int minor, int patch)
        {
            var pr = Parser.Parse(verString, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.Success, pr.Type);

            var (pMaj, pMin, pPat) = pr.Version;

            Assert.AreEqual(major, pMaj);
            Assert.AreEqual(minor, pMin);
            Assert.AreEqual(patch, pPat);

            Assert.IsFalse(pr.Version.Identifiers.Any());
            Assert.IsFalse(pr.Version.Metadata.Any());
        }

        /// <summary>
        /// Tests that strictly parsing <see cref="SemanticVersion"/> strings with
        /// build metadata and pre-release identifiers works as expected when
        /// using valid version strings.
        /// </summary>
        [DataRow("1.0.0-alpha", new[] { "alpha" }, new string[0])]
        [DataRow("1.0.0-alpha.1", new[] { "alpha", "1" }, new string[0])]
        [DataRow("1.0.0-0.3.7", new[] { "0", "3", "7" }, new string[0])]
        [DataRow("1.0.0-x.7.z.92", new[] { "x", "7", "z", "92" }, new string[0])]
        [DataRow("1.0.0+20130313144700", new string[0], new[] { "20130313144700" })]
        [DataRow("1.0.0-beta+exp.sha.5114f85", new[] { "beta" }, new[] { "exp", "sha", "5114f85" })]
        [DataRow("1.0.0--doublehyphen+-startinghyphen", new[] { "-doublehyphen" }, new[] { "-startinghyphen" })]
        [DataRow("1.0.0-secondhas.-hyphen+secondhas.-hyphen", new[] { "secondhas", "-hyphen" }, new[] { "secondhas", "-hyphen" })]
        [DataRow("1.0.0--.--+-.--", new[] { "-", "--" }, new[] { "-", "--" })]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_MetaAndIDs(string verString, string[] id, string[] meta)
        {
            var pr = Parser.Parse(verString, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.Success, pr.Type);

            var (pMaj, pMin, pPat) = pr.Version;

            Assert.AreEqual(1, pMaj);
            Assert.AreEqual(0, pMin);
            Assert.AreEqual(0, pPat);

            Assert.IsTrue(id.SequenceEqual(pr.Version.Identifiers));
            Assert.IsTrue(meta.SequenceEqual(pr.Version.Metadata));
        }


        /// <summary>
        /// <para>
        /// Ensures that the <see cref="SemanticVersion"/> parser
        /// ignores insignificant whitespace.
        /// </para>
        /// </summary>
        [DataRow("1.0.1")]
        [DataRow("1.10.0")]
        [DataRow("1.0.0-alpha")]
        [DataRow("1.0.0-alpha.1")]
        [DataRow("1.0.0-0.3.7")]
        [DataRow("1.0.0-x.7.z.92")]
        [DataRow("1.0.0+20130313144700")]
        [DataRow("1.0.0-beta+exp.sha.5114f85")]
        [DataRow("1.0.0--.--+-.--")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_IgnoreWhitespace(string verString)
        {
            var basic = Parser.Parse(verString, ParseMode.Strict).Version;
            var padded = Parser.Parse($"\t {verString}\t ", ParseMode.Strict).Version;

            Assert.AreEqual(basic, padded);
        }

        /// <summary>
        /// <para>
        /// Tests that parsing with the <see cref="ParseMode.AllowPrefix"/>
        /// flag works as expected.
        /// </para>
        /// </summary>
        [DataRow("1.0.1")]
        [DataRow("1.10.0")]
        [DataRow("1.0.0-alpha")]
        [DataRow("1.0.0-alpha.1")]
        [DataRow("1.0.0-0.3.7")]
        [DataRow("1.0.0-x.7.z.92")]
        [DataRow("1.0.0+20130313144700")]
        [DataRow("1.0.0-beta+exp.sha.5114f85")]
        [DataRow("1.0.0--.--+-.--")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_AllowPrefix(string verString)
        {
            var basic = Parser.Parse(verString, ParseMode.Strict).Version;

            var prefixed = new[] { $"v{verString}", $"V{verString}" }
                            .Select(s => Parser.Parse(s, ParseMode.AllowPrefix).Version);

            Assert.IsTrue(prefixed.All(v => v == basic));
        }

        /// <summary>
        /// <para>
        /// Tests that the <see cref="Parse(string, ParseMode, out IEnumerator{char})"/>
        /// and <see cref="TryParse(string, ParseMode, out SemanticVersion, out IEnumerator{char})"/>
        /// methods return a <see cref="IEnumerator{T}"/> as expected.
        /// </para>
        /// </summary>
        [DataRow("1.0.0", null, ParseMode.Lenient)]
        [DataRow("1.0", null, ParseMode.Lenient)]
        [DataRow("1.0.0  ", null, ParseMode.Lenient)]
        [DataRow("1.0.0 !", " !", ParseMode.Greedy)]
        [DataRow("1.0 SDF", " SDF", ParseMode.Greedy)]
        [DataRow("1.0.0.0", ".0", ParseMode.Greedy)]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_OutputsIEnumeratorT(string verString, string expected, ParseMode mode)
        {
            IEnumerator<char> parseIter, tryParseIter;

            Parse(verString, mode, out parseIter);
            Assert.IsTrue(TryParse(verString, mode, out _, out tryParseIter));

            var leftovers = new[] { parseIter, tryParseIter }
                .Select(iter =>
                {
                    if (iter is null)
                        return null;


                    var sb = new StringBuilder();

                    do { sb.Append(iter.Current); } while (iter.MoveNext());

                    return sb.ToString();
                });

            Assert.IsTrue(leftovers.All(l => l == expected));
        }

        /// <summary>
        /// <para>
        /// Tests that parsing with the <see cref="ParseMode.OptionalPatch"/>
        /// flag works as expected.
        /// </para>
        /// </summary>
        [DataRow("1.0.1")]
        [DataRow("1.10.0")]
        [DataRow("1.0.0-alpha")]
        [DataRow("1.0.0-alpha.1")]
        [DataRow("1.0.0-0.3.7")]
        [DataRow("1.0.0-x.7.z.92")]
        [DataRow("1.0.0+20130313144700")]
        [DataRow("1.0.0-beta+exp.sha.5114f85")]
        [DataRow("1.0.0--.--+-.--")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_OptionalPatch(string verString)
        {
            var basic = Parser.Parse(verString, ParseMode.Strict).Version;
            var noPatch = Parser.Parse($"{basic:M.mppRRDD}", ParseMode.OptionalPatch).Version;

            Assert.AreEqual(basic, noPatch);
        }

        /// <summary>
        /// <para>
        /// Tests that parsing with the <see cref="InternalModes.OptionalMinor"/>
        /// flag works as intended.
        /// </para>
        /// </summary>
        [DataRow("1", "1.0.0")]
        [DataRow("2", "2.0.0")]
        [DataRow("3.5", "3.5.0")]
        [DataRow("4.7.2", "4.7.2")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_OptionalMinor(string input, string expected)
        {
            // The [OptionalPatch] flag must also be specified for [OptionalMinor] to
            // have any effect.
            const ParseMode mode = ParseMode.OptionalPatch | InternalModes.OptionalMinor;

            var inVer = Parser.Parse(input, mode).Version;
            var expVer = (SemanticVersion)expected;

            Assert.AreEqual(expVer, inVer);
        }

        /// <summary>
        /// Tests that the inclusion of <see cref="InternalModes.OptionalMinor"/> has
        /// not affected validation in other parsing modes.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="mode"></param>
        [DataRow("1", ParseMode.Strict)]
        [DataRow("2", ParseMode.Lenient)]
        [DataRow("3", ParseMode.OptionalPatch)]
        [DataRow("4", InternalModes.OptionalMinor)]
        [DataRow("4..0", InternalModes.OptionalMinor | ParseMode.OptionalPatch)]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_OptionalMinor_Failure(string input, ParseMode mode)
        {
            Assert.ThrowsException<ArgumentException>(() => SemanticVersion.Parse(input, mode));
        }
 
        /// <summary>
        /// Tests that <see cref="SemanticVersion.ParseInfo"/> provides the correct
        /// component states when all components are present.
        /// </summary>
        [DataRow(ParseMode.OptionalPatch)]
        [DataRow(ParseMode.OptionalPatch | InternalModes.OptionalMinor)]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_ComponentStates_AllPresent(ParseMode mode)
        {
            var pi = SemanticVersion.Parse("1.2.3", mode).ParseInfo;

            Assert.AreEqual(ComponentState.Present, pi.MajorState);
            Assert.AreEqual(ComponentState.Present, pi.MinorState);
            Assert.AreEqual(ComponentState.Present, pi.PatchState);
        }

        /// <summary>
        /// Tests that <see cref="SemanticVersion.ParseInfo"/> provides the correct
        /// component states the patch component is omitted.
        /// </summary>
        [DataRow(ParseMode.OptionalPatch)]
        [DataRow(ParseMode.OptionalPatch | InternalModes.OptionalMinor)]
        [DataRow(ParseMode.Lenient)]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_ComponentStates_PatchOmitted(ParseMode mode)
        {
            var pi = SemanticVersion.Parse("1.2", mode).ParseInfo;

            Assert.AreEqual(ComponentState.Present, pi.MajorState);
            Assert.AreEqual(ComponentState.Present, pi.MinorState);
            Assert.AreEqual(ComponentState.Omitted, pi.PatchState);
        }

        /// <summary>
        /// Tests that <see cref="SemanticVersion.ParseInfo"/> provides the correct
        /// component states when the patch and minor components are omitted.
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_ComponentStates_MinorPatchOmitted()
        {
            const ParseMode mode = ParseMode.OptionalPatch | InternalModes.OptionalMinor;

            var pi = SemanticVersion.Parse("5", mode).ParseInfo;

            Assert.AreEqual(ComponentState.Present, pi.MajorState);
            Assert.AreEqual(ComponentState.Omitted, pi.MinorState);
            Assert.AreEqual(ComponentState.Omitted, pi.PatchState);
        }

        /// <summary>
        /// Tests that <see cref="SemanticVersion.ParseInfo"/> provides the correct
        /// component states when the patch component is a wildcard.
        /// </summary>
        [DataRow("6.0.x")]
        [DataRow("6.0.X")]
        [DataRow("6.0.*")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_ComponentStates_PatchWildcard(string input)
        {
            var pi = SemanticVersion.Parse(input, InternalModes.AllowWildcard).ParseInfo;

            Assert.AreEqual(ComponentState.Present, pi.MajorState);
            Assert.AreEqual(ComponentState.Present, pi.MinorState);
            Assert.AreEqual(ComponentState.Wildcard, pi.PatchState);
        }

        /// <summary>
        /// Tests that <see cref="SemanticVersion.ParseInfo"/> provides the correct
        /// component states when the minor and patch components are wildcards.
        /// </summary>
        [DataRow("6.x")]
        [DataRow("6.X")]
        [DataRow("6.*")]
        [DataRow("7.x.x")]
        [DataRow("7.x.X")]
        [DataRow("7.x.*")]
        [DataRow("7.X.x")]
        [DataRow("7.X.X")]
        [DataRow("7.X.*")]
        [DataRow("7.*.x")]
        [DataRow("7.*.X")]
        [DataRow("7.*.*")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_ComponentStates_MinorPatchWildcard(string input)
        {
            var pi = SemanticVersion.Parse(input, InternalModes.AllowWildcard).ParseInfo;

            Assert.AreEqual(ComponentState.Present, pi.MajorState);
            Assert.AreEqual(ComponentState.Wildcard, pi.MinorState);
            Assert.AreEqual(ComponentState.Wildcard, pi.PatchState);
        }

        /// <summary>
        /// Tests that <see cref="SemanticVersion.ParseInfo"/> provides the correct
        /// component states when all components are wildcards.
        /// </summary>
        [DataRow("x")]
        [DataRow("X")]
        [DataRow("*")]
        // All possible 2-wildcard combos, read like a truth table.
        [DataRow("x.x")]
        [DataRow("x.X")]
        [DataRow("x.*")]
        [DataRow("X.x")]
        [DataRow("X.X")]
        [DataRow("X.*")]
        [DataRow("*.x")]
        [DataRow("*.X")]
        [DataRow("*.*")]
        // All possible 3-wildcard combos (3**3 = 27), read like a truth table:
        //
        //      o Left column changes every 3**2 = 9 items
        //      o Middle changes every 3**1 = 3 items
        //      o Right column changes every 3**0 = 1 items
        //
        [DataRow("x.x.x")]
        [DataRow("x.x.X")]
        [DataRow("x.x.*")]
        [DataRow("x.X.x")]
        [DataRow("x.X.X")]
        [DataRow("x.X.*")]
        [DataRow("x.*.x")]
        [DataRow("x.*.X")]
        [DataRow("x.*.*")]
        [DataRow("X.x.x")]
        [DataRow("X.x.X")]
        [DataRow("X.x.*")]
        [DataRow("X.X.x")]
        [DataRow("X.X.X")]
        [DataRow("X.X.*")]
        [DataRow("X.*.x")]
        [DataRow("X.*.X")]
        [DataRow("X.*.*")]
        [DataRow("*.x.x")]
        [DataRow("*.x.X")]
        [DataRow("*.x.*")]
        [DataRow("*.X.x")]
        [DataRow("*.X.X")]
        [DataRow("*.X.*")]
        [DataRow("*.*.x")]
        [DataRow("*.*.X")]
        [DataRow("*.*.*")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_ComponentStates_AllWildcard(string input)
        {
            var pi = SemanticVersion.Parse(input, InternalModes.AllowWildcard).ParseInfo;

            Assert.AreEqual(ComponentState.Wildcard, pi.MajorState);
            Assert.AreEqual(ComponentState.Wildcard, pi.MinorState);
            Assert.AreEqual(ComponentState.Wildcard, pi.PatchState);
        }

        /// <summary>
        /// Tests that the combinations of component states and <see cref="ParseMode"/>s
        /// anticipated to cause an <see cref="ArgumentException"/> do so.
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_ComponentStates_ArgumentException()
        {
            Assert.ThrowsException<ArgumentException>(
                () => SemanticVersion.Parse("5", ParseMode.Lenient)
                );
        }

        /// <summary>
        /// Tests that combinations of component states and <see cref="ParseMode"/>s
        /// anticipated to cause a <see cref="FormatException"/> do so.
        /// </summary>
        [DataRow("1.2.x", ParseMode.Lenient)]
        [DataRow("1.x", ParseMode.Lenient)]
        [DataRow("x", ParseMode.Lenient)]
        [DataRow("1.x.0", InternalModes.AllowWildcard)]
        [DataRow("x.2", InternalModes.AllowWildcard)]
        [DataRow("X.2.3", InternalModes.AllowWildcard)]
        [DataRow("*.*.15", InternalModes.AllowWildcard)]
        // A wildcard with pre-release identifiers or build metadata doesn't really
        // make sense, and 'node-semver' appears to ignore them anyway.
        [DataRow("1.0.x-alpha", InternalModes.AllowWildcard)]
        [DataRow("1.x-beta", InternalModes.AllowWildcard)]
        [DataRow("1.x.x-beta", InternalModes.AllowWildcard)]
        [DataRow("x.x.x-rc", InternalModes.AllowWildcard)]
        [DataRow("X.X-rc", InternalModes.AllowWildcard)]
        [DataRow("*-rc", InternalModes.AllowWildcard)]
        [DataRow("1.0.x+alpha", InternalModes.AllowWildcard)]
        [DataRow("1.x+beta", InternalModes.AllowWildcard)]
        [DataRow("1.x.x+beta", InternalModes.AllowWildcard)]
        [DataRow("x.x.x+rc", InternalModes.AllowWildcard)]
        [DataRow("X.X+rc", InternalModes.AllowWildcard)]
        [DataRow("*+rc", InternalModes.AllowWildcard)]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_ComponentStates_FormatException(string input, ParseMode mode)
        {
            // Regular
            Assert.ThrowsException<FormatException>(
                () => SemanticVersion.Parse(input, mode)
                );

            // When outputing the enumerator
            Assert.ThrowsException<FormatException>(
                () => SemanticVersion.Parse(input, mode, out _)
                );
        }


        /// <summary>
        /// Tests that <see cref="ParseMode.Greedy"/> produces the expected output
        /// when used alone without other modes.
        /// </summary>
        [DataRow("1", "1.0.0")]
        [DataRow("1.", "1.0.0")]
        [DataRow("1    ", "1.0.0")]
        [DataRow("1jhgw", "1.0.0")]
        [DataRow("1.5", "1.5.0")]
        [DataRow("1.5oiugbew", "1.5.0")]
        [DataRow("1.5-", "1.5.0")]
        [DataRow("1.5-abc", "1.5.0")]
        [DataRow("1.5-abc.", "1.5.0")]
        [DataRow("1.5-abc!!£R", "1.5.0")]
        [DataRow("1.5+", "1.5.0")]
        [DataRow("1.5+abc", "1.5.0")]
        [DataRow("1.5+abc.", "1.5.0")]
        [DataRow("1.5+abc!!£R", "1.5.0")]
        [DataRow("1.5.", "1.5.0")]
        [DataRow("1.5.0-", "1.5.0")]
        [DataRow("1.5.0-abc.", "1.5.0-abc")]
        [DataRow("1.5.0-abc&^(", "1.5.0-abc")]
        [DataRow("1.5.0+", "1.5.0")]
        [DataRow("1.5.0+abc.", "1.5.0+abc")]
        [DataRow("1.5.0+abc&($%", "1.5.0+abc")]
        [DataRow("1..1", "1.0.0")]
        [DataRow("1.1.-abc", "1.1.0")]
        [DataRow("1.5.0-ab..cd", "1.5.0-ab")]
        [DataRow("1.5.0+ab..cd", "1.5.0+ab")]
        [DataRow("1.05", "1.0.0")]
        [DataRow("1.05.0", "1.0.0")]
        [DataRow("1.1.05", "1.1.0")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Greedy(string input, string output)
        {
            var greedy = SemanticVersion.Parse(input, ParseMode.Greedy);
            var expected = (SemanticVersion)output;

            Assert.AreEqual(expected, greedy);
        }

        /// <summary>
        /// Tests that <see cref="ParseMode.Greedy"/> produces the expected
        /// output when used in combination with <see cref="ParseMode.OptionalPatch"/>.
        /// </summary>
        [DataRow("1.5-abc", "1.5.0-abc")]
        [DataRow("1.5-abc.", "1.5.0-abc")]
        [DataRow("1.5-abc!!£R", "1.5.0-abc")]
        [DataRow("1.5+abc", "1.5.0+abc")]
        [DataRow("1.5+abc.", "1.5.0+abc")]
        [DataRow("1.5+abc!!£R", "1.5.0+abc")]
        [DataRow("1.5-ab..cd", "1.5.0-ab")]
        [DataRow("1.5+ab..cd", "1.5.0+ab")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Greedy_OptionalPatch(string input, string output)
        {
            var greedy = SemanticVersion.Parse(input, ParseMode.Greedy | ParseMode.OptionalPatch);
            var expected = (SemanticVersion)output;

            Assert.AreEqual(expected, greedy);
        }

        [DataRow("v1", "1.0.0")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Greedy_AllowPrefix(string input, string output)
        {
            var greedy = SemanticVersion.Parse(input, ParseMode.Greedy | ParseMode.AllowPrefix);
            var expected = (SemanticVersion)output;

            Assert.AreEqual(expected, greedy);
        }


        /// <summary>
        /// <para>
        /// Tests that parsing <see cref="SemanticVersion"/> strings works
        /// as expected when the parser is given an invalid string.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_Invalid()
        {
            string[] versionStrings =
            {
                // These ones are testing "Strict" specifically, but
                // should apply to all modes.
                
                // NullString
                null,                   // 0
                String.Empty,           // 1
                " \t ",                 // 2
                
                // PreTrioInvalidChar
                "ẅ1.0.0",               // 3

                // TrioInvalidChar
                "1ñ.0.0",               // 4
                "1.û0.0",               // 5
                "1.0.ç0",               // 6

                // TrioItemLeadingZero
                "01.0.0",               // 7
                "1.00.0",               // 8
                "1.0.00",               // 9

                // TrioItemMissing
                "1.0-rc",               // 10
                "1-rc",                 // 11
                "1..0",                 // 12

                // TrioItemOverflow
                "1.2147483649.0",       // 13

                // IdentifierMissing
                "1.0.0-",               // 14
                "1.0.0-rc.",            // 15

                // IdentifierInvalid
                "1.0.0-öffentlich",     // 16
                "1.0.0-00.2",           // 17

                // MetadataMissing
                "1.0.0+",               // 18
                "1.0.0+a972bae.",       // 19

                // MetadataInvalid
                "1.0.0+schlüssel.534a", // 20


                // These ones are testing [OptionalPatch]-specific
                // behaviour.

                // This specifically makes sure that it won't count
                // a patch omitted but with the period separator
                // present valid.

                // TrioItemMissing
                "1.0.",                 // 21
                "1.0.-rc",              // 22
                "1.0.+a972bae",         // 23
            };
            ParseMode[] modes =
            {
                ParseMode.Strict,       // 0
                ParseMode.Strict,       // 1
                ParseMode.Strict,       // 2
                ParseMode.Strict,       // 3
                ParseMode.Strict,       // 4
                ParseMode.Strict,       // 5
                ParseMode.Strict,       // 6
                ParseMode.Strict,       // 7
                ParseMode.Strict,       // 8
                ParseMode.Strict,       // 9
                ParseMode.Strict,       // 10
                ParseMode.Strict,       // 11
                ParseMode.Strict,       // 12
                ParseMode.Strict,       // 13
                ParseMode.Strict,       // 14
                ParseMode.Strict,       // 15
                ParseMode.Strict,       // 16
                ParseMode.Strict,       // 17
                ParseMode.Strict,       // 18
                ParseMode.Strict,       // 19
                ParseMode.Strict,       // 20

                ParseMode.OptionalPatch,// 21
                ParseMode.OptionalPatch,// 22
                ParseMode.OptionalPatch,// 23
            };
            ParseResultType[] results =
            {
                ParseResultType.NullString,         // 0
                ParseResultType.NullString,         // 1
                ParseResultType.NullString,         // 2
                
                ParseResultType.PreTrioInvalidChar, // 3
                
                ParseResultType.TrioInvalidChar,    // 4
                ParseResultType.TrioInvalidChar,    // 5
                ParseResultType.TrioInvalidChar,    // 6

                ParseResultType.TrioItemLeadingZero,// 7
                ParseResultType.TrioItemLeadingZero,// 8
                ParseResultType.TrioItemLeadingZero,// 9

                ParseResultType.TrioItemMissing,    // 10
                ParseResultType.TrioItemMissing,    // 11
                ParseResultType.TrioItemMissing,    // 12

                ParseResultType.TrioItemOverflow,   // 13

                ParseResultType.IdentifierMissing,  // 14
                ParseResultType.IdentifierMissing,  // 15
                
                ParseResultType.IdentifierInvalid,  // 16
                ParseResultType.IdentifierInvalid,  // 17

                ParseResultType.MetadataMissing,    // 18
                ParseResultType.MetadataMissing,    // 19
                
                ParseResultType.MetadataInvalid,    // 20


                ParseResultType.TrioItemMissing,    // 21
                ParseResultType.TrioItemMissing,    // 22
                ParseResultType.TrioItemMissing,    // 23
            };

            // We can't do the test if we don't have a result for
            // each version string we put in.
            Assert.IsTrue(versionStrings.Length == results.Length,
                          "Unit test incorrectly configured.");

            // Iterate through the test strings and compare them to
            // the expected result.
            for (int i = 0; i < results.Length; i++)
            {
                Assert.AreEqual(
                    results[i], Parser.Parse(versionStrings[i], modes[i]).Type,
                    $"Did not produce expected status ({i})."
                    );
            }
        }

        /// <summary>
        /// <para>
        /// Tests that the regular values of <see cref="ParseMode"/> do not
        /// overlap with the <see cref="InternalModes"/> values.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ParseMode_NoOverlap()
        {
            Assert.IsFalse(
                Enum.GetValues(typeof(ParseMode))
                    .Cast<ParseMode>()
                    .Any(i => (i & InternalModes.Mask) > 0));
        }
    }
}
