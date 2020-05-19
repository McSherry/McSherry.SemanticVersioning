// Copyright (c) 2020 Liam McSherry
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

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// Tests the methods provided for formatting a <see cref="SemanticVersion"/>
    /// as a <see cref="string"/>.
    /// </summary>
    [TestClass]
    public class SemanticVersionFormatTests
    {
        private const string Category = "Semantic Version Formatting";

        // A version string with all components included
        private const string Str_AllComponents = "1.7.0-alpha.2+20150925.f8f2cb1a";
        // A version string with only pre-release identifiers
        private const string Str_Prerelease = "2.0.1-rc.1";
        // A version string with only metadata items
        private const string Str_Metadata = "1.7.0+20150925.f8f2cb1a";
        // A version string without any additional components, with a zero patch
        private const string Str_Basic_NoPatch = "1.7.0";
        // A version string without any additional components, with a non-zero patch
        private const string Str_Basic_Patch = "2.0.1";


        // ===== Standard Format Specifiers ===== //

        public void StandardSpecifierValues()
        {
#pragma warning disable 618

            Assert.AreEqual("G", SemanticVersionFormat.Default);
            Assert.AreEqual("g", SemanticVersionFormat.PrefixedDefault);

            Assert.AreEqual("C", SemanticVersionFormat.Concise);
            Assert.AreEqual("c", SemanticVersionFormat.PrefixedConcise);
            Assert.AreEqual("C", SemanticVersionFormat.Monotonic);
            Assert.AreEqual("c", SemanticVersionFormat.PrefixedMonotonic);

#pragma warning restore 618
        }

        [DataRow(Str_AllComponents, Str_AllComponents)]
        [DataRow(Str_Prerelease, Str_Prerelease)]
        [DataRow(Str_Metadata, Str_Metadata)]
        [DataRow(Str_Basic_NoPatch, Str_Basic_NoPatch)]
        [DataRow(Str_Basic_Patch, Str_Basic_Patch)]
        [DataTestMethod]
        public void Standard_General(string version, string output)
        {
            var v = (SemanticVersion)version;

            Assert.AreEqual(output, v.ToString(), "Unspecified (default) format");
            Assert.AreEqual(output, v.ToString("G"), "Explicit general specifier");
            Assert.AreEqual(output, v.ToString(""), "Implicit general specifier, empty");
            Assert.AreEqual(output, v.ToString(null), "Implicit general specifier, null");
        }

        [DataRow(Str_AllComponents, "v" + Str_AllComponents)]
        [DataRow(Str_Prerelease, "v" + Str_Prerelease)]
        [DataRow(Str_Metadata, "v" + Str_Metadata)]
        [DataRow(Str_Basic_NoPatch, "v" + Str_Basic_NoPatch)]
        [DataRow(Str_Basic_Patch, "v" + Str_Basic_Patch)]
        [DataTestMethod]
        public void Standard_PrefixedGeneral(string version, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString("g"));
        }

        [DataRow(Str_AllComponents, "1.7-alpha.2")]
        [DataRow(Str_Prerelease, Str_Prerelease)]
        [DataRow(Str_Metadata, "1.7")]
        [DataRow(Str_Basic_NoPatch, "1.7")]
        [DataRow(Str_Basic_Patch, "2.0.1")]
        [DataTestMethod]
        public void Standard_Concise(string version, string output)
        {
            var v = (SemanticVersion)version;

            Assert.AreEqual(output, v.ToString("C"));
        }

        [DataRow(Str_AllComponents, "v1.7-alpha.2")]
        [DataRow(Str_Prerelease, "v" + Str_Prerelease)]
        [DataRow(Str_Metadata, "v1.7")]
        [DataRow(Str_Basic_NoPatch, "v1.7")]
        [DataRow(Str_Basic_Patch, "v2.0.1")]
        [DataTestMethod]
        public void Standard_PrefixedConcise(string version, string output)
        {
            var v = (SemanticVersion)version;

            Assert.AreEqual(output, v.ToString("c"));
        }


        // ===== Custom Format Specifiers ===== //

        [DataRow(Str_AllComponents, "1")]
        [DataRow(Str_Prerelease, "2")]
        [DataRow(Str_Metadata, "1")]
        [DataRow(Str_Basic_NoPatch, "1")]
        [DataRow(Str_Basic_Patch, "2")]
        [DataTestMethod]
        public void Custom_Major(string version, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString("M"));
        }

        [DataRow(Str_AllComponents, "7")]
        [DataRow(Str_Prerelease, "0")]
        [DataRow(Str_Metadata, "7")]
        [DataRow(Str_Basic_NoPatch, "7")]
        [DataRow(Str_Basic_Patch, "0")]
        [DataTestMethod]
        public void Custom_Minor(string version, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString("m"));
        }

        [DataRow(Str_AllComponents, "0")]
        [DataRow(Str_Prerelease, "1")]
        [DataRow(Str_Metadata, "0")]
        [DataRow(Str_Basic_NoPatch, "0")]
        [DataRow(Str_Basic_Patch, "1")]
        [DataTestMethod]
        public void Custom_Patch(string version, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString("p"));
        }

        [DataRow(Str_AllComponents, "")]
        [DataRow(Str_Prerelease, ".1")]
        [DataRow(Str_Metadata, "")]
        [DataRow(Str_Basic_NoPatch, "")]
        [DataRow(Str_Basic_Patch, ".1")]
        [DataTestMethod]
        public void Custom_OptionalPatch(string version, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString("pp"));
        }

        [DataRow(Str_AllComponents, "-alpha.2")]
        [DataRow(Str_Prerelease, "-rc.1")]
        [DataRow(Str_Metadata, "")]
        [DataRow(Str_Basic_NoPatch, "")]
        [DataRow(Str_Basic_Patch, "")]
        [DataTestMethod]
        public void Custom_PrefixedPrereleaseGroup(string version, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString("RR"));
        }

        [DataRow(Str_AllComponents, "alpha.2")]
        [DataRow(Str_Prerelease, "rc.1")]
        [DataRow(Str_Metadata, "")]
        [DataRow(Str_Basic_NoPatch, "")]
        [DataRow(Str_Basic_Patch, "")]
        [DataTestMethod]
        public void Custom_StandalonePrereleaseGroup(string version, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString("R"));
        }

        [DataRow(Str_AllComponents, 0, "alpha")]
        [DataRow(Str_AllComponents, 1, "2")]
        [DataRow(Str_Prerelease, 0, "rc")]
        [DataRow(Str_Prerelease, 1, "1")]
        [DataTestMethod]
        public void Custom_IndexedPrereleaseID_Success(string version, int index, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString($"r{index}"));
        }

        [DataRow(Str_AllComponents, 2)]
        [DataRow(Str_AllComponents, 3)]
        [DataRow(Str_Prerelease, 2)]
        [DataRow(Str_Prerelease, 3)]
        [DataTestMethod]
        public void Custom_IndexedPrereleaseID_Failure(string version, int index)
        {
            Assert.ThrowsException<FormatException>(
                () => ((SemanticVersion)version).ToString($"r{index}")
                );
        }

        [DataRow(Str_AllComponents)]
        [DataRow(Str_Prerelease)]
        [DataRow(Str_Metadata)]
        [DataRow(Str_Basic_NoPatch)]
        [DataRow(Str_Basic_Patch)]
        [DataTestMethod]
        public void Custom_Reserved_rr(string version)
        {
            Assert.ThrowsException<FormatException>(
                () => ((SemanticVersion)version).ToString("rr")
                );
        }

        [DataRow(Str_AllComponents, "20150925.f8f2cb1a")]
        [DataRow(Str_Prerelease, "")]
        [DataRow(Str_Metadata, "20150925.f8f2cb1a")]
        [DataRow(Str_Basic_NoPatch, "")]
        [DataRow(Str_Basic_Patch, "")]
        [DataTestMethod]
        public void Custom_StandaloneMetadataGroup(string version, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString("D"));
        }

        [DataRow(Str_AllComponents, "+20150925.f8f2cb1a")]
        [DataRow(Str_Prerelease, "")]
        [DataRow(Str_Metadata, "+20150925.f8f2cb1a")]
        [DataRow(Str_Basic_NoPatch, "")]
        [DataRow(Str_Basic_Patch, "")]
        [DataTestMethod]
        public void Custom_PrefixedMetadataGroup(string version, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString("DD"));
        }

        [DataRow(Str_AllComponents, 0, "20150925")]
        [DataRow(Str_AllComponents, 1, "f8f2cb1a")]
        [DataRow(Str_Metadata, 0, "20150925")]
        [DataRow(Str_Metadata, 1, "f8f2cb1a")]
        [DataTestMethod]
        public void Custom_IndexedMetadata_Success(string version, int index, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)version).ToString($"d{index}"));
        }

        [DataRow(Str_AllComponents, 2)]
        [DataRow(Str_AllComponents, 3)]
        [DataRow(Str_Metadata, 2)]
        [DataRow(Str_Metadata, 3)]
        [DataTestMethod]
        public void Custom_IndexedMetadata_Failure(string version, int index)
        {
            Assert.ThrowsException<FormatException>(
                () => ((SemanticVersion)version).ToString($"d{index}")
                );
        }

        [DataRow(Str_AllComponents)]
        [DataRow(Str_Prerelease)]
        [DataRow(Str_Metadata)]
        [DataRow(Str_Basic_NoPatch)]
        [DataRow(Str_Basic_Patch)]
        [DataTestMethod]
        public void Custom_Reserved_dd(string version)
        {
            Assert.ThrowsException<FormatException>(
                () => ((SemanticVersion)version).ToString("dd")
                );
        }

        [DataRow("M.m.pRRDD", Str_AllComponents)]
        [DataRow("  c(m)", "  v1.7-alpha.2(7)")]
        [DataRow("{{Build Date:}} d0", "Build Date: 20150925")]
        [DataRow("Build Date: d0", "Build 20150925.f8f2cb1aate: 20150925")]
        [DataRow("M.m.p ({{Alpha Release}} r1)", "1.7.0 (Alpha Release 2)")]
        [DataRow("M.m.p (Alpha Release r1)", "1.7.0 (Al0ha alpha.2elease 2)")]
        [DataTestMethod]
        public void Custom_MultipleSpecifiers(string specifiers, string output)
        {
            Assert.AreEqual(output, ((SemanticVersion)Str_AllComponents).ToString(specifiers));
        }
    }
}
