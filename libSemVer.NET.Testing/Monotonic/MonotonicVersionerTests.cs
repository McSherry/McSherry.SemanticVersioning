// Copyright (c) 2015-16 Liam McSherry
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

namespace McSherry.SemanticVersioning.Monotonic
{
    /// <summary>
    /// <para>
    /// Provides tests for the <see cref="MonotonicVersioner"/> class.
    /// </para>
    /// </summary>
    [TestClass]
    public sealed class MonotonicVersionerTests
    {
        private const string Category = "Monotonic Versioning - Generator";

        /// <summary>
        /// <para>
        /// Tests that the constructor <see cref="MonotonicVersioner()"/>
        /// creates an instance with the expected defaults.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Create_Default()
        {
            var mv = new MonotonicVersioner();

            // Compatibility component starts at one
            Assert.AreEqual(1, mv.Compatibility);
            Assert.AreEqual(1, mv.Latest.Major);

            // Release component starts at zero
            Assert.AreEqual(0, mv.Release);
            Assert.AreEqual(0, mv.Latest.Minor);

            // Patch component is zero
            Assert.AreEqual(0, mv.Latest.Patch);

            // No identifiers or metadata
            Assert.IsFalse(mv.Latest.Identifiers.Any());
            Assert.IsFalse(mv.Latest.Metadata.Any());


            // Collection of all latest versions is populated
            Assert.IsTrue(mv.LatestVersions.Contains(
                new KeyValuePair<int, SemanticVersion>(
                    1, new SemanticVersion(1, 0)
                    )));

            Assert.AreEqual(1, mv.LatestVersions.Count);


            // Chronology contains the first version
            Assert.IsTrue(mv.Chronology.SequenceEqual(new[]
            {
                new SemanticVersion(1, 0)
            }));
        }
        /// <summary>
        /// <para>
        /// Tests that the constructor <see cref="MonotonicVersioner(bool)"/> 
        /// correctly produces an instance where the compatibility component
        /// is zero and not one.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Create_StartAtZero()
        {
            // These are pretty much the same tests as are in the
            // [Create_Default] test, but with values substituted for
            // a compatibility component that starts at zero.
            //
            // Check that test for appropriate comments.
            var mv = new MonotonicVersioner(startAtOne: false);

            Assert.AreEqual(0, mv.Compatibility);
            Assert.AreEqual(0, mv.Latest.Major);

            Assert.AreEqual(0, mv.Release);
            Assert.AreEqual(0, mv.Latest.Minor);

            Assert.AreEqual(0, mv.Latest.Patch);

            Assert.IsFalse(mv.Latest.Identifiers.Any());
            Assert.IsFalse(mv.Latest.Metadata.Any());


            Assert.IsTrue(mv.LatestVersions.Contains(
                new KeyValuePair<int, SemanticVersion>(
                    0, new SemanticVersion(0, 0)
                    )));

            Assert.AreEqual(1, mv.LatestVersions.Count);


            Assert.IsTrue(mv.Chronology.SequenceEqual(new[]
            {
                new SemanticVersion(0, 0)
            }));
        }
        /// <summary>
        /// <para>
        /// Tests that the constructor
        /// <see cref="MonotonicVersioner(bool, IEnumerable{String})"/>
        /// produces an instance which is as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Create_StartAtZeroAndMetadata()
        {
            // This constructor allows the initial version produced to
            // be produced with the specified metadata, as well as allowing
            // the caller to specify whether the compatibility sequence starts
            // at one or zero.

            var md = new[] { "abc", "123" };

            // Ensure it works when starting at one
            var mv0 = new MonotonicVersioner(
                startAtOne: true,
                metadata:   md
                );
            Assert.AreEqual(
                new SemanticVersion(1, 0, 0, new string[0], md),
                mv0.Latest
                );

            // Ensure it works when starting at zero
            var mv1 = new MonotonicVersioner(
                startAtOne: false,
                metadata:   md
                );
            Assert.AreEqual(
                new SemanticVersion(0, 0, 0, new string[0], md),
                mv1.Latest
                );
        }
        /// <summary>
        /// <para>
        /// Tests that creating a <see cref="MonotonicVersioner"/> from
        /// a specified chronology works as expected for basic valid
        /// inputs.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Create_Chronology_Valid_Basic()
        {
            // Does it work at all?
            var ch = new SemanticVersion[]
            {
                new SemanticVersion(1, 0),
                new SemanticVersion(1, 1),
                new SemanticVersion(1, 2),
            };
            var mv = new MonotonicVersioner(ch);

            Assert.AreEqual(1, mv.Compatibility);
            Assert.AreEqual(2, mv.Release);

            Assert.AreEqual(new SemanticVersion(1, 2), mv.Latest);

            Assert.AreEqual(ch.Last(), mv.LatestVersions[1]);

            Assert.IsTrue(mv.Chronology.SequenceEqual(ch));
        }
        /// <summary>
        /// <para>
        /// Tests that creation from a chronology works for complex
        /// valid inputs with multiple lines of compatibility.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Create_Chronology_Valid_Complex()
        {
            var ch = new SemanticVersion[]
            {
                /* 0 */ new SemanticVersion(1, 0),
                /* 1 */ new SemanticVersion(1, 1),
                /* 2 */ new SemanticVersion(2, 2), // Latest, line 2
                /* 3 */ new SemanticVersion(1, 3),
                /* 4 */ new SemanticVersion(3, 4),
                /* 5 */ new SemanticVersion(3, 5), // Latest, line 3
                /* 6 */ new SemanticVersion(1, 6), // Latest, line 1 & overall
            };
            var mv = new MonotonicVersioner(ch);

            // [Compatibility] provides the greatest-value component
            // yet generated by the class, not the latest component.
            Assert.AreEqual(3, mv.Compatibility);
            // [Release] is not related to [Compatibility], and provides
            // the current release number.
            Assert.AreEqual(6, mv.Release);

            // [Latest] should provide the chronologically-latest version,
            // regardless of the line of compatibility.
            Assert.AreEqual(ch.Last(), mv.Latest);

            // [LatestVersions] should, as the name might imply, provide
            // the latest version in each line of compatibility.
            Assert.AreEqual(ch[6], mv.LatestVersions[1]);
            Assert.AreEqual(ch[2], mv.LatestVersions[2]);
            Assert.AreEqual(ch[5], mv.LatestVersions[3]);
            // Make sure these are the only keys in the collection.
            Assert.AreEqual(3, mv.LatestVersions.Keys.Count());

            // The chronology must be in order.
            Assert.IsTrue(mv.Chronology.SequenceEqual(ch));
        }
        /// <summary>
        /// <para>
        /// The <see cref="MonotonicVersioner(IEnumerable{SemanticVersion})"/>
        /// constructor must be able to accept an out-of-order chronology, and
        /// this test ensures that it does.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Create_Chronology_Valid_Disordered()
        {
            var ch = new SemanticVersion[]
            {
                new SemanticVersion(2, 2),
                new SemanticVersion(1, 0),
                new SemanticVersion(1, 1),
            };
            var mv = new MonotonicVersioner(ch);

            Assert.AreEqual(2, mv.Compatibility);
            Assert.AreEqual(2, mv.Release);

            Assert.AreEqual(ch[0], mv.Latest);

            Assert.AreEqual(ch[2], mv.LatestVersions[1]);
            Assert.AreEqual(ch[0], mv.LatestVersions[2]);

            Assert.AreEqual(2, mv.LatestVersions.Keys.Count());

            Assert.IsTrue(mv.Chronology.SequenceEqual(
                ch.OrderBy(sv => sv.Minor)
                ));
        }
        /// <summary>
        /// <para>
        /// A chronology where the first line of compatibility is zero
        /// instead of one must be considered valid.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Create_Chronology_Valid_StartsAtZero()
        {
            var ch = new SemanticVersion[]
            {
                new SemanticVersion(0, 0),
                new SemanticVersion(0, 1),
                new SemanticVersion(1, 2),
                new SemanticVersion(2, 3),
            };
            var mv = new MonotonicVersioner(ch);

            Assert.AreEqual(2, mv.Compatibility);
            Assert.AreEqual(3, mv.Release);

            Assert.AreEqual(ch.Last(), mv.Latest);

            Assert.AreEqual(ch[1], mv.LatestVersions[0]);
            Assert.AreEqual(ch[2], mv.LatestVersions[1]);
            Assert.AreEqual(ch[3], mv.LatestVersions[2]);

            Assert.AreEqual(3, mv.LatestVersions.Keys.Count());

            Assert.IsTrue(mv.Chronology.SequenceEqual(ch));
        }

        /// <summary>
        /// <para>
        /// Tests the behaviour of the constructor
        /// <see cref="MonotonicVersioner(bool, IEnumerable{String})"/> in
        /// response to invalid inputs.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Create_StartAtZeroAndMetadata_Invalid()
        {
            // Collection of metadata items is null
            new Action(() => new MonotonicVersioner(true, null))
                .AssertThrows<ArgumentNullException>();

            // Item within collection of metadata items is null
            var narr = new string[] { null };
            new Action(() => new MonotonicVersioner(true, narr))
                .AssertThrows<ArgumentNullException>();


            // Metadata item is invalid
            foreach (var item in SemanticVersionTests.InvalidMetadata
                                                     .Where(m => m != null))
            {
                new Action(() => new MonotonicVersioner(true, new[] { item }))
                    .AssertThrowsExact<ArgumentException>(item);
            }
        }
        /// <summary>
        /// <para>
        /// Tests that creation with an invalid chronology fails
        /// as is expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void Create_Chronology_Invalid()
        {
            // Chronology is null
            new Action(() => new MonotonicVersioner(chronology: null))
                .AssertThrows<ArgumentNullException>();

            // Chronology has a null item
            var nullarr = new SemanticVersion[] { null };
            new Action(() => new MonotonicVersioner(nullarr))
                .AssertThrows<ArgumentNullException>();


            // Chronology contains a non-monotonic version
            var nonmon = new[] { new SemanticVersion(1, 0, 1) };
            new Action(() => new MonotonicVersioner(nonmon))
                .AssertThrows<ArgumentOutOfRangeException>();


            // Chronology is incomplete, missing a compatibility number
            var incComp = new[]
            {
                new SemanticVersion(1, 0),
                new SemanticVersion(3, 1),
            };
            new Action(() => new MonotonicVersioner(incComp))
                .AssertThrowsExact<ArgumentException>();

            // Chronology is incomplete, missing a release number
            var incRel = new[]
            {
                new SemanticVersion(1, 0),
                new SemanticVersion(1, 2),
            };
            new Action(() => new MonotonicVersioner(incRel))
                .AssertThrowsExact<ArgumentException>();

            // Chronology is incomplete, compatibility does not start at zero
            var incNonZ = new[] { new SemanticVersion(2, 0) };
            new Action(() => new MonotonicVersioner(incNonZ))
                .AssertThrowsExact<ArgumentException>();

            // Chronology is incomplete, empty collection
            var incEmpty = Enumerable.Empty<SemanticVersion>();
            new Action(() => new MonotonicVersioner(incEmpty))
                .AssertThrowsExact<ArgumentException>();
        }


        /// <summary>
        /// <para>
        /// Tests that the method to retrieve the next version accepting
        /// a <see cref="MonotonicChange"/> behaves as anticipated for
        /// correct inputs.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Next_ChangeOnly_Valid()
        {
            var mv = new MonotonicVersioner();

            // A compatible change increments only the release number
            Assert.AreEqual(
                new SemanticVersion(1, 1), mv.Next(MonotonicChange.Compatible)
                );

            // A breaking change increments both the release number and the
            // compatibility number.
            Assert.AreEqual(
                new SemanticVersion(2, 2), mv.Next(MonotonicChange.Breaking)
                );

            // A change is applied to the latest version.
            Assert.AreEqual(
                new SemanticVersion(2, 3), mv.Next(MonotonicChange.Compatible)
                );

            // A change is applied to the latest version.
            // This call makes the latest version "1.4".
            mv.Next(1, MonotonicChange.Compatible);
            // So this call should result in 1.5.
            Assert.AreEqual(
                new SemanticVersion(1, 5), mv.Next(MonotonicChange.Compatible)
                );
        }
        /// <summary>
        /// <para>
        /// Tests that the method for retrieving the next version accepting
        /// a change type and collection of metadata items will behave as
        /// expected for valid input.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Next_ChangeAndMetadata_Valid()
        {
            var mv = new MonotonicVersioner();

            // Compatible change
            var sv0 = (SemanticVersion)"1.1+abc.123";
            Assert.AreEqual(
                sv0, mv.Next(MonotonicChange.Compatible, new[] { "abc", "123" })
                );

            // Breaking change
            var sv1 = (SemanticVersion)"2.2+abc.123";
            Assert.AreEqual(
                sv1, mv.Next(MonotonicChange.Breaking, new[] { "abc", "123" })
                );

            // Change applies to latest version.
            var sv2 = (SemanticVersion)"2.3+def.456";
            Assert.AreEqual(
                sv2, mv.Next(MonotonicChange.Compatible, new[] { "def", "456" })
                );

            // Change applies to latest version.
            // This call causes the latest version to be in the 1.x line.
            mv.Next(1, MonotonicChange.Compatible);
            // Thus, this call should return another version in the 1.x line.
            var sv3 = (SemanticVersion)"1.5+ghi.789";
            Assert.AreEqual(
                sv3, mv.Next(MonotonicChange.Compatible, new[] { "ghi", "789" })
                );
        }
        /// <summary>
        /// <para>
        /// Tests that the method to retrieve the next version accepting a
        /// collection of metadata items behaves correctly for valid input.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Next_LineAndMetadata_Valid()
        {
            var mv = new MonotonicVersioner();

            var md = new[] { "abc", "123" };

            // Compatible changes
            Assert.AreEqual(
                new SemanticVersion(1, 1, 0, new string[0], md),
                mv.Next(1, MonotonicChange.Compatible, md)
                );

            // Breaking changes
            Assert.AreEqual(
                new SemanticVersion(2, 2, 0, new string[0], md),
                mv.Next(1, MonotonicChange.Breaking, md)
                );
        }
        /// <summary>
        /// <para>
        /// Tests that the method to retrieve the next version accepting both a
        /// line number and a <see cref="MonotonicChange"/> behaves correctly for
        /// valid inputs.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Next_ChangeAndLine_Valid()
        {
            var mv = new MonotonicVersioner();

            // A compatible change increments only the release number.
            Assert.AreEqual(
                new SemanticVersion(1, 1),
                mv.Next(1, MonotonicChange.Compatible)
                );

            // A breaking change increments compatibility and release numbers.
            Assert.AreEqual(
                new SemanticVersion(2, 2),
                mv.Next(1, MonotonicChange.Breaking)
                );

            // But having released a breaking change does not preclude
            // updating a previous line of compatibility.
            Assert.AreEqual(
                new SemanticVersion(1, 3),
                mv.Next(1, MonotonicChange.Compatible)
                );

            // Nor does updating an older release preclude updating a newer one.
            Assert.AreEqual(
                new SemanticVersion(2, 4),
                mv.Next(2, MonotonicChange.Compatible)
                );

            // And specifying another breaking change on an older line of
            // compatibility should increment the compatibility number.
            Assert.AreEqual(
                new SemanticVersion(3, 5),
                mv.Next(1, MonotonicChange.Breaking)
                );
        }

        /// <summary>
        /// <para>
        /// Tests the behaviour of the
        /// <see cref="MonotonicVersioner.Next(MonotonicChange)"/> method
        /// in response to invalid input.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Next_ChangeOnly_Invalid()
        {
            var mv = new MonotonicVersioner();

            // Specified change is not a recognised value
            new Action(() => mv.Next((MonotonicChange)Int32.MinValue))
                .AssertThrows<ArgumentOutOfRangeException>();
        }
        /// <summary>
        /// <para>
        /// Tests that the method <see cref="MonotonicVersioner.
        /// Next(MonotonicChange, IEnumerable{string})"/> responds in
        /// the manner expected when given invalid input.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Next_ChangeAndMetadata_Invalid()
        {
            var mv = new MonotonicVersioner();
            var valarr = new[] { "abc", "123" };

            // Change type is invalid
            new Action(() => mv.Next((MonotonicChange)Int32.MinValue, valarr))
                .AssertThrows<ArgumentOutOfRangeException>();


            // Metadata collection is null
            new Action(() => mv.Next(MonotonicChange.Compatible, null))
                .AssertThrows<ArgumentNullException>();

            // Metadata item is null
            var nularr = new string[] { null };
            new Action(() => mv.Next(MonotonicChange.Compatible, nularr))
                .AssertThrows<ArgumentNullException>();


            // Metadata item is invalid
            foreach (var item in SemanticVersionTests.InvalidMetadata
                                                     .Where(m => m != null))
            {
                var iarr = new[] { item };
                new Action(() => mv.Next(MonotonicChange.Compatible, iarr))
                    .AssertThrowsExact<ArgumentException>(item);
            }
        }
        /// <summary>
        /// <para>
        /// Tests that the method for retrieving the next version accepting
        /// a compatibility line and collection of metadata will respond to
        /// invalid input in the manner expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Next_LineAndMetadata_Invalid()
        {
            var mv = new MonotonicVersioner();
            var valarr = new string[] { "abc", "123" };
            var nularr = new string[] { null };

            // Line is negative
            new Action(() => mv.Next(-1, MonotonicChange.Compatible, valarr))
                .AssertThrows<ArgumentOutOfRangeException>();

            // Change type is not valid
            new Action(() => mv.Next(1, (MonotonicChange)Int32.MinValue, valarr))
                .AssertThrows<ArgumentOutOfRangeException>();


            // Metadata collection is null
            new Action(() => mv.Next(1, MonotonicChange.Compatible, null))
                .AssertThrows<ArgumentNullException>();

            // Item in metadata collection is null
            new Action(() => mv.Next(1, MonotonicChange.Compatible, nularr))
                .AssertThrows<ArgumentNullException>();


            // Line is not a current line of compatibility
            new Action(() => mv.Next(100, MonotonicChange.Compatible, valarr))
                .AssertThrowsExact<ArgumentException>();

            // Metadata item is invalid
            foreach (var item in SemanticVersionTests.InvalidMetadata
                                                     .Where(m => m != null))
            {
                var iarr = new[] { item };
                new Action(() => mv.Next(1, MonotonicChange.Compatible, iarr))
                    .AssertThrowsExact<ArgumentException>(item);
            }
        }
        /// <summary>
        /// <para>
        /// Tests that the method for generating the next version accepting
        /// a compatibility line number and change responds as expected for
        /// invalid inputs.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Next_ChangeAndLine_Invalid()
        {
            var mv = new MonotonicVersioner();

            // Line is negative
            new Action(() => mv.Next(-1, MonotonicChange.Compatible))
                .AssertThrows<ArgumentOutOfRangeException>();

            // Type of change is not recognised
            new Action(() => mv.Next(1, (MonotonicChange)Int32.MinValue))
                .AssertThrows<ArgumentOutOfRangeException>();


            // Line is not a line present in the versioner
            new Action(() => mv.Next(100, MonotonicChange.Compatible))
                .AssertThrowsExact<ArgumentException>();
        }


        /// <summary>
        /// <para>
        /// Tests that the <see cref="MonotonicVersioner.Clone"/> method
        /// behaves as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Clone()
        {
            var seed = new SemanticVersion[]
            {
                new SemanticVersion(1, 0),
                new SemanticVersion(1, 1),
                new SemanticVersion(1, 2),
                new SemanticVersion(2, 3),
                new SemanticVersion(2, 4),
                new SemanticVersion(3, 5),
                new SemanticVersion(1, 6),
            };

            var mv0 = new MonotonicVersioner(seed);


            // Here we go.
            var mv1 = mv0.Clone();

            // The instances should not be the same instance.
            Assert.IsFalse(object.ReferenceEquals(mv0, mv1));

            // But they should start with the same chronology.
            Assert.IsTrue(mv0.Chronology.SequenceEqual(mv1.Chronology));

            // And should have the same values for other properties.
            Assert.AreEqual(mv0.Latest, mv1.Latest);
            Assert.AreEqual(mv0.Compatibility, mv1.Compatibility);
            Assert.AreEqual(mv0.Release, mv1.Release);
            Assert.IsTrue(mv0.LatestVersions.SequenceEqual(mv1.LatestVersions));

            // But if we advance one, it shouldn't affect the other.
            var mv1_lat = mv1.Latest;
            mv0.Next(MonotonicChange.Compatible);
            Assert.AreEqual(new SemanticVersion(1, 7), mv0.Latest);
            Assert.AreEqual(mv1_lat, mv1.Latest);
            Assert.AreNotEqual(mv0.Latest, mv1.Latest);

            var mv0_lat = mv0.Latest;
            mv1.Next(MonotonicChange.Breaking);
            Assert.AreEqual(new SemanticVersion(4, 7), mv1.Latest);
            Assert.AreEqual(mv0_lat, mv0.Latest);
            Assert.AreNotEqual(mv0.Latest, mv1.Latest);
        }


        /// <summary>
        /// <para>
        /// Tests that the <see cref="MonotonicVersioner.Latest"/> property
        /// behaves as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Latest()
        {
            // [Latest] provides the chronologically-latest version
            // created by the instance. That is, the version with the
            // greatest-value release number, regardless of line of
            // compatibility.
            var mv = new MonotonicVersioner();

            // Starts at the initial version
            Assert.AreEqual((SemanticVersion)"1.0", mv.Latest);

            // Updated on a compatible change
            mv.Next(MonotonicChange.Compatible);
            Assert.AreEqual((SemanticVersion)"1.1", mv.Latest);

            // Updated on a breaking change
            mv.Next(MonotonicChange.Breaking);
            Assert.AreEqual((SemanticVersion)"2.2", mv.Latest);

            // Updating a lower-value line of compatibility produces
            // the correct result.
            mv.Next(1, MonotonicChange.Compatible);
            Assert.AreEqual((SemanticVersion)"1.3", mv.Latest);
        }

        /// <summary>
        /// <para>
        /// Tests that the <see cref="MonotonicVersioner.LatestVersions"/>
        /// property's behaviour is as expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_LatestVersions()
        {
            // The [LatestVersions] property provides a dictionary
            // where the key is the line of compatibility and the
            // version is the latest version produced in that line.
            var mv = new MonotonicVersioner();


            // Expected initial value.
            Assert.AreEqual(1, mv.LatestVersions.Count);
            Assert.IsTrue(mv.LatestVersions.ContainsKey(1));
            Assert.AreEqual((SemanticVersion)"1.0", mv.LatestVersions[1]);

            // Value updates with compatible change
            mv.Next(MonotonicChange.Compatible);

            Assert.AreEqual(1, mv.LatestVersions.Count);
            Assert.IsTrue(mv.LatestVersions.ContainsKey(1));
            Assert.AreEqual((SemanticVersion)"1.1", mv.LatestVersions[1]);

            // Value updates with breaking change
            mv.Next(MonotonicChange.Breaking);

            Assert.AreEqual(2, mv.LatestVersions.Count);
            Assert.IsTrue(mv.LatestVersions.ContainsKey(1));
            Assert.IsTrue(mv.LatestVersions.ContainsKey(2));
            Assert.AreEqual((SemanticVersion)"1.1", mv.LatestVersions[1]);
            Assert.AreEqual((SemanticVersion)"2.2", mv.LatestVersions[2]);
        }

        /// <summary>
        /// <para>
        /// Tests that the <see cref="MonotonicVersioner.Compatibility"/>
        /// property acts in the manner expected.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Compatibility()
        {
            // The [Compatibility] property gives the greatest line of
            // compatibility present in a version created by the instance.
            var mv = new MonotonicVersioner();

            // Starts off as expected
            Assert.AreEqual(1, mv.Compatibility);

            // Does not increment with a compatible change
            mv.Next(MonotonicChange.Compatible);
            Assert.AreEqual(1, mv.Compatibility);

            // Does increment with a breaking change
            mv.Next(MonotonicChange.Breaking);
            Assert.AreEqual(2, mv.Compatibility);
        }

        /// <summary>
        /// <para>
        /// Tests the behaviour of the <see cref="MonotonicVersioner.Release"/>
        /// property.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Release()
        {
            // The [Release] property tracks the current release number,
            // and is incremented with each new version generated.
            var mv = new MonotonicVersioner();

            // Starts off as expected
            Assert.AreEqual(0, mv.Release);

            // Increments with a compatible release
            mv.Next(MonotonicChange.Compatible);
            Assert.AreEqual(1, mv.Release);

            // Increments with a breaking release
            mv.Next(MonotonicChange.Breaking);
            Assert.AreEqual(2, mv.Release);
        }

        /// <summary>
        /// <para>
        /// Tests the behaviour of the <see cref="MonotonicVersioner.
        /// Chronology"/> property.
        /// </para>
        /// </summary>
        [TestMethod, TestCategory(Category)]
        public void General_Chronology()
        {
            // The [Chronology] property provides an ordered
            // record of the versions created by the instance.
            var mv = new MonotonicVersioner();

            // Chronology starts with only the initial version
            Assert.IsTrue(mv.Chronology.SequenceEqual(new[]
            {
                new SemanticVersion(1, 0),
            }));

            // Compatible change adds to the chronology
            mv.Next(MonotonicChange.Compatible);
            Assert.IsTrue(mv.Chronology.SequenceEqual(new[]
            {
                new SemanticVersion(1, 0),
                new SemanticVersion(1, 1),
            }));

            // Breaking change adds to the chronology
            mv.Next(MonotonicChange.Breaking);
            Assert.IsTrue(mv.Chronology.SequenceEqual(new[]
            {
                new SemanticVersion(1, 0),
                new SemanticVersion(1, 1),
                new SemanticVersion(2, 2),
            }));

            // A second compatible change produces a chronology
            // with the correct ordering
            mv.Next(1, MonotonicChange.Compatible);
            Assert.IsTrue(mv.Chronology.SequenceEqual(new[]
            {
                new SemanticVersion(1, 0),
                new SemanticVersion(1, 1),
                new SemanticVersion(2, 2),
                new SemanticVersion(1, 3),
            }));
        }
    }
}
