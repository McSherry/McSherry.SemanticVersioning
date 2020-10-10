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
        /// <para>
        /// Tests that component states are correctly indicated in parse
        /// metadata provided with a semantic version.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_ComponentStates()
        {
            const ParseMode patch = ParseMode.OptionalPatch;
            const ParseMode both  = patch | InternalModes.OptionalMinor;
            const ParseMode wcard = InternalModes.AllowWildcard;

            const CompSt present = CompSt.Present;
            const CompSt omitted = CompSt.Omitted;
            const CompSt wildcard = CompSt.Wildcard;

            // Tests for valid input
            (
                string VID, 
                string Input, 
                ParseMode Mode, 
                (CompSt Major, CompSt Minor, CompSt Patch) Expected
            )[] vectors1 =
            {
                ("V1.1",  "1.0.0",  patch,              (present, present, present)),
                ("V1.2",  "2.0",    patch,              (present, present, omitted)),
                ("V1.3",  "3.0.0",  both,               (present, present, present)),
                ("V1.4",  "4.0",    both,               (present, present, omitted)),
                ("V1.5",  "5",      both,               (present, omitted, omitted)),
                ("V1.6",  "5.0",    ParseMode.Lenient,  (present, present, omitted)),
                ("V1.7",  "6.0.x",  wcard,              (present, present, wildcard)),
                ("V1.8",  "6.0.X",  wcard,              (present, present, wildcard)),
                ("V1.9",  "6.0.*",  wcard,              (present, present, wildcard)),
                ("V1.10", "6.x",    wcard,              (present, wildcard, wildcard)),
                ("V1.11", "6.X",    wcard,              (present, wildcard, wildcard)),
                ("V1.12", "6.*",    wcard,              (present, wildcard, wildcard)),
                ("V1.13", "x",      wcard,              (wildcard, wildcard, wildcard)),
                ("V1.14", "X",      wcard,              (wildcard, wildcard, wildcard)),
                ("V1.15", "*",      wcard,              (wildcard, wildcard, wildcard)),
                ("V1.16", "x.x.x",  wcard,              (wildcard, wildcard, wildcard)),
                ("V1.17", "x.x",    wcard,              (wildcard, wildcard, wildcard)),
                ("V1.19", "*.*.*",  wcard,              (wildcard, wildcard, wildcard)),
                ("V1.18", "*.*",    wcard,              (wildcard, wildcard, wildcard)),
                ("V1.20", "X.X.X",  wcard,              (wildcard, wildcard, wildcard)),
                ("V1.21", "X.X",    wcard,              (wildcard, wildcard, wildcard)),
                ("V1.22", "x.X.*",  wcard,              (wildcard, wildcard, wildcard)),
                ("V1.23", "7.X.X",  wcard,              (present, wildcard, wildcard)),
                ("V1.24", "7.x.x",  wcard,              (present, wildcard, wildcard)),
                ("V1.25", "7.*.*",  wcard,              (present, wildcard, wildcard)),
                ("V1.26", "7.x.X",  wcard,              (present, wildcard, wildcard)),
                ("V1.27", "7.*.X",  wcard,              (present, wildcard, wildcard)),
                ("V1.28", "7.X.*",  wcard,              (present, wildcard, wildcard)),
            };

            foreach (var vector in vectors1)
            {
                // Appease the unassigned variable checker
                ParseMetadata info = null;

                try
                {
                    info = SemanticVersion.Parse(vector.Input, vector.Mode).ParseInfo;
                }
                catch (Exception ex)
                {
                    Assert.Fail(
                        message:    $"Parse failure: vector {vector.VID}",
                        parameters: ex
                        );
                }

                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     (info.MajorState, info.MinorState, info.PatchState),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for invalid input which should throw [ArgumentException]
            Assert.ThrowsException<ArgumentException>(
                action:     () => SemanticVersion.Parse("5", ParseMode.Lenient),
                message:    $"Failure: vector V2"
                );

            // Tests for invalid input which should throw [FormatException]
            (string VID, string Input, ParseMode mode)[] vectors3 =
            {
                ("V3.1",    "1.2.x",    ParseMode.Lenient),
                ("V3.2",    "1.x",      ParseMode.Lenient),
                ("V3.3",    "x",        ParseMode.Lenient),
                ("V3.4",    "1.x.0",    wcard),
                ("V3.5",    "x.2",      wcard),
                ("V3.6",    "X.2.3",    wcard),
                ("V3.7",    "*.*.15",   wcard),

                // Wildcards with pre-release identifiers or build metadata make
                // no logical sense, and don't appear to work in 'node-semver' anyway
                ("V3.8",    "1.0.x-alpha",  wcard),
                ("V3.9",    "1.x-beta",     wcard),
                ("V3.10",   "1.x.x-beta",   wcard),
                ("V3.11",   "x.x.x-rc",     wcard),
                ("V3.12",   "X.X-rc",       wcard),
                ("V3.13",   "*-rc",         wcard),

                ("V3.14",   "1.0.x+alpha",  wcard),
                ("V3.15",   "1.x+beta",     wcard),
                ("V3.16",   "1.x.x+beta",   wcard),
                ("V3.17",   "x.x.x+rc",     wcard),
                ("V3.18",   "X.X+rc",       wcard),
                ("V3.19",   "*+rc",         wcard),
            };

            foreach (var vector in vectors3)
            {
                Assert.ThrowsException<FormatException>(
                    action:  () => SemanticVersion.Parse(vector.Input, vector.mode),
                    message: $"Failure: vector {vector.VID}"
                    );
            }
        }

        /// <summary>
        /// <para>
        /// Tests that greedy parsing works as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_Greedy()
        {
            // The parser, when in "greedy" mode, will attempt to parse as much
            // as it can. When it encounters an error, it will take whatever it
            // has and try to make that into a valid version.
            //
            // If a valid version can be made, it's returned. Otherwise, whatever
            // error encountered is allowed to propagate upwards.


            // Tests that should produce valid output in greedy mode
            const ParseMode greedy = ParseMode.Greedy;
            const ParseMode prefix = ParseMode.AllowPrefix;
            const ParseMode patch  = ParseMode.OptionalPatch;

            (string VID, string Input, SemanticVersion Output, ParseMode Mode)[] vectors1 =
            {
                ("V1.1",    "1",            (SemanticVersion)"1.0.0",       greedy),
                ("V1.2",    "1.",           (SemanticVersion)"1.0.0",       greedy),
                ("V1.3",    "1    ",        (SemanticVersion)"1.0.0",       greedy),
                ("V1.4",    "1jhgw",        (SemanticVersion)"1.0.0",       greedy),
                ("V1.5",    "1.5",          (SemanticVersion)"1.5.0",       greedy),
                ("V1.6",    "1.5oiugbew",   (SemanticVersion)"1.5.0",       greedy),
                ("V1.7",    "1.5-",         (SemanticVersion)"1.5.0",       greedy),
                ("V1.8",    "1.5-abc",      (SemanticVersion)"1.5.0",       greedy),
                ("V1.9",    "1.5-abc.",     (SemanticVersion)"1.5.0-abc",   greedy | patch),
                ("V1.10",   "1.5-abc!!£R",  (SemanticVersion)"1.5.0-abc",   greedy | patch),
                ("V1.11",   "1.5+",         (SemanticVersion)"1.5.0",       greedy),
                ("V1.12",   "1.5+abc",      (SemanticVersion)"1.5.0",       greedy),
                ("V1.13",   "1.5+abc.",     (SemanticVersion)"1.5.0+abc",   greedy | patch),
                ("V1.14",   "1.5+abc!%$£",  (SemanticVersion)"1.5.0+abc",   greedy | patch),
                ("V1.15",   "1.5.",         (SemanticVersion)"1.5.0",       greedy),
                ("V1.16",   "1.5.0-",       (SemanticVersion)"1.5.0",       greedy),
                ("V1.17",   "1.5.0-abc.",   (SemanticVersion)"1.5.0-abc",   greedy),
                ("V1.18",   "1.5.0-abc&^(", (SemanticVersion)"1.5.0-abc",   greedy),
                ("V1.19",   "1.5.0+",       (SemanticVersion)"1.5.0",       greedy),
                ("V1.20",   "1.5.0+abc.",   (SemanticVersion)"1.5.0+abc",   greedy),
                ("V1.21",   "1.5.0+ab&($%", (SemanticVersion)"1.5.0+ab",    greedy),
                ("V1.22",   "v1",           (SemanticVersion)"1.0.0",       greedy | prefix),
                ("V1.23",   "1..1",         (SemanticVersion)"1.0.0",       greedy),
                ("V1.24",   "1.1.-abc",     (SemanticVersion)"1.1.0",       greedy),
                ("V1.25",   "1.5.0-ab..cd", (SemanticVersion)"1.5.0-ab",    greedy),
                ("V1.26",   "1.5.0+ab..cd", (SemanticVersion)"1.5.0+ab",    greedy),
                ("V1.27",   "1.5-ab..cd",   (SemanticVersion)"1.5.0-ab",    greedy | patch),
                ("V1.28",   "1.5+ab..cd",   (SemanticVersion)"1.5.0+ab",    greedy | patch),
                ("V1.29",   "1.05",         (SemanticVersion)"1.0.0",       greedy),
                ("V1.30",   "1.05.0",       (SemanticVersion)"1.0.0",       greedy),
                ("V1.31",   "1.1.05",       (SemanticVersion)"1.1.0",       greedy),
            };

            foreach (var vector in vectors1)
            {
                SemanticVersion sv = null;

                try
                {
                    sv = SemanticVersion.Parse(vector.Input, vector.Mode);
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Parse failure, vector {vector.VID}:\n\n{ex}");
                }

                Assert.AreEqual(
                    expected:   vector.Output,
                    actual:     sv,
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests that should provoke a [FormatException] in greedy mode with
            // no other flags being specified
            (string VID, string Input)[] vectors2 =
            {
                ("V2.1",    "v1"),
                ("V2.2",    "v1."),
                ("V2.3",    "v1.5"),
                ("V2.4",    "v1.5"),
                ("V2.5",    "v1.5.0"),
                ("V2.6",    "v1.5.0-"),
                ("V2.7",    "i1"),
                ("V2.8",    "i1."),
                ("V2.9",    "i1.5"),
                ("V2.10",   "i1.5."),
                ("V2.11",   "i1.5.0"),
                ("V2.12",   "i1.5.0-"),
                ("V2.13",   ".1.0"),
                ("V2.14",   "01"),
                ("V2.15",   "01.0"),
                ("V2.16",   "01.0.0"),
            };

            foreach (var vector in vectors2)
            {
                Assert.ThrowsException<FormatException>(
                    action:     () => SemanticVersion.Parse(vector.Input, greedy),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests that should provoke an [OverflowException]
            (string VID, string Input)[] vectors3 =
            {
                ("V3.1",    "2147483648"),
                ("V3.2",    "10000000000"),
                ("V3.3",    "1.2147483648"),
                ("V3.4",    "1.1.2147483648"),
            };

            foreach (var vector in vectors3)
            {
                Assert.ThrowsException<OverflowException>(
                    action:     () => SemanticVersion.Parse(vector.Input, greedy),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
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
