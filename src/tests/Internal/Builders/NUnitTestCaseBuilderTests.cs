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
using System.Reflection;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Builders;
#endregion

namespace NUnit.Framework.Tests.Internal.Builders
{
	[TestFixture]
	public class NUnitTestCaseBuilderTests
	{
		[Test]
		public void SorterNotSetWhenNoSortAttribute()
		{
			var method = (MethodInfo)MethodBase.GetCurrentMethod();
			Assert.That(method.GetCustomAttributes(typeof(SortAttribute), true), Is.Empty, "precondition");
			TestMethod test = new NUnitTestCaseBuilder().BuildTestMethod(method, null, null);
			Assert.That(test.Sorter, Is.Null);
		}

		[Test]
		[Sort]
		public void SorterSetToSortAttribute()
		{
			var method = (MethodInfo)MethodBase.GetCurrentMethod();
			Assert.That(method.GetCustomAttributes(typeof(SortAttribute), true), Is.Not.Empty, "precondition");
			TestMethod test = new NUnitTestCaseBuilder().BuildTestMethod(method, null, null);
			Assert.That(test.Sorter, Is.Not.Null);
		}
	}
}
