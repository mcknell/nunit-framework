﻿// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
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

using System;
using System.Reflection;

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// OneTimeSetUpCommand runs any one-time setup methods for a suite,
    /// constructing the user test object if necessary.
    /// </summary>
    public class OneTimeSetUpCommand : TestCommand
    {
        private readonly TestSuite _suite;
        private readonly Type _fixtureType;
        private readonly object[] _arguments;
        private readonly MethodInfo[] _setUpMethods;
        private readonly SetUpTearDownList _setUpTearDown;

        /// <summary>
        /// Constructs a OneTimeSetUpComand for a suite
        /// </summary>
        /// <param name="suite">The suite to which the command applies</param>
        /// <param name="setUpTearDown">A SetUpTearDownList for use by the command</param>
        public OneTimeSetUpCommand(TestSuite suite, SetUpTearDownList setUpTearDown)
            : base(suite) 
        {
            _suite = suite;
            _fixtureType = suite.FixtureType;
            _arguments = suite.Arguments;
            if (_fixtureType != null)
                _setUpMethods = Reflect.GetMethodsWithAttribute(_fixtureType, typeof(OneTimeSetUpAttribute), true);
            _setUpTearDown = setUpTearDown;
        }

        /// <summary>
        /// Overridden to run the one-time setup for a suite.
        /// </summary>
        /// <param name="context">The TestExecutionContext to be used.</param>
        /// <returns>A TestResult</returns>
        public override TestResult Execute(TestExecutionContext context)
        {
            if (_fixtureType != null)
            {
                // Use pre-constructed fixture if available, otherwise construct it
                if (!IsStaticClass(_fixtureType))
                    context.TestObject = _suite.Fixture ?? Reflect.Construct(_fixtureType, _arguments);

                _setUpTearDown.RunSetUp(context);
            }

            return context.CurrentResult;
        }

        private static bool IsStaticClass(Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }
    }
}
