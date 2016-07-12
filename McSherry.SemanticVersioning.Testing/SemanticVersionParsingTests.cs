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
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static McSherry.SemanticVersioning.SemanticVersion;
using static McSherry.SemanticVersioning.SemanticVersion.Parser;
using static System.Linq.Enumerable;

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// <para>
    /// Tests the user-exposed methods for parsing <see cref="SemanticVersion"/>
    /// strings.
    /// </para>
    /// </summary>
    [TestClass]
    public class SemanticVersionParsingTests
    {
        private const string Category = "Semantic Version Parsing";

        /// <summary>
        /// <para>
        /// Tests that the <see cref="SemanticVersion"/> from-string explicit
        /// casting operator behaves as expected for valid input.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ExplicitCast_FromString_Valid()
        {
            // The explicit cast should behave identically to [Parse] with all
            // [ParseMode] flags set. 

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

            // Now we need to use equivalents that test that the flags are
            // set.
            string es0 = "V1.0.1",
                   es1 = "v1.10",
                   es2 = "1.0.0-alpha",
                   es3 = "v1.0-alpha.1",
                   es4 = "V1.0-0.3.7",
                   es5 = "1.0-x.7.z.92",
                   es6 = "v1.0+20130313144700",
                   es7 = "V1.0-beta+exp.sha.5114f85";

            var cv0 = (SemanticVersion)es0;
            var cv1 = (SemanticVersion)es1;
            var cv2 = (SemanticVersion)es2;
            var cv3 = (SemanticVersion)es3;
            var cv4 = (SemanticVersion)es4;
            var cv5 = (SemanticVersion)es5;
            var cv6 = (SemanticVersion)es6;
            var cv7 = (SemanticVersion)es7;

            // Now we test equality.
            Assert.AreEqual(sv0, cv0, "Cast produced invalid result (0).");
            Assert.AreEqual(sv1, cv1, "Cast produced invalid result (1).");
            Assert.AreEqual(sv2, cv2, "Cast produced invalid result (2).");
            Assert.AreEqual(sv3, cv3, "Cast produced invalid result (3).");
            Assert.AreEqual(sv4, cv4, "Cast produced invalid result (4).");
            Assert.AreEqual(sv5, cv5, "Cast produced invalid result (5).");
            Assert.AreEqual(sv6, cv6, "Cast produced invalid result (6).");
            Assert.AreEqual(sv7, cv7, "Cast produced invalid result (7).");
        }
        /// <summary>
        /// <para>
        /// Tests that the <see cref="SemanticVersion"/> from-string explicit
        /// cast operator behaves as expected for invalid input.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void ExplicitCast_FromString_Invalid()
        {
            // We're just going to throw some invalid strings at the cast
            // operator and make sure it throws the correct exceptions back
            // at us.

            // These are taken from another unit test.
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
            Type[] exTypes =
            {
                // NullString
                typeof(ArgumentNullException),  // 0
                typeof(ArgumentNullException),  // 1
                typeof(ArgumentNullException),  // 2
                
                // PreTrioInvalidChar
                typeof(FormatException),        // 3
                
                // TrioInvalidChar
                typeof(FormatException),        // 4
                typeof(FormatException),        // 5
                typeof(FormatException),        // 6
                
                // TrioItemLeadingZero
                typeof(FormatException),        // 7
                typeof(FormatException),        // 8
                typeof(FormatException),        // 9

                // TrioItemMissing
                typeof(ArgumentException),      // 10
                typeof(ArgumentException),      // 11
                typeof(ArgumentException),      // 12

                // TrioItemOverflow
                typeof(OverflowException),      // 13

                // IdentifierMissing
                typeof(ArgumentException),      // 14
                typeof(ArgumentException),      // 15

                // IdentifierInvalid
                typeof(FormatException),        // 16
                typeof(FormatException),        // 17

                // MetadataMissing
                typeof(ArgumentException),      // 18
                typeof(ArgumentException),      // 19

                // MetadataInvalid
                typeof(FormatException),        // 20


                // TrioItemMissing [OptionalPatch]
                typeof(ArgumentException),      // 21
                typeof(ArgumentException),      // 22
                typeof(ArgumentException),      // 23
            };

            for (int i = 0; i < exTypes.Length; i++)
            {
                try
                {
                    var x = (SemanticVersion)versionStrings[i];
                }
                catch (InvalidCastException icex)
                {
                    Assert.AreEqual(exTypes[i], icex.InnerException.GetType(),
                                   $"Unexpected [InnerException] type ({i}).");
                }
                catch (Exception)
                {
                    Assert.Fail("Threw unexpected exception type.");
                }
            }
        }
    }
}
