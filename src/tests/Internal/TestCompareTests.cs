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

#region using...
using System;
using NUnit.Framework.Attributes;
#endregion

namespace NUnit.Framework.Internal
{
	[TestFixture]
	public class TestCompareTests
	{
		[Test]
		public void LeftArgumentCannotBeNull()
		{
			Assert.Throws<ArgumentNullException>(() => Test.Compare(null, new TestDummy()));
		}

		[Test]
		public void LeftSorterComparesToNullRightSorter()
		{
			AssertSpyAgainstNull(false);
		}

		[Test]
		public void LeftSorterComparesToRightSorterOnLeft()
		{
			SpySorter leftSpy;
			TestDummy leftDummy = MakeSpy(5, out leftSpy);
			SpySorter rightSpy;
			TestDummy rightDummy = MakeSpy(7, out rightSpy);
			Assert.That(leftDummy.Sorter, Is.EqualTo(leftSpy), "precondition");
			Assert.That(rightDummy.Sorter, Is.EqualTo(rightSpy), "precondition");
			Assert.That(leftSpy.Result, Is.Not.EqualTo(rightSpy.Result), "precondition");
			Assert.That(leftSpy.Result, Is.Not.EqualTo(0), "precondition");

			int result = Test.Compare(leftDummy, rightDummy);

			Assert.That(result, Is.EqualTo(leftSpy.Result));
			Assert.That(leftSpy.Other, Is.EqualTo(rightSpy));
		}

		[Test]
		public void NullLeftSorterComparesToRightSorterOnRight()
		{
			AssertSpyAgainstNull(true);
		}

		[Test]
		public void NullSorterComparesToNullSorterByFullName()
		{
			var left = new TestDummy {FullName = "a"};
			var right = new TestDummy {FullName = "b"};
			Assert.That(left.Sorter, Is.Null, "precondition");
			Assert.That(right.Sorter, Is.Null, "precondition");
			Assert.That(left.FullName.CompareTo(right.FullName), Is.LessThan(0), "precondition");

			Assert.That(Test.Compare(left, right), Is.LessThan(0));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>This is not consistent with the guidance in the MSDN documentation
		/// of <see cref="IComparable"/> but it is the historic behavior of NUnit.</remarks>
		[Test]
		public void RightArgumentNullReturnsNegativeOne()
		{
			Assert.That(Test.Compare(new TestDummy(), null), Is.EqualTo(-1));
		}

		private static void AssertSpyAgainstNull(bool flip)
		{
			const int expected = 7;
			var spySorter = new SpySorter(expected);
			var spyDummy = new TestDummy {Sorter = spySorter};
			var nullDummy = new TestDummy {FullName = "b"};
			Assert.That(spyDummy.Sorter, Is.EqualTo(spySorter), "precondition");
			Assert.That(spySorter.Other, Is.Null, "precondition");
			Assert.That(nullDummy.Sorter, Is.Null, "precondition");
			int sign = flip ? -1 : 1;
			int result;
			if (flip)
			{
				result = Test.Compare(nullDummy, spyDummy);
			}
			else
			{
				result = Test.Compare(spyDummy, nullDummy);
			}

			Assert.That(result, Is.EqualTo(expected * sign));
			Assert.That(spySorter.Other, Is.EqualTo(SortAttribute.Default.Instance));
		}

		private static TestDummy MakeSpy(int result, out SpySorter sorter)
		{
			sorter = new SpySorter(result);
			Assert.That(sorter.Other, Is.Null, "precondition");
			return new TestDummy {Sorter = sorter};
		}

		private class SpySorter : SortAttribute
		{
			internal SpySorter(int result)
			{
				Result = result;
			}

			public SortAttribute Other { get; private set; }
			public int Result { get; private set; }

			protected override int Compare(SortAttribute other)
			{
				Other = other;
				return Result;
			}
		}
	}
}
