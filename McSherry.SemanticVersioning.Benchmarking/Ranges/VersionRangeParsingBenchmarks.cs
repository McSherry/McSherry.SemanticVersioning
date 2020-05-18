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

namespace McSherry.SemanticVersioning.Ranges
{
    /// <summary>
    /// Provides benchmarks for <see cref="VersionRange"/>'s parsing methods.
    /// </summary>
    [MemoryDiagnoser]
    public class VersionRangeParsingBenchmarks
    {
        [Benchmark(Baseline = true)]
        public void VerRange_Basic()
            => VersionRange.Parse("1.2.3");

        [Benchmark]
        [Arguments("^1.2.3")]
        [Arguments("~1.2.3")]
        [Arguments(">1.2.3")]
        [Arguments("<1.2.3")]
        [Arguments(">=1.2.3")]
        [Arguments("<=1.2.3")]
        public void VerRange_PrefixOperator(string str)
            => VersionRange.Parse(str);

        [Benchmark]
        [Arguments("1.2.3 - 2.3.4")]
        public void VerRange_InfixOperator(string str)
            => VersionRange.Parse(str);

        [Benchmark]
        [Arguments("*")]
        [Arguments("x")]
        [Arguments("1.*")]
        [Arguments("1.x")]
        [Arguments("1.2.*")]
        [Arguments("1.2.x")]
        public void VerRange_WildcardOperator(string str)
            => VersionRange.Parse(str);

        [Benchmark]
        public void VerRange_MultipleComparators()
            => VersionRange.Parse(">1.2.3 <2.3.4");

        [Benchmark]
        public void VerRange_MultipleComparatorSets()
            => VersionRange.Parse(">1.2.3 <2.3.4 || 5.0.x");
    }
}
