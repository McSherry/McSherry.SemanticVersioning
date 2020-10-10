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
using System.Text;

namespace McSherry.SemanticVersioning
{
    using Parser            = SemanticVersion.Parser;
    using ParseResult       = SemanticVersion.ParseResult;
    using ParseResultType   = SemanticVersion.ParseResultType;
    using InternalModes     = SemanticVersion.InternalModes;
    using ComponentState    = SemanticVersion.ComponentState;


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
        [DataRow("\t   {0}")]
        [DataRow("{0}\t   ")]
        [DataRow("\t {0}\t ")]
        [DataTestMethod, TestCategory(Category)]
        public void Normalise(string format)
        {
            var seed = "1.2.3";
            var formatted = String.Format(format, seed);

            var seedRes = Parser.Normalise(ref seed, ParseMode.Strict);
            var fmtRes = Parser.Normalise(ref formatted, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.Success, seedRes);
            Assert.AreEqual(ParseResultType.Success, fmtRes);

            Assert.AreEqual(seed, formatted);
        }

        [DataRow(null)]
        [DataRow("")]
        [DataRow(" \t  ")]
        [DataTestMethod, TestCategory(Category)]
        public void Normalise_NullString(string input)
        {
            var res = Parser.Normalise(ref input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.NullString, res);
        }

        [DataRow("v1.2.3")]
        [DataRow("V1.2.3")]
        [DataRow("€1.2.3")]
        [DataTestMethod, TestCategory(Category)]
        public void Normalise_PreTrioInvalidChar(string input)
        {
            var res = Parser.Normalise(ref input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.PreTrioInvalidChar, res);
        }

        [DataRow("{0}")]
        [DataRow("v{0}")]
        [DataRow("V{0}")]
        [DataTestMethod, TestCategory(Category)]
        public void Normalise_AllowPrefix(string format)
        {
            const string Seed = "1.2.3";

            var formatted = String.Format(format, Seed);

            var res = Parser.Normalise(ref formatted, ParseMode.AllowPrefix);

            Assert.AreEqual(ParseResultType.Success, res);
            Assert.AreEqual(Seed, formatted);
        }

        [DataRow("€1.2.3")]
        [DataTestMethod, TestCategory(Category)]
        public void Normalise_AllowPrefix_PreTrioInvalidChar(string input)
        {
            var res = Parser.Normalise(ref input, ParseMode.AllowPrefix);

            Assert.AreEqual(ParseResultType.PreTrioInvalidChar, res);
        }


        [TestMethod, TestCategory(Category)]
        public void ParseResult_Ctor_FailOnSuccessCode()
        {
            Assert.ThrowsException<ArgumentException>(
                () => new ParseResult(error: ParseResultType.Success)
                );
        }

        [TestMethod, TestCategory(Category)]
        public void ParseResult_CreateException_FailOnSuccess()
        {
            Assert.ThrowsException<InvalidOperationException>(
                new ParseResult((SemanticVersion)"1.0").CreateException
                );
        }

        [DataRow(ParseResultType.NullString)]
        [DataTestMethod, TestCategory(Category)]
        public void ParseResult_CreateException_ArgumentNullException(int error)
        {
            var pr = new ParseResult((ParseResultType)error);

            var ex = pr.CreateException();

            Assert.IsInstanceOfType(ex, typeof(ArgumentNullException));
        }

        [DataRow(ParseResultType.NullString)]
        [DataRow(ParseResultType.TrioItemMissing)]
        [DataRow(ParseResultType.IdentifierMissing)]
        [DataRow(ParseResultType.MetadataMissing)]
        [DataTestMethod, TestCategory(Category)]
        public void ParseResult_CreateException_ArgumentException(int error)
        {
            var pr = new ParseResult((ParseResultType)error);

            var ex = pr.CreateException();

            Assert.IsInstanceOfType(ex, typeof(ArgumentException));
        }

        [DataRow(ParseResultType.PreTrioInvalidChar)]
        [DataRow(ParseResultType.TrioInvalidChar)]
        [DataRow(ParseResultType.TrioItemLeadingZero)]
        [DataRow(ParseResultType.IdentifierInvalid)]
        [DataRow(ParseResultType.MetadataInvalid)]
        [DataTestMethod, TestCategory(Category)]
        public void ParseResult_CreateException_FormatException(int error)
        {
            var pr = new ParseResult((ParseResultType)error);

            var ex = pr.CreateException();

            Assert.IsInstanceOfType(ex, typeof(FormatException));
        }

        [DataRow(ParseResultType.TrioItemOverflow)]
        [DataTestMethod, TestCategory(Category)]
        public void ParseResult_CreateException_OverflowException(int error)
        {
            var pr = new ParseResult((ParseResultType)error);

            var ex = pr.CreateException();

            Assert.IsInstanceOfType(ex, typeof(OverflowException));
        }


        [TestMethod, TestCategory(Category)]
        public void ParseResult_GetErrorMessage()
        {
            // Testing the actual contents of error messages makes things a bit
            // brittle, so we just test that every non-success result type produces
            // a non-null, non-empty, non-whitespace error message.

            var failureResults = Enum.GetValues(typeof(ParseResultType))
                                     .Cast<ParseResultType>()
                                     .Where(prt => prt != ParseResultType.Success)
                                     .Select(prt => new ParseResult(prt));

            Assert.IsFalse(
                failureResults.Any(pr => String.IsNullOrWhiteSpace(pr.GetErrorMessage()))
                );
        }

        [TestMethod, TestCategory(Category)]
        public void ParseResult_GetErrorMessage_FailOnSuccess()
        {
            Assert.ThrowsException<InvalidOperationException>(
                new ParseResult((SemanticVersion)"1.0").GetErrorMessage
                );
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

        [DataRow(null)]
        [DataRow("")]
        [DataRow("  \t  ")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_NullString(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.NullString, pi.Type);
        }

        [DataRow("ẅ1.0.0")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_PreTrioInvalidChar(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.PreTrioInvalidChar, pi.Type);
        }

        [DataRow("1ñ.0.0")]
        [DataRow("1.û0.0")]
        [DataRow("1.0.ç0")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_TrioInvalidChar(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.TrioInvalidChar, pi.Type);
        }

        [DataRow("01.0.0")]
        [DataRow("1.00.0")]
        [DataRow("1.0.00")]
        public void Parse_Strict_TrioItemLeadingZero(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.TrioItemLeadingZero, pi.Type);
        }

        [DataRow("1.0")]
        [DataRow("1.0-rc")]
        [DataRow("1.0+rc")]
        [DataRow("1")]
        [DataRow("1-rc")]
        [DataRow("1+rc")]
        [DataRow("1..0")]
        [DataRow("1.0.")]
        [DataRow("1.0.-rc")]
        [DataRow("1.0.+rc")]
        public void Parse_Strict_TrioItemMissing(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.TrioItemMissing, pi.Type);
        }

        [DataRow("2147483649.0.0")]
        [DataRow("1.2147483649.0")]
        [DataRow("1.1.2147483649")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_TrioItemOverflow(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.TrioItemOverflow, pi.Type);
        }

        [DataRow("1.0.0-")]
        [DataRow("1.0.0-rc.")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_IdentifierMissing(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.IdentifierMissing, pi.Type);
        }

        [DataRow("1.0.0-öffentlich")]
        [DataRow("1.0.0-00.2")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_IdentifierInvalid(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.IdentifierInvalid, pi.Type);
        }

        [DataRow("1.0.0+")]
        [DataRow("1.0.0+a972bae.")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_MetadataMissing(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.MetadataMissing, pi.Type);
        }

        [DataRow("1.0.0+schlüssel.534a")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Strict_MetadataInvalid(string input)
        {
            var pi = Parser.Parse(input, ParseMode.Strict);

            Assert.AreEqual(ParseResultType.MetadataInvalid, pi.Type);
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

            SemanticVersion.Parse(verString, mode, out parseIter);

            Assert.IsTrue(SemanticVersion.TryParse(verString, mode, out _, out tryParseIter));

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

        [DataRow("1.0.", ParseResultType.TrioItemMissing)]
        [DataRow("1.0.-rc", ParseResultType.TrioItemMissing)]
        [DataRow("1.0.+a972bae", ParseResultType.TrioItemMissing)]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_OptionalPatch_Failure(string input, int result)
        {
            var pi = Parser.Parse(input, ParseMode.OptionalPatch);

            // The [ParseResultType] enum isn't publicly accessible and the test
            // runner seems to ignore non-public tests, so a cast is the workaround.
            Assert.AreEqual((ParseResultType)result, pi.Type);
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

        [DataRow("v1")]
        [DataTestMethod, TestCategory(Category)]
        public void Parse_Greedy_FormatException(string input)
        {
            Assert.ThrowsException<FormatException>(
                () => SemanticVersion.Parse(input, ParseMode.Greedy)
                );
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
