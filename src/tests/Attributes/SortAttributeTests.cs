﻿// ***********************************************************************
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

#region using...
using System;
#endregion

namespace NUnit.Framework.Attributes
{
	[TestFixture]
	[Sort(1)]
	public class SortAttributeTests
	{
		private const string ExpectedReason = "because";
		private const int Before = -1;
		private const int Same = 0;
		private const int After = 1;
		private const int PositiveOrderBy = 4;
		private const int NegativeOrderBy = -3;

		[Test]
		public void CallsTemplatedMethod()
		{
			var normal = new SortAttribute();
			var saboteur = new SubSortAttribute();
			Assert.Throws<Exception>(() => saboteur.CompareTo(normal));
		}

		[Test]
		public void CompareDefaultOrderSameAsSelf()
		{
			var attribute = new SortAttribute();

			AssertFirstComparesToSecondAs(attribute, attribute, Same);
		}

		[Test]
		public void ConstructWithOrderBy()
		{
			SortAttribute attr = MakePositiveSortAttribute();
			Assert.That(attr.Order, Is.EqualTo(PositiveOrderBy));
			Assert.That(attr.Reason, Is.Null);
		}

		[Test]
		public void ConstructWithOrderByAndPropertySetter()
		{
			var attr = new SortAttribute(NegativeOrderBy) {Reason = ExpectedReason};
			Assert.That(attr.Order, Is.EqualTo(NegativeOrderBy));
			Assert.That(attr.Reason, Is.EqualTo(ExpectedReason));
		}

		[Test]
		public void ConstructWithoutArguments()
		{
			var attr = new SortAttribute();
			Assert.That(attr.Order, Is.EqualTo(SortAttribute.Default.OrderBy));
			Assert.That(attr.Reason, Is.Null);
		}

		[Test]
		public void ConstructWithoutArgumentsButPropertySetter()
		{
			var attr = new SortAttribute {Reason = ExpectedReason};
			Assert.That(attr.Order, Is.EqualTo(SortAttribute.Default.OrderBy));
			Assert.That(attr.Reason, Is.EqualTo(ExpectedReason));
		}

		[Test]
		public void Inherit()
		{
			Action<int> action = new Derived().Foo;
			object[] customAttributes = action.Method.GetCustomAttributes(typeof(SortAttribute), true);
			Assert.That(customAttributes.Length, Is.EqualTo(1));
			var sortAttribute = customAttributes[0] as SortAttribute;
			Assert.NotNull(sortAttribute);
			Assert.That(sortAttribute.Order, Is.EqualTo(Base.OldSort));
		}

		[Test]
		[Sort(3, Reason = "cuz why not")]
		public void LesserNegativeOrderComparesBeForeGreaterNegativeOrder()
		{
			SortAttribute lesser = MakeNegativeSortAttribute();

			SortAttribute greater = MakeGreaterSortAttribute(lesser);

			Assert.That(greater.Order, Is.LessThan(0), "precondition");

			AssertLessThan(lesser, greater);
		}

		[Test]
		[Sort(4, Reason = "cuz why")]
		public void LesserPositiveOrderComparesBeforeGreaterPositiveOrder()
		{
			SortAttribute lesser = MakePositiveSortAttribute();

			var greater = new SortAttribute(lesser.Order + 1);

			Assert.That(lesser.Order, Is.LessThan(greater.Order), "precondition");
			Assert.That(lesser.Order, Is.GreaterThan(0), "precondition");

			AssertLessThan(lesser, greater);
		}

		[Test]
		[Sort(-5, Reason = "late")]
		public void NegativeOrderComparesAfterZero()
		{
			SortAttribute negativeSortAttribute = MakeNegativeSortAttribute();

			var attribute = new SortAttribute();

			AssertLessThan(attribute, negativeSortAttribute);
		}

		[Test]
		[Sort(-7, Reason = "not AS late")]
		public void ObjectComparesAfterNull()
		{
			AssertFirstComparesToSecondAs(new SortAttribute(), null, After);
		}

		[Test]
		public void Override()
		{
			Action<int> action = new Overriding().Foo;
			object[] customAttributes = action.Method.GetCustomAttributes(typeof(SortAttribute), true);
			Assert.That(customAttributes.Length, Is.EqualTo(1));
			var sortAttribute = customAttributes[0] as SortAttribute;
			Assert.NotNull(sortAttribute);
			Assert.That(sortAttribute.Order, Is.EqualTo(Overriding.NewSort));
		}

		[Test]
		public void PositiveOrderCompareBeforeZero()
		{
			SortAttribute positiveSortAttribute = MakePositiveSortAttribute();

			var attribute = new SortAttribute();

			AssertLessThan(positiveSortAttribute, attribute);
		}

		private static void AssertFirstComparesToSecondAs(SortAttribute first, SortAttribute second, int expected)
		{
			Assert.That(first.CompareTo(second), Is.EqualTo(expected));
		}

		private void AssertLessThan(SortAttribute lesser, SortAttribute greater)
		{
			AssertFirstComparesToSecondAs(lesser, greater, Before);
			AssertFirstComparesToSecondAs(greater, lesser, After);
		}

		private static SortAttribute MakeGreaterSortAttribute(SortAttribute lesser)
		{
			var attribute = new SortAttribute(lesser.Order + 1);
			Assert.That(attribute.Order, Is.GreaterThan(lesser.Order), "precondition");
			return attribute;
		}

		private static SortAttribute MakeNegativeSortAttribute()
		{
			var attribute = new SortAttribute(NegativeOrderBy);
			Assert.That(attribute.Order, Is.LessThan(0), "precondition");
			return attribute;
		}

		private static SortAttribute MakePositiveSortAttribute()
		{
			var attribute = new SortAttribute(PositiveOrderBy);
			Assert.That(attribute.Order, Is.GreaterThan(0), "precondition");
			return attribute;
		}

		private class Base
		{
			public const int OldSort = 3;

			[Sort(OldSort)]
			public virtual void Foo(int bar) { }
		}

		private class Derived : Base { }

		private class Overriding : Base
		{
			public const int NewSort = -5;

			[Sort(NewSort)]
			public override void Foo(int bar) { }
		}

		private class SubSortAttribute : SortAttribute
		{
			protected override int Compare(SortAttribute other)
			{
				throw new Exception();
			}
		}
	}
}