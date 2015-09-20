﻿// Copyright (c) 2015 Liam McSherry
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
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace McSherry.SemVer
{
    /// <summary>
    /// <para>
    /// Provides tests for the base <see cref="SemanticVersion"/> functionality.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class SemanticVersionTests
    {
        private const string Category = "Semantic Version Base";

        /// <summary>
        /// <para>
        /// Tests that validation of build metadata items is working
        /// as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void MetadataValidation()
        {
            var validItems = new string[]
            {
                // These items are taken from the specification.
                "001",                                  // 0
                "20130313144700",                       // 1
                "exp",                                  // 2
                "sha",                                  // 3
                "5114f85",                              // 4

                // These we came up with ourselves.
                "contains-some-hyphens",                // 5
                "-leading-hyphen",                      // 6
                "trailing-hyphen-",                     // 7
                "-leading-and-trailing-",               // 8

                // The specification doesn't mention
                // leading or trailing hyphens, so we
                // have to assume they're valid (if
                // ugly).
            };

            // Iterate through all the items we expect to be valid,
            // checking that they are valid.
            for (int i = 0; i < validItems.Length; i++)
            {
                Assert.IsTrue(SemanticVersion.IsValidMetadata(validItems[i]),
                              String.Format(
                                  "Unexpected rejection: item {0} (\"{1}\").",
                                  i, validItems[i]
                                  ));
            }

            var invItems = new string[]
            {
                "",                         // 0
                null,                       // 1
                " ",                        // 2
                "infix space",              // 3
                " leading space",           // 4
                "trailing space ",          // 5
                " leading and trailing ",   // 6
                "Tür",                      // 7
                "jalapeño",                 // 8
                "çava",                     // 9
                "?",                        // 10
            };
            // Iterate through all the items we expect to be invalid.
            for (int i = 0; i < invItems.Length; i++)
            {
                Assert.IsFalse(SemanticVersion.IsValidMetadata(invItems[i]),
                               String.Format(
                                   "Unexpected acceptance: item {0} (\"{1}\").",
                                   i, invItems[i]
                                   ));
            }
        }
        /// <summary>
        /// <para>
        /// Tests that validation of pre-release identifiers works
        /// as is expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void IdentifierValidation()
        {
            // Pre-release identifiers only differ from metadata items
            // in that they cannot have leading zeroes in their numeric
            // items. This simplifies our tests somewhat.

            // A lone zero does not count as a leading zero, and must be
            // considered valid.
            Assert.IsTrue(SemanticVersion.IsValidIdentifier("0"),
                          "Unexpected rejection: single zero.");

            // Leading zeroes don't matter if the identifier is not a
            // numeric identifier.
            Assert.IsTrue(SemanticVersion.IsValidIdentifier("00nonnumber"),
                          "Unexpected rejection: non-numeric leading zero.");

            // If the identifier is numeric, then we can't have any leading
            // zeroes.
            Assert.IsFalse(SemanticVersion.IsValidIdentifier("0150"),
                           "Unexpected acceptance: numeric leading zero.");
        }
    }
}
