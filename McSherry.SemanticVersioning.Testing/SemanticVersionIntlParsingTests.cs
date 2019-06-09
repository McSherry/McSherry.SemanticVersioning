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
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static McSherry.SemanticVersioning.SemanticVersion;
using static McSherry.SemanticVersioning.SemanticVersion.Parser;
using static System.Linq.Enumerable;

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
            string control  = "1.2.3",          // No whitespace
                   ws0      = "\t   1.2.3",     // Leading whitespace
                   ws1      = "1.2.3\t   ",     // Trailing whitespace
                   ws2      = "\t 1.2.3\t ";    // Leading and trailing

            // We need to test both the return value, which indicates whether
            // we were successful, and the value of the string after we passed
            // it in, which should have been modified.
            Assert.AreEqual(ParseResultType.Success,
                            Normalise(ref ws0, ParseMode.Strict),
                            "Bad return (0).");
            Assert.AreEqual(control, ws0, "Normalisation failure (0).");

            Assert.AreEqual(ParseResultType.Success, 
                            Normalise(ref ws1, ParseMode.Strict),
                            "Bad return (1).");
            Assert.AreEqual(control, ws1, "Normalisation failure (1).");

            Assert.AreEqual(ParseResultType.Success, 
                            Normalise(ref ws2, ParseMode.Strict),
                            "Bad return (2).");
            Assert.AreEqual(control, ws2, "Normalisation failure (2).");
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
            string bis0     = null,         // Null value
                   bis1     = "",           // Empty
                   bis2     = " \t  ",      // Entirely whitespace
                   // These two are only invalid in [ParseMode.Strict].
                   bis3     = "v1.2.3",     // Invalid character
                   bis4     = "V1.2.3",     // Invalid character
                   // This is always invalid.
                   bis5     = "€1.2.3";     // Invalid character


            Assert.AreEqual(ParseResultType.NullString,
                            Normalise(ref bis0, ParseMode.Strict),
                            "Bad status (0).");
            Assert.AreEqual(ParseResultType.NullString,
                            Normalise(ref bis1, ParseMode.Strict),
                            "Bad status (1).");
            Assert.AreEqual(ParseResultType.NullString,
                            Normalise(ref bis2, ParseMode.Strict),
                            "Bad status (2).");
            Assert.AreEqual(ParseResultType.PreTrioInvalidChar,
                            Normalise(ref bis3, ParseMode.Strict),
                            "Bad status (3).");
            Assert.AreEqual(ParseResultType.PreTrioInvalidChar,
                            Normalise(ref bis4, ParseMode.Strict),
                            "Bad status (4).");
            Assert.AreEqual(ParseResultType.PreTrioInvalidChar,
                            Normalise(ref bis5, ParseMode.Strict),
                            "Bad status (5).");
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
            string control  = "1.2.3",
                   ap0      = "v1.2.3",
                   ap1      = "V1.2.3",
                   ap2      = "€1.2.3",
                   ap3      = control;

            // Normalisation should strip the leading "v" from this.
            Assert.AreEqual(ParseResultType.Success,
                            Normalise(ref ap0, ParseMode.AllowPrefix),
                            "Bad status (0).");
            Assert.AreEqual(control, ap0, "Normalisation failure (0).");

            // Normalisation should strip the leading "V" from this.
            Assert.AreEqual(ParseResultType.Success,
                            Normalise(ref ap1, ParseMode.AllowPrefix),
                            "Bad status (1).");
            Assert.AreEqual(control, ap1, "Normalisation failure (1).");

            // There's an invalid character, so [Normalise] should return
            // an error result instead of one indicating success.
            Assert.AreEqual(ParseResultType.PreTrioInvalidChar,
                            Normalise(ref ap2, ParseMode.AllowPrefix),
                            "Bad status (2).");

            // And, of course, normalisation shouldn't touch version strings
            // that don't need anything done to them.
            Assert.AreEqual(ParseResultType.Success,
                            Normalise(ref ap3, ParseMode.AllowPrefix),
                            "Bad status (3).");
            Assert.AreEqual(control, ap3, "Normalisation failure (3).");
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
        /// Tests that parsing <see cref="SemanticVersion"/> strings works
        /// as expected when using valid version strings.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_Valid()
        {
            // These example strings are all taken from the
            // SemVer spec (2.0.0), so if we can't parse these
            // then something is amiss.
            string vs0 = "1.0.1",
                   vs1 = "1.10.0",
                   vs2 = "1.0.0-alpha",
                   vs3 = "1.0.0-alpha.1",
                   vs4 = "1.0.0-0.3.7",
                   vs5 = "1.0.0-x.7.z.92",
                   vs6 = "1.0.0+20130313144700",
                   vs7 = "1.0.0-beta+exp.sha.5114f85";

            // These won't throw unless something is seriously
            // wrong, so we're quite free to do this.
            ParseResult pr0 = Parser.Parse(vs0, ParseMode.Strict);
            ParseResult pr1 = Parser.Parse(vs1, ParseMode.Strict);
            ParseResult pr2 = Parser.Parse(vs2, ParseMode.Strict);
            ParseResult pr3 = Parser.Parse(vs3, ParseMode.Strict);
            ParseResult pr4 = Parser.Parse(vs4, ParseMode.Strict);
            ParseResult pr5 = Parser.Parse(vs5, ParseMode.Strict);
            ParseResult pr6 = Parser.Parse(vs6, ParseMode.Strict);
            ParseResult pr7 = Parser.Parse(vs7, ParseMode.Strict);

            SemanticVersion
                   sv0 = pr0.Version,
                   sv1 = pr1.Version,
                   sv2 = pr2.Version,
                   sv3 = pr3.Version,
                   sv4 = pr4.Version,
                   sv5 = pr5.Version,
                   sv6 = pr6.Version,
                   sv7 = pr7.Version;

            #region Check status
            Assert.AreEqual(ParseResultType.Success, pr0.Type,
                            "Parse unexpectedly failed (0).");
            Assert.AreEqual(ParseResultType.Success, pr1.Type,
                            "Parse unexpectedly failed (1).");
            Assert.AreEqual(ParseResultType.Success, pr2.Type,
                            "Parse unexpectedly failed (2).");
            Assert.AreEqual(ParseResultType.Success, pr3.Type,
                            "Parse unexpectedly failed (3).");
            Assert.AreEqual(ParseResultType.Success, pr4.Type,
                            "Parse unexpectedly failed (4).");
            Assert.AreEqual(ParseResultType.Success, pr5.Type,
                            "Parse unexpectedly failed (5).");
            Assert.AreEqual(ParseResultType.Success, pr6.Type,
                            "Parse unexpectedly failed (6).");
            Assert.AreEqual(ParseResultType.Success, pr7.Type,
                            "Parse unexpectedly failed (7).");
            #endregion
            #region Check [SemanticVersion] properties
            Assert.AreEqual(1, sv0.Major, "Major version incorrect (0).");
            Assert.AreEqual(0, sv0.Minor, "Minor version incorrect (0).");
            Assert.AreEqual(1, sv0.Patch, "Patch version incorrect (0).");
            Assert.IsTrue(sv0.Identifiers.SequenceEqual(Empty<string>()),
                          "Pre-release identifiers incorrect (0).");
            Assert.IsTrue(sv0.Metadata.SequenceEqual(Empty<string>()),
                          "Build metadata items incorrect (0).");

            Assert.AreEqual(1, sv1.Major, "Major version incorrect (1).");
            Assert.AreEqual(10, sv1.Minor, "Minor version incorrect (1).");
            Assert.AreEqual(0, sv1.Patch, "Patch version incorrect (1).");
            Assert.IsTrue(sv1.Identifiers.SequenceEqual(Empty<string>()),
                          "Pre-release identifiers incorrect (1).");
            Assert.IsTrue(sv1.Metadata.SequenceEqual(Empty<string>()),
                          "Build metadata items incorrect (1).");

            Assert.AreEqual(1, sv2.Major, "Major version incorrect (2).");
            Assert.AreEqual(0, sv2.Minor, "Minor version incorrect (2).");
            Assert.AreEqual(0, sv2.Patch, "Patch version incorrect (2).");
            Assert.IsTrue(sv2.Identifiers.SequenceEqual(new[] { "alpha" }),
                          "Pre-release identifiers incorrect (2).");
            Assert.IsTrue(sv2.Metadata.SequenceEqual(Empty<string>()),
                          "Build metadata items incorrect (2).");

            Assert.AreEqual(1, sv3.Major, "Major version incorrect (3).");
            Assert.AreEqual(0, sv3.Minor, "Minor version incorrect (3).");
            Assert.AreEqual(0, sv3.Patch, "Patch version incorrect (3).");
            Assert.IsTrue(sv3.Identifiers.SequenceEqual(new[] { "alpha", "1" }),
                          "Pre-release identifiers incorrect (3).");
            Assert.IsTrue(sv3.Metadata.SequenceEqual(Empty<string>()),
                          "Build metadata items incorrect (3).");

            Assert.AreEqual(1, sv4.Major, "Major version incorrect (4).");
            Assert.AreEqual(0, sv4.Minor, "Minor version incorrect (4).");
            Assert.AreEqual(0, sv4.Patch, "Patch version incorrect (4).");
            Assert.IsTrue(sv4.Identifiers.SequenceEqual(new[] { "0", "3", "7" }),
                          "Pre-release identifiers incorrect (4).");
            Assert.IsTrue(sv4.Metadata.SequenceEqual(Empty<string>()),
                          "Build metadata items incorrect (4).");

            Assert.AreEqual(1, sv5.Major, "Major version incorrect (5).");
            Assert.AreEqual(0, sv5.Minor, "Minor version incorrect (5).");
            Assert.AreEqual(0, sv5.Patch, "Patch version incorrect (5).");
            Assert.IsTrue(sv5.Identifiers.SequenceEqual(new[] { "x", "7", "z",
                                                                "92" }),
                          "Pre-release identifiers incorrect (5).");
            Assert.IsTrue(sv5.Metadata.SequenceEqual(Empty<string>()),
                          "Build metadata items incorrect (5).");

            Assert.AreEqual(1, sv6.Major, "Major version incorrect (6).");
            Assert.AreEqual(0, sv6.Minor, "Minor version incorrect (6).");
            Assert.AreEqual(0, sv6.Patch, "Patch version incorrect (6).");
            Assert.IsTrue(sv6.Identifiers.SequenceEqual(Empty<string>()),
                          "Pre-release identifiers incorrect (6).");
            Assert.IsTrue(sv6.Metadata.SequenceEqual(new[] { "20130313144700" }),
                          "Build metadata items incorrect (6).");

            Assert.AreEqual(1, sv7.Major, "Major version incorrect (7).");
            Assert.AreEqual(0, sv7.Minor, "Minor version incorrect (7).");
            Assert.AreEqual(0, sv7.Patch, "Patch version incorrect (7).");
            Assert.IsTrue(sv7.Identifiers.SequenceEqual(new[] { "beta" }),
                          "Pre-release identifiers incorrect (7).");
            Assert.IsTrue(sv7.Metadata.SequenceEqual(new[] { "exp", "sha",
                                                             "5114f85" }),
                          "Build metadata items incorrect (7).");
            #endregion
        }
        /// <summary>
        /// <para>
        /// Ensures that the <see cref="SemanticVersion"/> parser
        /// ignores insignificant whitespace.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_Valid_IgnoreWhiteSpace()
        {
            // These versions parsing correctly is tested in another unit
            // test, so we don't have to worry about them.
            SemanticVersion
                sv0 = Parser.Parse("1.0.1", ParseMode.Strict).Version,
                sv1 = Parser.Parse("1.10.0", ParseMode.Strict).Version,
                sv2 = Parser.Parse("1.0.0-alpha", ParseMode.Strict).Version,
                sv3 = Parser.Parse("1.0.0-alpha.1", ParseMode.Strict).Version,
                sv4 = Parser.Parse("1.0.0-0.3.7", ParseMode.Strict).Version,
                sv5 = Parser.Parse("1.0.0-x.7.z.92", ParseMode.Strict).Version,
                sv6 = Parser.Parse("1.0.0+20130313144700",
                                   ParseMode.Strict).Version,
                sv7 = Parser.Parse("1.0.0-beta+exp.sha.5114f85", 
                                   ParseMode.Strict).Version;

            // Leading and trailing whitespace is not significant, and
            // analogous methods (e.g. [Int32.Parse]) ignore it, so we want
            // to make sure that we ignore it, too.
            var ws0 = "\t " + sv0.ToString() + "\t ";
            var ws1 = "\t " + sv1.ToString() + "\t ";
            var ws2 = "\t " + sv2.ToString() + "\t ";
            var ws3 = "\t " + sv3.ToString() + "\t ";
            var ws4 = "\t " + sv4.ToString() + "\t ";
            var ws5 = "\t " + sv5.ToString() + "\t ";
            var ws6 = "\t " + sv6.ToString() + "\t ";
            var ws7 = "\t " + sv7.ToString() + "\t ";

            var wspr0 = Parser.Parse(ws0, ParseMode.Strict);
            var wspr1 = Parser.Parse(ws1, ParseMode.Strict);
            var wspr2 = Parser.Parse(ws2, ParseMode.Strict);
            var wspr3 = Parser.Parse(ws3, ParseMode.Strict);
            var wspr4 = Parser.Parse(ws4, ParseMode.Strict);
            var wspr5 = Parser.Parse(ws5, ParseMode.Strict);
            var wspr6 = Parser.Parse(ws6, ParseMode.Strict);
            var wspr7 = Parser.Parse(ws7, ParseMode.Strict);

            Assert.AreEqual(sv0, wspr0.Version,
                            "Parser did not ignore whitespace (0).");
            Assert.AreEqual(sv1, wspr1.Version,
                            "Parser did not ignore whitespace (1).");
            Assert.AreEqual(sv2, wspr2.Version,
                            "Parser did not ignore whitespace (2).");
            Assert.AreEqual(sv3, wspr3.Version,
                            "Parser did not ignore whitespace (3).");
            Assert.AreEqual(sv4, wspr4.Version,
                            "Parser did not ignore whitespace (4).");
            Assert.AreEqual(sv5, wspr5.Version,
                            "Parser did not ignore whitespace (5).");
            Assert.AreEqual(sv6, wspr6.Version,
                            "Parser did not ignore whitespace (6).");
            Assert.AreEqual(sv7, wspr7.Version,
                            "Parser did not ignore whitespace (7).");
        }
        /// <summary>
        /// <para>
        /// Tests that parsing with the <see cref="ParseMode.AllowPrefix"/>
        /// flag works as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_Valid_AllowPrefixFlag()
        {
            // These versions parsing correctly is tested in another unit
            // test, so we don't have to worry about them.
            string vs0 = "1.0.1",
                   vs1 = "1.10.0",
                   vs2 = "1.0.0-alpha",
                   vs3 = "1.0.0-alpha.1",
                   vs4 = "1.0.0-0.3.7",
                   vs5 = "1.0.0-x.7.z.92",
                   vs6 = "1.0.0+20130313144700",
                   vs7 = "1.0.0-beta+exp.sha.5114f85";

            // These won't throw unless something is seriously
            // wrong, so we're quite free to do this.
            var sv0 = Parser.Parse(vs0, ParseMode.Strict).Version;
            var sv1 = Parser.Parse(vs1, ParseMode.Strict).Version;
            var sv2 = Parser.Parse(vs2, ParseMode.Strict).Version;
            var sv3 = Parser.Parse(vs3, ParseMode.Strict).Version;
            var sv4 = Parser.Parse(vs4, ParseMode.Strict).Version;
            var sv5 = Parser.Parse(vs5, ParseMode.Strict).Version;
            var sv6 = Parser.Parse(vs6, ParseMode.Strict).Version;
            var sv7 = Parser.Parse(vs7, ParseMode.Strict).Version;

            // The prefix may be either "v" or "V", so we need to test
            // that both are accepted.
            #region Lowercase
            var psv0 = Parser.Parse("v" + vs0, ParseMode.AllowPrefix).Version;
            var psv1 = Parser.Parse("v" + vs1, ParseMode.AllowPrefix).Version;
            var psv2 = Parser.Parse("v" + vs2, ParseMode.AllowPrefix).Version;
            var psv3 = Parser.Parse("v" + vs3, ParseMode.AllowPrefix).Version;
            var psv4 = Parser.Parse("v" + vs4, ParseMode.AllowPrefix).Version;
            var psv5 = Parser.Parse("v" + vs5, ParseMode.AllowPrefix).Version;
            var psv6 = Parser.Parse("v" + vs6, ParseMode.AllowPrefix).Version;
            var psv7 = Parser.Parse("v" + vs7, ParseMode.AllowPrefix).Version;
            
            Assert.AreEqual(sv0, psv0, "Prefix parsing unexpectedly failed (0).");
            Assert.AreEqual(sv1, psv1, "Prefix parsing unexpectedly failed (1).");
            Assert.AreEqual(sv2, psv2, "Prefix parsing unexpectedly failed (2).");
            Assert.AreEqual(sv3, psv3, "Prefix parsing unexpectedly failed (3).");
            Assert.AreEqual(sv4, psv4, "Prefix parsing unexpectedly failed (4).");
            Assert.AreEqual(sv5, psv5, "Prefix parsing unexpectedly failed (5).");
            Assert.AreEqual(sv6, psv6, "Prefix parsing unexpectedly failed (6).");
            Assert.AreEqual(sv7, psv7, "Prefix parsing unexpectedly failed (7).");
            #endregion
            #region Uppercase
            var uv0 = Parser.Parse("V" + vs0, ParseMode.AllowPrefix).Version;
            var uv1 = Parser.Parse("V" + vs1, ParseMode.AllowPrefix).Version;
            var uv2 = Parser.Parse("V" + vs2, ParseMode.AllowPrefix).Version;
            var uv3 = Parser.Parse("V" + vs3, ParseMode.AllowPrefix).Version;
            var uv4 = Parser.Parse("V" + vs4, ParseMode.AllowPrefix).Version;
            var uv5 = Parser.Parse("V" + vs5, ParseMode.AllowPrefix).Version;
            var uv6 = Parser.Parse("V" + vs6, ParseMode.AllowPrefix).Version;
            var uv7 = Parser.Parse("V" + vs7, ParseMode.AllowPrefix).Version;

            Assert.AreEqual(sv0, uv0, "Prefix parsing unexpectedly failed (8).");
            Assert.AreEqual(sv1, uv1, "Prefix parsing unexpectedly failed (9).");
            Assert.AreEqual(sv2, uv2, "Prefix parsing unexpectedly failed (10).");
            Assert.AreEqual(sv3, uv3, "Prefix parsing unexpectedly failed (11).");
            Assert.AreEqual(sv4, uv4, "Prefix parsing unexpectedly failed (12).");
            Assert.AreEqual(sv5, uv5, "Prefix parsing unexpectedly failed (13).");
            Assert.AreEqual(sv6, uv6, "Prefix parsing unexpectedly failed (14).");
            Assert.AreEqual(sv7, uv7, "Prefix parsing unexpectedly failed (15).");
            #endregion
        }
        /// <summary>
        /// <para>
        /// Tests that parsing with the <see cref="ParseMode.OptionalPatch"/>
        /// flag works as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_Valid_OptionalPatchFlag()
        {
            // These versions parsing correctly is tested in another unit
            // test, so we don't have to worry about them.
            string vs0 = "1.0.1",
                   vs1 = "1.10.0",
                   vs2 = "1.0.0-alpha",
                   vs3 = "1.0.0-alpha.1",
                   vs4 = "1.0.0-0.3.7",
                   vs5 = "1.0.0-x.7.z.92",
                   vs6 = "1.0.0+20130313144700",
                   vs7 = "1.0.0-beta+exp.sha.5114f85";

            // These won't throw unless something is seriously
            // wrong, so we're quite free to do this.
            var sv0 = Parser.Parse(vs0, ParseMode.Strict).Version;
            var sv1 = Parser.Parse(vs1, ParseMode.Strict).Version;
            var sv2 = Parser.Parse(vs2, ParseMode.Strict).Version;
            var sv3 = Parser.Parse(vs3, ParseMode.Strict).Version;
            var sv4 = Parser.Parse(vs4, ParseMode.Strict).Version;
            var sv5 = Parser.Parse(vs5, ParseMode.Strict).Version;
            var sv6 = Parser.Parse(vs6, ParseMode.Strict).Version;
            var sv7 = Parser.Parse(vs7, ParseMode.Strict).Version;

            string ofvs0 = "1.0.1",
                   ofvs1 = "1.10",
                   ofvs2 = "1.0-alpha",
                   ofvs3 = "1.0-alpha.1",
                   ofvs4 = "1.0-0.3.7",
                   ofvs5 = "1.0-x.7.z.92",
                   ofvs6 = "1.0+20130313144700",
                   ofvs7 = "1.0-beta+exp.sha.5114f85";

            Assert.AreEqual(sv0,
                            Parser.Parse(ofvs0, ParseMode.OptionalPatch).Version,
                            "[OptionalPatch] parsing unexpectedly failed (0).");

            Assert.AreEqual(sv1,
                            Parser.Parse(ofvs1, ParseMode.OptionalPatch).Version,
                            "[OptionalPatch] parsing unexpectedly failed (1).");

            Assert.AreEqual(sv2,
                            Parser.Parse(ofvs2, ParseMode.OptionalPatch).Version,
                            "[OptionalPatch] parsing unexpectedly failed (2).");

            Assert.AreEqual(sv3,
                            Parser.Parse(ofvs3, ParseMode.OptionalPatch).Version,
                            "[OptionalPatch] parsing unexpectedly failed (3).");

            Assert.AreEqual(sv4,
                            Parser.Parse(ofvs4, ParseMode.OptionalPatch).Version,
                            "[OptionalPatch] parsing unexpectedly failed (4).");

            Assert.AreEqual(sv5,
                            Parser.Parse(ofvs5, ParseMode.OptionalPatch).Version,
                            "[OptionalPatch] parsing unexpectedly failed (5).");

            Assert.AreEqual(sv6,
                            Parser.Parse(ofvs6, ParseMode.OptionalPatch).Version,
                            "[OptionalPatch] parsing unexpectedly failed (6).");

            Assert.AreEqual(sv7,
                            Parser.Parse(ofvs7, ParseMode.OptionalPatch).Version,
                            "[OptionalPatch] parsing unexpectedly failed (7).");
        }

        /// <summary>
        /// <para>
        /// Tests that parsing with the <see cref="InternalModes.OptionalMinor"/>
        /// flag works as intended.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Parse_OptionalMinorFlag()
        {
            // Tests for valid inputs
            (string VID, string Input, SemanticVersion Expected)[] vectors1 =
            {
                ("V1.1", "1",       (SemanticVersion)"1.0.0"),
                ("V1.2", "2",       (SemanticVersion)"2.0.0"),
                ("V1.3", "3.5",     (SemanticVersion)"3.5.0"),
                ("V1.4", "4.7.2",   (SemanticVersion)"4.7.2"),
            };

            foreach (var vector in vectors1)
            {
                const ParseMode mode = InternalModes.OptionalMinor |
                                       ParseMode.OptionalPatch;

                Assert.AreEqual(
                    expected:   vector.Expected,
                    actual:     SemanticVersion.Parse(vector.Input, mode),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }


            // Tests for invalid inputs that should throw [ArgumentException]
            (string VID, string Input, ParseMode Mode)[] vectors2 =
            {
                ("V2.1", "1", ParseMode.Strict),
                ("V2.2", "2", ParseMode.Lenient),
                ("V2.3", "3", ParseMode.OptionalPatch),
                ("V2.4", "4", InternalModes.OptionalMinor),
                ("V2.5", "4..0", InternalModes.OptionalMinor | ParseMode.OptionalPatch),
            };

            foreach (var vector in vectors2)
            {
                Assert.ThrowsException<ArgumentException>(
                    action:     () => SemanticVersion.Parse(vector.Input, vector.Mode),
                    message:    $"Failure: vector {vector.VID}"
                    );
            }
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
            };

            foreach (var vector in vectors1)
            {
                var info = SemanticVersion.Parse(vector.Input, vector.Mode).ParseInfo;

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
                ("V3.6",    "x.2.3",    wcard),

                // Wildcards with pre-release identifiers make no logical sense,
                // and don't appear to work in 'node-semver' anyway
                ("V3.7",    "1.0.x-alpha",  wcard),
                ("V3.8",    "1.x-beta",     wcard),
                ("V3.9",    "x-rc",         wcard),
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
