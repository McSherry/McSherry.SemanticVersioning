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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace McSherry.SemanticVersioning
{
    /// <summary>
    /// <para>
    /// Represents the ways a comparable item may compare to another
    /// comparable item.
    /// </para>
    /// </summary>
    public enum Ordering
    {
        /// <summary>
        /// <para>
        /// The item is greater. When sorted, it comes before the
        /// other item.
        /// </para>
        /// </summary>
        Greater,
        /// <summary>
        /// <para>
        /// The item is equal. When sorted, it occurs in the same
        /// position as the other item.
        /// </para>
        /// </summary>
        Equal,
        /// <summary>
        /// <para>
        /// The item is lesser. When sorted, it comes after the
        /// other item.
        /// </para>
        /// </summary>
        Lesser,
    }

    /// <summary>
    /// Provides extension methods related to unit tests.
    /// </summary>
    public static class TestingExtensions
    {
        /// <summary>
        /// <para>
        /// Asserts that a given <see cref="Action"/> throws a specified
        /// <see cref="Exception"/> or any subclasses of that exception.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of <see cref="Exception"/> to test for.
        /// </typeparam>
        /// <param name="action">
        /// The <see cref="Action"/> to assert throws the specified
        /// <see cref="Exception"/>.
        /// </param>
        /// <param name="msg">
        /// The message to display when the assertion fails. If this 
        /// is null, a default message is displayed.
        /// </param>
        public static void AssertThrows<T>(this Action action, 
                                           string msg = null)
            where T : Exception
        {
            try
            {
                action();
            }
            // If it throws an exception that we were expecting, then
            // we can just return. It fulfilled the condition.
            catch (T)
            {
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail(String.Format(
                    "Unexpected exception thrown: {0}{1}",
                    ex.GetType().FullName,
                    String.IsNullOrEmpty(msg) ? "." : ": " + msg
                    ), ex);
            }

            Assert.Fail(msg ?? "Method did not throw an exception.");
        }
        /// <summary>
        /// <para>
        /// Asserts that a given <see cref="Action"/> throws a specified
        /// <see cref="Exception"/>, ignoring any thrown subclasses of
        /// that exception.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of <see cref="Exception"/> to test for.
        /// </typeparam>
        /// <param name="action">
        /// The <see cref="Action"/> to assert throws the specified
        /// <see cref="Exception"/>.
        /// </param>
        /// <param name="msg">
        /// The message to display when the assertion fails. If this
        /// is null, a default message is displayed.
        /// </param>
        public static void AssertThrowsExact<T>(this Action action,
                                                string msg = null)
            where T : Exception
        {
            try
            {
                action();
            }
            // The crucial difference between this and [AssertThrows]. We
            // check to make sure that the type of the caught exception is
            // exactly the same as the type of our type parameter.
            catch (T rex) when (rex.GetType() == typeof(T))
            {
                return;
            }
            catch (Exception ex)
            {
                Assert.Fail(String.Format(
                    "Unexpected exception thrown: {0}{1}",
                    ex.GetType().FullName,
                    String.IsNullOrEmpty(msg) ? "." : ": " + msg
                    ), ex);
            }

            Assert.Fail(msg ?? "Method did not throw an exception.");
        }

        /// <summary>
        /// <para>
        /// Compares the provided <see cref="IComparable{T}"/> with
        /// another object of type <typeparamref name="T"/>, and
        /// returns an <see cref="Ordering"/> indicating their
        /// relation.
        /// </para>
        /// </summary>
        /// <typeparam name="T">
        /// The type of the objects to compare.
        /// </typeparam>
        /// <param name="comparable">
        /// The object that will perform the comparison.
        /// </param>
        /// <param name="comparand">
        /// The object that <paramref name="comparable"/> will be
        /// comparing against.
        /// </param>
        /// <returns>
        /// An <see cref="Ordering"/> indicating the relation of
        /// <paramref name="comparable"/> and <paramref name="comparand"/>.
        /// </returns>
        public static Ordering CompareTo<T>(this IComparable<T> comparable,
                                            T comparand)
        {
            int comparison = comparable.CompareTo(comparand);

            if (comparison < 0)
                return Ordering.Lesser;
            else if (comparison > 0)
                return Ordering.Greater;
            else
                return Ordering.Equal;
        }
    }
}