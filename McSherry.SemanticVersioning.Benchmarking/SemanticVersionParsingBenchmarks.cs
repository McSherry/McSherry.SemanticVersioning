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
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// Provides benchmarks for <see cref="SemanticVersion"/>'s parsing methods.
    /// </summary>
    [MemoryDiagnoser]
    public class SemanticVersionParsingBenchmarks
    {
        // This class provides benchmarks for all the basic Semantic Version
        // parsing functionality exposed to an end-user.
        //
        // Where [ParseMode]s are applied, they are intentionally only applied
        // on top of a basic version string. That should be sufficient to see
        // any cost they incur and keeps the benchmark number down.


        [Benchmark(Baseline = true)]
        public void SemVer_BasicParse()
            => SemanticVersion.Parse("1.10.0");

        [Benchmark]
        public void SemVer_BasicParse_AllowPrefix()
            => SemanticVersion.Parse("v1.10.0", ParseMode.AllowPrefix);

        [Benchmark]
        public void SemVer_BasicParse_OptionalPatch()
            => SemanticVersion.Parse("1.10", ParseMode.OptionalPatch);

        [Benchmark]
        public void SemVer_PreReleaseParse()
            => SemanticVersion.Parse("1.0.16-alpha.3");

        [Benchmark]
        public void SemVer_MetadataParse()
            => SemanticVersion.Parse("1.0.0+202005181554");

        [Benchmark]
        public void SemVer_BasicPreMetaParse()
            => SemanticVersion.Parse("2.19.0-rc.2+sha.5114f85");
    }
}
