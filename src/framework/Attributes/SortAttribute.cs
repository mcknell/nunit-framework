// ***********************************************************************
// Copyright (c) 2014 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace NUnit.Framework
{
    using System;

	/// <summary>
    /// Adding this attribute to a test, fixture, or assembly affects the sort order used by
    /// the NUnit test runner, relative to items of comparable scope. There is a property
    /// called 
    /// </summary>
    /// 
    /// <example>
    /// [TestFixture]
    /// [Sort(-1)]
    /// public class Fixture
    /// {
    ///   [Test]
    ///   [Sort(9)
    ///   public void MethodToTest()
    ///   {}
    ///   
    ///   [Test]
    ///   [Sort(9.01, Reason = "Not quite as important")]
    ///   publc void TestDescriptionMethod()
    ///   {}
    /// }
    /// </example>
    /// 
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
    public class SortAttribute : NUnitAttribute, IApplyToTest, IComparable<SortAttribute>
    {
	    /// <summary>
	    /// Default constructor
	    /// </summary>
	    public SortAttribute()
		    : this(Default.OrderBy) { }

		/// <summary>
		/// Constructor with explicit integral sort order
		/// </summary>
		public SortAttribute(long orderBy)
			: this((double)orderBy) { }

		
	    /// <summary>
	    /// Constructor with explicit sort order
	    /// </summary>
	    public SortAttribute(double orderBy)
	    {
		    if (double.IsNaN(orderBy))
		    {
			    throw new ArgumentOutOfRangeException("orderBy");
		    }
		    Order = orderBy;
	    }

	    /// <summary>
        /// Numerical basis for sort order. There are three groups. Positive values 
        /// indicate an explicit preference for priority, so they come first, in ascending order. 
        /// Zero (the default) indicates no preference, so they come next. Negative values
        /// indicate an explicity preference for finality, so they come come third, 
        /// also in ascending order. 
        /// </summary>
        public double Order { get; set; }

		/// <summary>
		/// Optional description of the motivation for the explicit sort order.
		/// </summary>
		public string Reason { get; set; }

	    /// <summary>
	    /// Compares the current object with another object of the same type.
	    /// </summary>
	    /// <returns>
	    /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
	    /// </returns>
	    /// <param name="other">An object to compare with this object.</param>
	    /// <remarks>Convention of <see cref="IComparable"/> requires that it return a value greater than 0
	    /// when compared to null, and 0 when compared to itself. </remarks>
	    public int CompareTo(SortAttribute other)
	    {
			if (ReferenceEquals(null, other))
			{
				return 1;
			}
			if (other.Equals(null)) // respect virtual Equals
			{
				return 1;
			}
			if (Equals(other))
			{
				return 0;
			}
		    return Compare(other);
	    }

		/// <summary>
		/// Templated method for the implementation-dependent portion of <see cref="IComparable{T}"/>.
		/// </summary>
		/// <param name="other">A non-null object to compare with this object.</param>
		/// <returns></returns>
		protected virtual int Compare(SortAttribute other)
		{
			if (Order == 0F)
			{
				if (other.Order == 0F)
				{
					return 0;
				}
				if (other.Order > 0)
				{
					return -1;
				}
				return 1;
			}
			if (Order > 0)
			{
				if (other.Order > 0)
				{
					return Order.CompareTo(other.Order);
				}
				return 1;
			}
			if (other.Order < 0)
			{
				return Order.CompareTo(other.Order);
			}
			return -1;
		}

		/// <summary>
		/// Collects default values for its containing class.
		/// </summary>
		public static class Default
		{
			/// <summary>
			/// Default value for <see cref="OrderBy"/>.
			/// </summary>
			public const float OrderBy = 0;

			/// <summary>
			/// Default instance.
			/// </summary>
			public static readonly SortAttribute Instance = new SortAttribute();
		}

		/// <summary>
		/// Method that implements <see cref="IComparable.CompareTo"/>.
		/// </summary>
		/// <param name="left">Basis for comparison</param>
		/// <param name="right">Object to be compared to</param>
		/// <returns></returns>
		public int Sort(ITest left, ITest right)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Modifies a test as defined for the specific attribute.
		/// </summary>
		/// <param name="test">The test to modify</param>
		public void ApplyToTest(Test test)
		{
			test.Sorter = this;
		}
    }
}
