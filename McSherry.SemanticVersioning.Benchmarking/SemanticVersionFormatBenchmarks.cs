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
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace McSherry.SemanticVersioning
{
    [MemoryDiagnoser]
    public class SemanticVersionFormatBenchmarks
    {
        private readonly SemanticVersion SV = (SemanticVersion)"1.7.0-alpha.2+20150925.f8f2cb1a";

        // Benchmarks the fast path general format specifier
        [Benchmark(Baseline = true)]
        public string Format_General() => SV.ToString();

        // Benchmarks the equivalent of the general format specifier, but this
        // time it causes the cusstom formatter to be used rather than the fast path.
        [Benchmark]
        public string Format_GeneralEquiv() => SV.ToString("M.m.pRRDD");
    }
}
