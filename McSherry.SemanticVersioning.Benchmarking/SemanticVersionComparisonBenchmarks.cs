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
using System.Text;
using BenchmarkDotNet.Attributes;

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// Provides benchmarks for <see cref="SemanticVersion"/>'s comparison
    /// functions and operators.
    /// </summary>
    [MemoryDiagnoser]
    public class SemanticVersionComparisonBenchmarks
    {
        private SemanticVersion Basic_Low       = (SemanticVersion)"1.2.3";
        private SemanticVersion Basic_LowCompat = (SemanticVersion)"1.4.7";
        private SemanticVersion Basic_High      = (SemanticVersion)"2.3.4";
        private SemanticVersion Pre_Low         = (SemanticVersion)"1.2.3-alpha.1";
        private SemanticVersion Pre_High        = (SemanticVersion)"1.2.3-beta.2";

        public IEnumerable<SemanticVersion> ParamsSrc
        {
            get
            {
                yield return Basic_Low;
                yield return Basic_LowCompat;
                yield return Basic_High;
                yield return Pre_Low;
                yield return Pre_High;
            }
        }

        [ParamsSource(nameof(ParamsSrc))]
        public SemanticVersion A { get; set; }

        [ParamsSource(nameof(ParamsSrc))]
        public SemanticVersion B { get; set; }


        [Benchmark]
        public bool SemVer_Equals() => A == B;

        [Benchmark]
        public bool SemVer_GreaterThan() => A > B;

        [Benchmark]
        public bool SemVer_LessThan() => A < B;

        [Benchmark]
        public bool SemVer_GreaterThanOrEqual() => A >= B;

        [Benchmark]
        public bool SemVer_LessThanOrEqual() => A <= B;

        [Benchmark]
        public bool SemVer_CompatibleWith() => A.CompatibleWith(B);
    }
}
